using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sep2026.RecipeAndPantryFinder.Controllers;
using Sep2026.RecipeAndPantryFinder.Services;
using PantryContext = Sep2026.ReceipeDb.Models.AppDbContext;

static class HttpSmoke
{
    public static async Task Run(string connectionString)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Spoonacular:BaseUrl"] = "https://api.spoonacular.com",
            ["Spoonacular:ApiKey"] = "test-only"
        });
        builder.Services.AddControllers().AddApplicationPart(typeof(SpoonacularController).Assembly);
        builder.Services.AddProblemDetails();
        builder.Services.AddDbContext<PantryContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddScoped<IRecipeService, RecipeService>();
        var upstream = new RecipeHandler();
        builder.Services.AddHttpClient<SpoonacularService>().ConfigurePrimaryHttpMessageHandler(() => upstream);
        await using var app = builder.Build();
        app.UseExceptionHandler();
        app.MapControllers();
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()), Timeout = TimeSpan.FromSeconds(15) };
            upstream.Body = """{"results":[{"id":42,"title":"Pasta","readyInMinutes":20,"servings":2}]}""";
            var search = await Get("/api/spoonacular?query=pasta%26apiKey%3Dinjected&number=1", HttpStatusCode.OK);
            Assert(search[0].GetProperty("recipeId").GetInt32() == 42, "Search mapping failed over HTTP.");
            Assert(upstream.Uri!.Query.Contains("pasta%26apiKey%3Dinjected") && !upstream.Uri.Query.Contains("test-only"), "Query or credential transport failed.");
            int calls = upstream.RequestCount;
            foreach (string invalid in new[] { "0", "101", "abc" })
                await Get("/api/spoonacular?number=" + invalid, HttpStatusCode.BadRequest);
            foreach (string invalid in new[] { "0", "-1", "abc" })
                await Get("/api/spoonacular/" + invalid, HttpStatusCode.NotFound);
            Assert(upstream.RequestCount == calls, "Invalid requests reached the upstream.");

            upstream.Body = """{"id":42,"title":"Pasta","instructions":"<p>Cook &amp; serve.</p>","extendedIngredients":[{"id":123,"name":"pasta","amount":2,"unit":"cups"}]}""";
            var detail = await Get("/api/spoonacular/42", HttpStatusCode.OK);
            Assert(detail.GetProperty("instructions").GetString() == "Cook & serve.", "Detail mapping failed over HTTP.");
            var ingredients = await Get("/api/spoonacular/42/ingredients", HttpStatusCode.OK);
            Assert(ingredients[0].GetProperty("id").GetInt32() == 123, "Ingredient mapping failed over HTTP.");
            upstream.Body = """[{"id":42,"title":"Pasta","usedIngredients":[{"name":"pasta"}],"missedIngredients":[{"name":"salt"}]}]""";
            var pantry = await Get("/api/spoonacular/pantry", HttpStatusCode.OK);
            Assert(pantry[0].GetProperty("matchedIngredientCount").GetInt32() == 1, "Pantry route failed over HTTP.");

            upstream.Status = HttpStatusCode.NotFound;
            await Get("/api/spoonacular/42", HttpStatusCode.NotFound);
            await Get("/api/spoonacular/42/ingredients", HttpStatusCode.NotFound);
            upstream.Status = HttpStatusCode.InternalServerError;
            var failure = await Get("/api/spoonacular", HttpStatusCode.InternalServerError);
            Assert(!failure.ToString().Contains("sensitive-upstream-body"), "Upstream error leaked over HTTP.");

            // The local route must resolve to the database service, not the upstream service.
            calls = upstream.RequestCount;
            var local = await Get("/api/Recipe", HttpStatusCode.OK);
            Assert(local.GetArrayLength() == 0 && upstream.RequestCount == calls, "Local CRUD route changed.");
            Console.WriteLine("PASS: loopback HTTP Spoonacular routes, validation, mappings, errors, and local-route separation.");

            async Task<JsonElement> Get(string path, HttpStatusCode expected)
            {
                using var response = await client.GetAsync(path);
                string body = await response.Content.ReadAsStringAsync();
                Assert(response.StatusCode == expected, $"{path}: expected {(int)expected}, got {(int)response.StatusCode}.");
                Assert(!body.Contains("test-only") && !body.Contains("StackTrace") && !body.Contains("HttpRequestException"), "Response exposed internal details.");
                using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "null" : body);
                return document.RootElement.Clone();
            }
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
