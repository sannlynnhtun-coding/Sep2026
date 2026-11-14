using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;
using Sep2026.Thazineihlaing.PersonalityTest.Webapi.Controllers;
using Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models;
using MagicContext = Sep2026.MagicNumberTrick.Database.Models.AppDbContext;
using PersonalityContext = Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels.AppDbContext;
using static Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models.AnswerOptionsRequestModels;

var database = "OtherApisRegression_" + Guid.NewGuid().ToString("N");
var connectionString = $"Server=.;Database={database};Integrated Security=True;TrustServerCertificate=True;";
var originalDefault = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
var originalPersonality = Environment.GetEnvironmentVariable("ConnectionStrings__PersonalityTestDb");
await using var admin = new SqlConnection("Server=.;Database=master;Integrated Security=True;TrustServerCertificate=True;");
await admin.OpenAsync();
await new SqlCommand($"CREATE DATABASE [{database}]", admin).ExecuteNonQueryAsync();
try
{
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__PersonalityTestDb", connectionString);
    using var magic = new MagicContext(new DbContextOptionsBuilder<MagicContext>().UseSqlServer(connectionString).Options);
    Assert(new SqlConnectionStringBuilder(magic.Database.GetConnectionString()).InitialCatalog == database, "Magic context replaced injected options.");
    magic.Database.GetService<IRelationalDatabaseCreator>().CreateTables();
    Assert(magic.MagicNumberCards.Count() == 7, "Magic seed/context could not query its configured database.");
    using var fallback = new MagicContext(new DbContextOptions<MagicContext>());
    Assert(new SqlConnectionStringBuilder(fallback.Database.GetConnectionString()).InitialCatalog == database, "Magic context ignored external fallback configuration.");

    using var personality = new PersonalityContext();
    personality.Database.GetService<IRelationalDatabaseCreator>().CreateTables();
    var type = new PersonalityType { TypeName = "First" };
    var otherType = new PersonalityType { TypeName = "Second" };
    var question = new Question { QuestionText = "First question" };
    var otherQuestion = new Question { QuestionText = "Second question" };
    var first = new AnswerOption { Question = question, PersonalityType = type, OptionText = "A" };
    var second = new AnswerOption { Question = otherQuestion, PersonalityType = type, OptionText = "B" };
    var third = new AnswerOption { Question = question, PersonalityType = type, OptionText = "C" };
    var test = new Test();
    personality.AddRange(first, second, third, otherType, test);
    personality.SaveChanges();
    Assert(third.AnswerOptionId != question.QuestionId, "Fixture must distinguish option and question IDs.");
    var found = new AnswerOptionsController().GetAnswerOption(third.AnswerOptionId) as OkObjectResult;
    Assert(found?.Value is AnswerOption answer && answer.AnswerOptionId == third.AnswerOptionId, "Answer option lookup used a question ID.");
    Assert(new AnswerOptionsController().CreateAnswerOption(new()
    {
        QuestionId = int.MaxValue, PersonalityTypeId = type.PersonalityTypeId, OptionText = "Missing parent"
    }) is NotFoundObjectResult, "Missing question should return 404.");
    Assert(new AnswerOptionsController().UpdateAnswerOption(third.AnswerOptionId, new()
    {
        PersonalityTypeId = int.MaxValue, OptionText = "Changed"
    }) is NotFoundObjectResult, "Missing personality type should return 404.");
    personality.Entry(third).Reload();
    Assert(third.OptionText == "C", "Rejected update changed the answer option.");
    Assert(new UserAnswersController().CreateAnswer(new()
    {
        TestId = test.TestId, QuestionId = question.QuestionId, AnswerOptionId = first.AnswerOptionId
    }) is OkObjectResult, "Valid answer failed.");
    Assert(new AnswerOptionsController().UpdateAnswerOption(first.AnswerOptionId, new()
    {
        QuestionId = otherQuestion.QuestionId
    }) is ConflictObjectResult, "Used answer option changed its question.");
    Assert(new AnswerOptionsController().UpdateAnswerOption(first.AnswerOptionId, new()
    {
        PersonalityTypeId = otherType.PersonalityTypeId
    }) is ConflictObjectResult, "Used answer option changed its scoring type.");
    Assert(new AnswerOptionsController().DeleteAnswerOption(first.AnswerOptionId) is ConflictObjectResult, "Referenced option deletion should conflict.");
    Assert(new QuestionsController().DeleteItems(question.QuestionId) is ConflictObjectResult, "Referenced question deletion should conflict.");
    Assert(new PersonalityTypesController().DeletePersonalityType(type.PersonalityTypeId) is ConflictObjectResult, "Referenced personality deletion should conflict.");
    var concurrentTest = new Test();
    personality.Tests.Add(concurrentTest);
    personality.SaveChanges();
    var answerTask = Task.Run(() => new UserAnswersController().CreateAnswer(new()
    {
        TestId = concurrentTest.TestId, QuestionId = question.QuestionId, AnswerOptionId = third.AnswerOptionId
    }));
    var deleteTask = Task.Run(() => new AnswerOptionsController().DeleteAnswerOption(third.AnswerOptionId));
    await Task.WhenAll(answerTask, deleteTask);
    Assert(answerTask.Result is OkObjectResult && deleteTask.Result is ConflictObjectResult ||
        answerTask.Result is BadRequestObjectResult && deleteTask.Result is OkObjectResult,
        "Concurrent answer/option deletion must preserve answers or reject the missing option.");
    Assert(!Valid(new AnswerOptionCreateRequestModel { QuestionId = 1, PersonalityTypeId = 1, OptionText = " " }), "Whitespace options must fail validation.");
    Assert(!Valid(new AnswerOptionCreateRequestModel { QuestionId = 1, PersonalityTypeId = 1, OptionText = new string('x', 301) }), "Oversized options must fail validation.");
    Assert(!Valid(new Sep2026.Panyooneain_Movie_Watchlist_System.Movie { Title = " ", Genre = "Drama", Status = "Planned" }), "Blank movie title must fail validation.");
    using var readers = new Sep2026.ReaderClub.Database.AppDbContextModels.AppDbContext();
    readers.Database.GetService<IRelationalDatabaseCreator>().CreateTables();
    readers.Reviewers.AddRange(new[] { 100, 200, 300 }.Select(id => new Sep2026.ReaderClub.Database.AppDbContextModels.Reviewer
    {
        ReviewerId = id, ReviewerName = "Reviewer", ProfileDescription = "Profile"
    }));
    readers.BookReviews.AddRange(new[] { 1, 2 }.Select(id => new Sep2026.ReaderClub.Database.AppDbContextModels.BookReview
    {
        BookReviewId = id, BookTitle = "Book", Author = "Author", ReviewTitle = "Review", ReviewContent = "Content", ReviewDate = DateOnly.FromDateTime(DateTime.Today)
    }));
    readers.Reviews.AddRange(
        new Sep2026.ReaderClub.Database.AppDbContextModels.Review { ReviewId = 1, ReviewerId = 100, BookReviewId = 1 },
        new Sep2026.ReaderClub.Database.AppDbContextModels.Review { ReviewId = 2, ReviewerId = 200, BookReviewId = 1 },
        new Sep2026.ReaderClub.Database.AppDbContextModels.Review { ReviewId = 3, ReviewerId = null, BookReviewId = 1 },
        new Sep2026.ReaderClub.Database.AppDbContextModels.Review { ReviewId = 4, ReviewerId = 300, BookReviewId = 2 });
    readers.SaveChanges();
    Assert(new Sep2026.ReaderClub.WebApi.Controllers.ReviewerController().DeleteReviewer(100) is OkObjectResult, "Shared reviewer deletion failed.");
    Assert(readers.BookReviews.AsNoTracking().Any(x => x.BookReviewId == 1) && readers.Reviews.Count(x => x.BookReviewId == 1) == 2,
        "Reviewer deletion removed another or anonymous review's shared content.");
    Assert(new Sep2026.ReaderClub.WebApi.Controllers.ReviewerController().DeleteReviewer(300) is OkObjectResult, "Orphan reviewer deletion failed.");
    Assert(!readers.BookReviews.AsNoTracking().Any(x => x.BookReviewId == 2), "Unshared book review cleanup was lost.");

    await using (var connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();
        await new SqlCommand("""
            CREATE TABLE Patients (PatientID int PRIMARY KEY);
            CREATE TABLE Doctors (DoctorID int PRIMARY KEY);
            CREATE TABLE Tokens (TokenID int IDENTITY PRIMARY KEY, PatientID int REFERENCES Patients(PatientID),
                DoctorID int REFERENCES Doctors(DoctorID), TokenNumber int, TokenDate date, CreatedAt datetime2 DEFAULT SYSDATETIME());
            INSERT Patients VALUES (1); INSERT Doctors VALUES (1);
            """, connection).ExecuteNonQueryAsync();
    }
    var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:DefaultConnection"] = connectionString
    }).Build();
    var databaseConnection = new Sep2026.ClinicPatientRecord.API.Data.DatabaseConnection(settings);
    var tokens = new Sep2026.ClinicPatientRecord.API.Controllers.TokensController(
        new Sep2026.ClinicPatientRecord.API.Service.TokenService(new Sep2026.ClinicPatientRecord.API.Repositories.TokenRepository(databaseConnection)));
    Assert(await tokens.Generate(new() { PatientID = int.MaxValue, DoctorID = 1 }) is NotFoundObjectResult, "Missing token patient should return 404.");
    Assert(await tokens.Generate(new() { PatientID = 1, DoctorID = int.MaxValue }) is NotFoundObjectResult, "Missing token doctor should return 404.");
    Assert(await tokens.Generate(new() { PatientID = 1, DoctorID = 1 }) is OkObjectResult, "Valid token failed.");
    var patients = new Sep2026.ClinicPatientRecord.API.Controllers.PatientsController(
        new Sep2026.ClinicPatientRecord.API.Service.PatientService(new Sep2026.ClinicPatientRecord.API.Repositories.PatientRepository(databaseConnection)));
    Assert(await patients.Delete(1) is ConflictObjectResult, "Patient deletion should retain token history.");
    await BattleshipSmoke.Run();
    Console.WriteLine("PASS: Magic configuration/seed; Personality lookup/parents/history/deletions; ReaderClub shared deletion; Clinic FK responses; request validation.");
}
finally
{
    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", originalDefault);
    Environment.SetEnvironmentVariable("ConnectionStrings__PersonalityTestDb", originalPersonality);
    SqlConnection.ClearAllPools();
    await new SqlCommand($"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]", admin).ExecuteNonQueryAsync();
}

static bool Valid(object value) => Validator.TryValidateObject(value, new(value), [], true);
static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
