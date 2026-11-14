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
            var test = _db.Tests.FirstOrDefault(x => x.TestId == requestModel.TestId);

            if (test is null)
            { return NotFound("Test not found."); }

            if (test.ResultPersonalityTypeId != null)
                return BadRequest("Test already completed.");

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

            return Ok(result > 0? "Save Answer Successful.": "Save Answer Failed.");
        }
    }
}