using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Sep2026.ConsoleApp3;
using Sep2026.ClinicPatientRecord.API.Controllers;
using Sep2026.ClinicPatientRecord.API.Data;
using Sep2026.RecipeAndPantryFinder.Services;
using PantryContext = Sep2026.ReceipeDb.Models.AppDbContext;

string database = "SecurityRegression_" + Guid.NewGuid().ToString("N");
string connectionString = $"Server=.;Database={database};Integrated Security=True;TrustServerCertificate=True;";
string? original = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
string fixture = Path.GetTempFileName();
await using var admin = new SqlConnection("Server=.;Database=master;Integrated Security=True;TrustServerCertificate=True;");
await admin.OpenAsync();
await new SqlCommand($"CREATE DATABASE [{database}]", admin).ExecuteNonQueryAsync();
try
{
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
    await using var db = new SqlConnection(connectionString);
    await db.OpenAsync();
    await new SqlCommand("""
        CREATE TABLE Students (Id int IDENTITY PRIMARY KEY, No nvarchar(200), Name nvarchar(200), ClassNo nvarchar(200));
        CREATE TABLE Grades (GradeId int IDENTITY PRIMARY KEY, StudentId int REFERENCES Students(Id), Subject nvarchar(200), Mark int);
        CREATE TABLE Project1Answer (QuestionId int, AnswerNo int, AnswerResult nvarchar(max));
        """, db).ExecuteNonQueryAsync();
    var students = new StudentService();
    var grades = new GradeService();
    const string payload = "O'Brien'); DELETE FROM Students;-- မြန်မာ";
    students.RegisterStudent("sentinel", "Keep", "A");
    students.RegisterStudent(payload, payload, payload);
    Assert(await Count("Students") == 2, "Student insertion executed input as SQL.");
    grades.RegisterGrade("missing' OR 1=1--", "Math", 10);
    Assert(await Count("Grades") == 0, "Student lookup was injectable.");
    grades.RegisterGrade(payload, payload, 70);
    Assert(await Count("Grades") == 1, "Literal student lookup failed.");
    students.UpdateStudent(2, payload, payload, payload);
    grades.UpdateGrade(1, payload, payload, 80);
    Assert((string)(await new SqlCommand("SELECT Subject FROM Grades WHERE GradeId = 1", db).ExecuteScalarAsync())! == payload,
        "Grade update did not store literal Unicode input.");
    Assert((string)(await new SqlCommand("SELECT Name FROM Students WHERE Id = 1", db).ExecuteScalarAsync())! == "Keep",
        "An unrelated student was modified.");
    students.DeleteStudent(2);
    Assert(await Count("Students") == 2, "Dependent grade protection was lost.");
    grades.DeleteGrade(1);
    students.DeleteStudent(2);
    Assert(await Count("Students") == 1, "Parameterized deletion failed.");

    await File.WriteAllTextAsync(fixture, JsonSerializer.Serialize(new { answers = new[] { new { questionNo = 1, answerNo = 1, answerResult = payload } } }));
    var root = new DirectoryInfo(AppContext.BaseDirectory);
    while (!File.Exists(Path.Combine(root.FullName, "Sep2026.slnx"))) root = root.Parent ?? throw new Exception("Repository root not found.");
    var start = new ProcessStartInfo("dotnet") { WorkingDirectory = root.FullName, RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (string argument in new[] { "run", "--no-launch-profile", "--project", "Sep2026.ProjectIdea1", "--", fixture }) start.ArgumentList.Add(argument);
    using var importer = Process.Start(start)!;
    var output = importer.StandardOutput.ReadToEndAsync();
    var errors = importer.StandardError.ReadToEndAsync();
    await importer.WaitForExitAsync();
    Assert(importer.ExitCode == 0, "Importer failed: " + await output + await errors);
    Assert((string)(await new SqlCommand("SELECT AnswerResult FROM Project1Answer", db).ExecuteScalarAsync())! == payload,
        "Importer did not preserve literal input.");
    Assert(await Count("Students") == 1, "Importer executed the injection payload.");
    await new SqlCommand("ALTER TABLE Project1Answer ADD CONSTRAINT CK_TestAnswerNumber CHECK (AnswerNo > 0)", db).ExecuteNonQueryAsync();
    await File.WriteAllTextAsync(fixture, JsonSerializer.Serialize(new { answers = new[]
    {
        new { questionNo = 2, answerNo = 1, answerResult = "valid" },
        new { questionNo = 2, answerNo = -1, answerResult = "invalid" }
    } }));
    using var failedImport = Process.Start(start)!;
    var failedOutput = failedImport.StandardOutput.ReadToEndAsync();
    var failedErrors = failedImport.StandardError.ReadToEndAsync();
    await failedImport.WaitForExitAsync();
    await Task.WhenAll(failedOutput, failedErrors);
    Assert(failedImport.ExitCode != 0 && await Count("Project1Answer") == 1, "Failed importer batch left partial data.");


    var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Spoonacular:BaseUrl"] = "https://api.spoonacular.com",
        ["Spoonacular:ApiKey"] = "test-only",
        ["ConnectionStrings:DefaultConnection"] = connectionString
    }).Build();
    using var pantry = new PantryContext(new DbContextOptionsBuilder<PantryContext>().UseSqlServer(connectionString).Options);
    var handler = new RecipeHandler();
    using var client = new HttpClient(handler);
    var recipes = new SpoonacularService(client, settings, pantry);
    await recipes.GetRecipesAsync("pasta&apiKey=injected", 10);
    Assert(handler.Uri!.Query.Contains("pasta%26apiKey%3Dinjected") && !handler.Uri.Query.Contains("test-only"), "Recipe key or query leaked into the URL.");
    handler.Body = """{"results":[{"id":42,"title":"Pasta","readyInMinutes":20,"servings":2}]}""";
    var found = await recipes.GetRecipesAsync("pasta", 1);
    Assert(found.Single().RecipeId == 42 && found[0].Title == "Pasta", "Upstream search mapping failed.");
    handler.Body = """{"id":42,"title":"Pasta","instructions":"<p>Cook &amp; serve.</p>","extendedIngredients":[{"id":123,"name":"pasta","amount":2,"unit":"cups"}]}""";
    var detail = await recipes.GetRecipeByIdAsync(42);
    Assert(detail!.Instructions == "Cook & serve." && detail.Ingredients.Single().Amount == 2, "Recipe detail mapping failed.");
    var ingredients = await recipes.GetIngredientsByRecipeIdAsync(42);
    Assert(ingredients?.Single() is { IngredientName: "pasta", Id: 123 }, "Ingredient mapping failed.");
    handler.Status = HttpStatusCode.NotFound;
    Assert(await recipes.GetRecipeByIdAsync(42) == null && await recipes.GetIngredientsByRecipeIdAsync(42) == null,
        "Upstream 404 must remain not found.");
    handler.Status = HttpStatusCode.OK;
    await new SqlCommand("CREATE TABLE PantryItems (PantryItemId int PRIMARY KEY, ItemName nvarchar(100), Quantity decimal(10,2), Unit nvarchar(50)); INSERT PantryItems VALUES (1, 'pasta', 1, 'cup');", db).ExecuteNonQueryAsync();
    handler.Body = """[{"id":42,"title":"Pasta","usedIngredients":[{"name":"pasta"}],"missedIngredients":[{"name":"salt"}]}]""";
    var matched = await recipes.SearchRecipesByPantryAsync();
    Assert(matched.Single().MatchedIngredientCount == 1 && matched[0].MissingIngredients.Single() == "salt", "Pantry search mapping failed.");
    handler.Status = HttpStatusCode.InternalServerError;
    try { await recipes.GetRecipeByIdAsync(1); throw new Exception("Upstream failure was accepted."); }
    catch (HttpRequestException ex) { Assert(!ex.Message.Contains("sensitive-upstream-body"), "Upstream body leaked."); }
    settings["Spoonacular:BaseUrl"] = "http://api.spoonacular.com";
    try { await recipes.GetRecipesAsync(null, 10); throw new Exception("Insecure upstream accepted."); }
    catch (InvalidOperationException ex) { Assert(ex.Message.Contains("HTTPS"), "Unexpected configuration failure."); }
    var diagnostic = new DatabaseTestController(new DatabaseConnection(settings));
    Assert(await diagnostic.TestConnection(new TestEnvironment()) is NotFoundResult, "Production diagnostic must be hidden.");
    await new SqlCommand("CREATE TABLE Recipes (RecipeId int IDENTITY PRIMARY KEY, Title nvarchar(200), Instructions nvarchar(max), ReadyInMinutes int, Servings int); CREATE TABLE RecipeIngredients (Id int IDENTITY PRIMARY KEY, RecipeId int REFERENCES Recipes(RecipeId), IngredientName nvarchar(100), Amount decimal(10,2), Unit nvarchar(50));", db).ExecuteNonQueryAsync();
    await PersonalitySmoke.Run(connectionString);
    await new SqlCommand("INSERT PantryItems VALUES (2, ' ', 0, 'cup'); DELETE PantryItems WHERE PantryItemId = 1;", db).ExecuteNonQueryAsync();
    var localRecipes = new RecipeService(pantry);
    int emptyPantryCalls = handler.RequestCount;
    Assert((await localRecipes.SearchRecipesByPantryAsync()).Count == 0 && (await recipes.SearchRecipesByPantryAsync()).Count == 0 && handler.RequestCount == emptyPantryCalls,
        "Blank-only pantry must return no matches without an upstream request.");
    await new SqlCommand("INSERT PantryItems VALUES (1, 'pasta', 1, 'cup')", db).ExecuteNonQueryAsync();
    await new SqlCommand("DECLARE @recipeId int; INSERT Recipes (Title, Instructions, ReadyInMinutes, Servings) VALUES ('Rice dish', 'Cook', 1, 1); SET @recipeId = SCOPE_IDENTITY(); INSERT RecipeIngredients VALUES (@recipeId, 'RICE', 1, 'cup'); UPDATE PantryItems SET ItemName = 'rice' WHERE PantryItemId = 1;", db).ExecuteNonQueryAsync();
    pantry.ChangeTracker.Clear();
    var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
    try
    {
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
        Assert((await localRecipes.SearchRecipesByPantryAsync()).Single().MatchedIngredientCount == 1, "Pantry matching depended on current culture.");
    }
    finally
    {
        System.Globalization.CultureInfo.CurrentCulture = originalCulture;
    }
    await new SqlCommand("DELETE RecipeIngredients; DELETE Recipes; UPDATE PantryItems SET ItemName = 'pasta' WHERE PantryItemId = 1;", db).ExecuteNonQueryAsync();

    await HttpSmoke.Run(connectionString);
    await HttpSmoke.Run(connectionString, useHttps: true);
    Console.WriteLine("PASS: SQL injection payloads, importer, recipe credential transport/errors, production diagnostic.");

    async Task<int> Count(string table) => Convert.ToInt32(await new SqlCommand("SELECT COUNT(*) FROM " + table, db).ExecuteScalarAsync());
}
finally
{
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", original);
    File.Delete(fixture);
    SqlConnection.ClearAllPools();
    await new SqlCommand($"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]", admin).ExecuteNonQueryAsync();
}

static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }

sealed class RecipeHandler : HttpMessageHandler
{
    public int RequestCount { get; private set; }
    public Uri? Uri { get; private set; }
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public string Body { get; set; } = "{\"results\":[]}";
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        if (request.Headers.GetValues("x-api-key").Single() != "test-only") throw new Exception("Missing key header.");
        Uri = request.RequestUri;
        return Task.FromResult(new HttpResponseMessage(Status) { Content = new StringContent(Status == HttpStatusCode.OK ? Body : "sensitive-upstream-body") });
    }
}

sealed class TestEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Production";
    public string ApplicationName { get; set; } = "Regression";
    public string ContentRootPath { get; set; } = ".";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = ".";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
}
