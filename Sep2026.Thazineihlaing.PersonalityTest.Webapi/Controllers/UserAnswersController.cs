using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;
using Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models;

namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAnswersController : ControllerBase
    {
        private AppDbContext _db;

        public UserAnswersController()
        {
            _db = new AppDbContext();
        }

        [HttpPost]
        public IActionResult CreateAnswer(UserAnswerCreateRequestModel requestModel)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT TestId FROM [Test] WITH (UPDLOCK, HOLDLOCK) WHERE TestId = {requestModel.TestId}");
            var test = _db.Tests.FirstOrDefault(x => x.TestId == requestModel.TestId);

            if (test is null)
            { return NotFound("Test not found."); }

            if (test.ResultPersonalityTypeId != null)
                return BadRequest("Test already completed.");

            if (_db.UserAnswers.Any(x => x.TestId == requestModel.TestId && x.QuestionId == requestModel.QuestionId))
                return Conflict("This question already has an answer for this test.");

            _db.Database.ExecuteSqlInterpolated($"SELECT AnswerOptionId FROM AnswerOptions WITH (HOLDLOCK) WHERE AnswerOptionId = {requestModel.AnswerOptionId}");
            var answer = _db.AnswerOptions.FirstOrDefault(
                x => x.AnswerOptionId == requestModel.AnswerOptionId
                && x.QuestionId == requestModel.QuestionId);

            if (answer is null)
            { return BadRequest("Invalid answer option."); }

            var userAnswer = new UserAnswer
            {
                TestId = requestModel.TestId,
                QuestionId = requestModel.QuestionId,
                AnswerOptionId = requestModel.AnswerOptionId
            };

            _db.UserAnswers.Add(userAnswer);

            int result = _db.SaveChanges();
            transaction.Commit();

            return Ok(result > 0? "Save Answer Successful.": "Save Answer Failed.");
        }
    }
}