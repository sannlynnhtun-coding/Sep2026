using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;
using Sep2026.Thazineihlaing.PersonalityTest.Webapi.Controllers;
using Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models;

static class PersonalitySmoke
{
    public static async Task Run(string connectionString)
    {
        string? original = Environment.GetEnvironmentVariable("ConnectionStrings__PersonalityTestDb");
        Environment.SetEnvironmentVariable("ConnectionStrings__PersonalityTestDb", connectionString);
        try
        {
            using var db = new AppDbContext();
            db.Database.GetService<IRelationalDatabaseCreator>().CreateTables();
            var type = new PersonalityType { TypeName = "Test type" };
            var first = new Question { QuestionText = "First?" };
            var second = new Question { QuestionText = "Second?" };
            var option1 = new AnswerOption { Question = first, PersonalityType = type, OptionText = "A" };
            var option2 = new AnswerOption { Question = second, PersonalityType = type, OptionText = "B" };
            var test = new Test();
            db.AddRange(option1, option2, test);
            db.SaveChanges();
            var request1 = new UserAnswerCreateRequestModel { TestId = test.TestId, QuestionId = first.QuestionId, AnswerOptionId = option1.AnswerOptionId };
            var request2 = new UserAnswerCreateRequestModel { TestId = test.TestId, QuestionId = second.QuestionId, AnswerOptionId = option2.AnswerOptionId };
            Assert(new UserAnswersController().CreateAnswer(request1) is OkObjectResult, "First answer failed.");
            Assert(new UserAnswersController().CreateAnswer(request1) is ConflictObjectResult, "Duplicate answer must return conflict.");
            Assert(new TestsController().SubmitTest(test.TestId) is BadRequestObjectResult, "Incomplete test was accepted.");
            var answers = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => Task.Run(() => new UserAnswersController().CreateAnswer(request2))));
            Assert(answers.Count(r => r is OkObjectResult) == 1 && answers.Count(r => r is ConflictObjectResult) == 1,
                "Concurrent answers must create exactly one answer and one conflict.");
            var submits = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => Task.Run(() => new TestsController().SubmitTest(test.TestId))));
            Assert(submits.Count(r => r is OkObjectResult) == 1 && submits.Count(r => r is BadRequestObjectResult) == 1,
                "Concurrent submission must complete only once.");
            Assert(new UserAnswersController().CreateAnswer(request2) is BadRequestObjectResult, "Completed test accepted another answer.");
            db.Entry(test).Reload();
            Assert(test.ResultPersonalityTypeId == type.PersonalityTypeId && db.UserAnswers.Count() == 2, "Unexpected score or answer count.");

            // Older databases without the unique index must not score duplicate question rows.
            db.Database.ExecuteSqlRaw("DROP INDEX UQ_UserAnswer_Test_Question ON UserAnswer");
            var legacy = new Test();
            db.Tests.Add(legacy);
            db.SaveChanges();
            db.UserAnswers.AddRange(Enumerable.Range(0, 2).Select(_ => new UserAnswer
            {
                TestId = legacy.TestId, QuestionId = first.QuestionId, AnswerOptionId = option1.AnswerOptionId
            }));
            db.SaveChanges();
            Assert(new TestsController().SubmitTest(legacy.TestId) is BadRequestObjectResult, "Duplicate rows completed an unanswered test.");
            Console.WriteLine("PASS: duplicate answers, concurrent answers/submissions, completion guards, and legacy duplicate rows.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__PersonalityTestDb", original);
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
