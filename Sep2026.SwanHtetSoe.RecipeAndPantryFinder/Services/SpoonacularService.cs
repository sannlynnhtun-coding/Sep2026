using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Sep2026.RecipeAndPantryFinder.Models.Response;
using Sep2026.ReceipeDb.Models;

namespace Sep2026.RecipeAndPantryFinder.Services;


public class SpoonacularService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;

    public SpoonacularService(HttpClient httpClient, IConfiguration configuration, AppDbContext db)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _db = db;
    }

    private (string BaseUrl, string ApiKey) GetConfig()
    {
        var baseUrl = _configuration["Spoonacular:BaseUrl"];
        var apiKey = _configuration["Spoonacular:ApiKey"];

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("Configure Spoonacular__BaseUrl and Spoonacular__ApiKey in the environment or another configuration provider.");
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var endpoint) || endpoint.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("Spoonacular BaseUrl must use HTTPS.");

        return (baseUrl.TrimEnd('/'), apiKey);
    }

    public async Task<List<RecipeResponseModel>> GetRecipesAsync(string? query, int number)
    {
        var (baseUrl, apiKey) = GetConfig();

        var url = $"{baseUrl}/recipes/complexSearch?number={number}&addRecipeInformation=true";
        if (!string.IsNullOrWhiteSpace(query))
        {
            url += $"&query={Uri.EscapeDataString(query)}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-api-key", apiKey);
        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Spoonacular search failed with HTTP {(int)response.StatusCode}.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("results", out var resultsEl) ||
            resultsEl.ValueKind != JsonValueKind.Array)
        {
            return new List<RecipeResponseModel>();
        }

        return resultsEl.EnumerateArray()
            .Select(r => new RecipeResponseModel
            {
                RecipeId = r.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number
                    ? idEl.GetInt32()
                    : 0,
                Title = r.TryGetProperty("title", out var tEl) ? tEl.GetString() ?? "" : "",
                ReadyInMinutes = r.TryGetProperty("readyInMinutes", out var rimEl) &&
                                 rimEl.ValueKind == JsonValueKind.Number
                    ? rimEl.GetInt32()
                    : 0,
                Servings = r.TryGetProperty("servings", out var svEl) &&
                           svEl.ValueKind == JsonValueKind.Number
                    ? svEl.GetInt32()
                    : 0
            })
            .ToList();
    }

    private async Task<string?> FetchRecipeInfoJsonAsync(int id)
    {
        var (baseUrl, apiKey) = GetConfig();
        var url = $"{baseUrl}/recipes/{id}/information";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-api-key", apiKey);
        using var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Spoonacular recipe info failed with HTTP {(int)response.StatusCode}.");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<RecipeDetailResponseModel?> GetRecipeByIdAsync(int id)
    {
        var json = await FetchRecipeInfoJsonAsync(id);
        if (json == null)
        {
            return null;
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new RecipeDetailResponseModel
        {
            RecipeId = root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number
                ? idEl.GetInt32()
                : id,
            Title = root.TryGetProperty("title", out var tEl) ? tEl.GetString() ?? "" : "",
            Instructions = StripHtml(
                root.TryGetProperty("instructions", out var insEl) ? insEl.GetString() : null),
            ReadyInMinutes = root.TryGetProperty("readyInMinutes", out var rimEl) &&
                             rimEl.ValueKind == JsonValueKind.Number
                ? rimEl.GetInt32()
                : 0,
            Servings = root.TryGetProperty("servings", out var svEl) &&
                       svEl.ValueKind == JsonValueKind.Number
                ? svEl.GetInt32()
                : 0,
            Ingredients = MapIngredients(root)
        };
    }

    public async Task<List<IngredientResponseModel>?> GetIngredientsByRecipeIdAsync(int id)
    {
        var json = await FetchRecipeInfoJsonAsync(id);
        if (json == null)
        {
            return null;
        }

        using var doc = JsonDocument.Parse(json);
        return MapIngredients(doc.RootElement);
    }

    public async Task<List<RecipeSearchResponseModel>> SearchRecipesByPantryAsync()
    {
        var pantryItems = await _db.PantryItems.ToListAsync();

        if (pantryItems.Count == 0)
        {
            throw new InvalidOperationException(
                "Pantry is empty. Please add items to your pantry first.");
        }

        var (baseUrl, apiKey) = GetConfig();

        var ingredients = string.Join(",", pantryItems.Select(p => Uri.EscapeDataString(p.ItemName.Trim())));
        var url = $"{baseUrl}/recipes/findByIngredients?ingredients={ingredients}&number=10";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-api-key", apiKey);
        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Spoonacular findByIngredients failed with HTTP {(int)response.StatusCode}.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            return new List<RecipeSearchResponseModel>();
        }

        return doc.RootElement.EnumerateArray()
            .Select(r =>
            {
                var usedNames = r.TryGetProperty("usedIngredients", out var u) && u.ValueKind == JsonValueKind.Array
                    ? u.EnumerateArray()
                        .Select(x => x.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "")
                        .ToList()
                    : new List<string>();

                var missedNames = r.TryGetProperty("missedIngredients", out var m) && m.ValueKind == JsonValueKind.Array
                    ? m.EnumerateArray()
                        .Select(x => x.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "")
                        .ToList()
                    : new List<string>();

                return new RecipeSearchResponseModel
                {
                    RecipeId = r.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number
                        ? idEl.GetInt32()
                        : 0,
                    Title = r.TryGetProperty("title", out var tEl) ? tEl.GetString() ?? "" : "",
                    MatchedIngredientCount = usedNames.Count,
                    TotalIngredientCount = usedNames.Count + missedNames.Count,
                    MissingIngredients = missedNames
                };
            })
            .OrderByDescending(x => x.MatchedIngredientCount)
            .ToList();
    }

    private static List<IngredientResponseModel> MapIngredients(JsonElement root)
    {
        if (!root.TryGetProperty("extendedIngredients", out var ingEl) ||
            ingEl.ValueKind != JsonValueKind.Array)
        {
            return new List<IngredientResponseModel>();
        }

        return ingEl.EnumerateArray()
            .Select(i => new IngredientResponseModel
            {
                Id = i.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.Number ? id.GetInt32() : 0,
                IngredientName = i.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                Amount = GetIngredientAmount(i),
                Unit = GetIngredientUnit(i)
            })
            .ToList();
    }

    private static string StripHtml(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return "";
        }

        // Replace block/list item closers and <br> with a space (single-line output).
        var withSpaces = Regex.Replace(s, @"</(li|p|div|ul|ol|h[1-6])>", " ", RegexOptions.IgnoreCase);
        withSpaces = Regex.Replace(withSpaces, @"<br\s*/?>", " ", RegexOptions.IgnoreCase);

        var noTags = Regex.Replace(withSpaces, @"<[^>]*>", "");

        // Collapse all whitespace runs (including any newlines) into a single space.
        noTags = Regex.Replace(noTags, @"\s+", " ");

        return WebUtility.HtmlDecode(noTags).Trim();
    }

    private static decimal GetIngredientAmount(JsonElement ing)
    {
        if (ing.TryGetProperty("amount", out var a) && a.ValueKind == JsonValueKind.Number)
        {
            return a.GetDecimal();
        }

        if (ing.TryGetProperty("measures", out var m) && m.ValueKind == JsonValueKind.Object)
        {
            if (m.TryGetProperty("us", out var us) && us.ValueKind == JsonValueKind.Object &&
                us.TryGetProperty("amount", out var ua) && ua.ValueKind == JsonValueKind.Number)
            {
                return ua.GetDecimal();
            }

            if (m.TryGetProperty("metric", out var met) && met.ValueKind == JsonValueKind.Object &&
                met.TryGetProperty("amount", out var ma) && ma.ValueKind == JsonValueKind.Number)
            {
                return ma.GetDecimal();
            }
        }

        return 0m;
    }

    private static string GetIngredientUnit(JsonElement ing)
    {
        if (ing.TryGetProperty("unit", out var u) && u.ValueKind == JsonValueKind.String)
        {
            return u.GetString() ?? "";
        }

        if (ing.TryGetProperty("unitLong", out var ul) && ul.ValueKind == JsonValueKind.String)
        {
            return ul.GetString() ?? "";
        }

        if (ing.TryGetProperty("measures", out var m) && m.ValueKind == JsonValueKind.Object)
        {
            if (m.TryGetProperty("us", out var us) && us.ValueKind == JsonValueKind.Object)
            {
                if (us.TryGetProperty("unitLong", out var uul) && uul.ValueKind == JsonValueKind.String)
                {
                    return uul.GetString() ?? "";
                }

                if (us.TryGetProperty("unitShort", out var uss) && uss.ValueKind == JsonValueKind.String)
                {
                    return uss.GetString() ?? "";
                }
            }

            if (m.TryGetProperty("metric", out var met) && met.ValueKind == JsonValueKind.Object &&
                met.TryGetProperty("unitLong", out var mul) && mul.ValueKind == JsonValueKind.String)
            {
                return mul.GetString() ?? "";
            }
        }

        return "";
    }
}
