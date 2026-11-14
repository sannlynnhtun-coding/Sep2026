
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;

namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private AppDbContext _db;

        public TestsController()
        {
            _db = new AppDbContext();
        }

        [HttpPost("start")]
        public IActionResult StartTest()
        {
            Test test = new Test();

            _db.Tests.Add(test);
            _db.SaveChanges();

            return Ok(new
            {
                Message = "Test Started Successfully",
                TestId = test.TestId
            });
        }

        [HttpPost("{id}/submit")]
        public IActionResult SubmitTest(int id)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT TestId FROM [Test] WITH (UPDLOCK, HOLDLOCK) WHERE TestId = {id}");
            var test = _db.Tests.FirstOrDefault(x => x.TestId == id);

            if (test == null)
            {
                return NotFound("Test not found.");
            }

            if (test.ResultPersonalityTypeId != null)
            {
                return BadRequest("Test already completed.");
            }

            int totalQuestions = _db.Questions.Count();


            var answers = _db.UserAnswers.Include(x => x.AnswerOption).Where(x => x.TestId == id).ToList();

           
            if (answers.Count != totalQuestions || answers.Select(x => x.QuestionId).Distinct().Count() != totalQuestions)
            {
                return BadRequest(
                    $"Please answer all questions. Answered: {answers.Count}/{totalQuestions}"
                );
            }

           
            var result = answers.GroupBy(x => x.AnswerOption.PersonalityTypeId).Select(g => new
                {
                    PersonalityTypeId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.PersonalityTypeId)
                .FirstOrDefault();

            if (result == null)
            {
                return BadRequest("Unable to calculate result.");
            }


            test.ResultPersonalityTypeId = result.PersonalityTypeId;

            _db.SaveChanges();
            transaction.Commit();

            var personality = _db.PersonalityTypes
                .FirstOrDefault(x =>
                    x.PersonalityTypeId == result.PersonalityTypeId);

            return Ok(new
            {
                Message = "Test Completed Successfully",
                TestId = id,
                PersonalityTypeId = personality!.PersonalityTypeId,
                TypeName = personality.TypeName,
                Description = personality.Description,
                AnswerCount = answers.Count
            });
        }

       
        [HttpGet("{id}/result")]
        public IActionResult GetResult(int id)
        {
            var test = _db.Tests
                .Include(x => x.ResultPersonalityType)
                .FirstOrDefault(x => x.TestId == id);

            if (test == null)
            {
                return NotFound("Test not found.");
            }

            if (test.ResultPersonalityTypeId == null)
            {
                return BadRequest("Test has not been completed yet.");
            }

            return Ok(new
            {
                TestId = test.TestId,
                PersonalityTypeId = test.ResultPersonalityType!.PersonalityTypeId,
                TypeName = test.ResultPersonalityType.TypeName,
                Description = test.ResultPersonalityType.Description
            });
        }
    }
}
