using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;
using static Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models.AnswerOptionsRequestModels;

namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerOptionsController : ControllerBase
    {
        private AppDbContext _db;

        public AnswerOptionsController()
        {
            _db = new AppDbContext();
        }

        [HttpGet]
        public IActionResult GetAllAnswerOptions()
        {
            var lst = _db.AnswerOptions.ToList();
            return Ok(lst);
        }

        [HttpGet("Edit/{id}")]
        public IActionResult GetAnswerOption(int id)
        {
            var answerOption = _db.AnswerOptions.FirstOrDefault(x => x.AnswerOptionId == id);
            if (answerOption is null)
            {
                return NotFound("");
            }
            return Ok(answerOption);
        }

        [HttpPost]
        public IActionResult CreateAnswerOption(AnswerOptionCreateRequestModel requestModel)
        {
            if (!_db.Questions.Any(x => x.QuestionId == requestModel.QuestionId) ||
                !_db.PersonalityTypes.Any(x => x.PersonalityTypeId == requestModel.PersonalityTypeId))
                return NotFound("Question or personality type not found.");

            _db.AnswerOptions.Add(new AnswerOption
            {
                QuestionId = requestModel.QuestionId,
                OptionText = requestModel.OptionText.Trim(),
                PersonalityTypeId = requestModel.PersonalityTypeId
            });

            int result = _db.SaveChanges();

            string message = result > 0
                ? "Create Answer Option Successful"
                : "Create Answer Option Failed.";

            return Ok(message);
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateAnswerOption(int id, AnswerOptionUpdateRequestModel requestModel)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT AnswerOptionId FROM AnswerOptions WITH (XLOCK, HOLDLOCK) WHERE AnswerOptionId = {id}");
            var item = _db.AnswerOptions.FirstOrDefault(x => x.AnswerOptionId == id);

            if (item is null)
            {
                return NotFound();
            }

            if (requestModel.QuestionId.HasValue && !_db.Questions.Any(x => x.QuestionId == requestModel.QuestionId.Value) ||
                requestModel.PersonalityTypeId.HasValue && !_db.PersonalityTypes.Any(x => x.PersonalityTypeId == requestModel.PersonalityTypeId.Value))
                return NotFound("Question or personality type not found.");

            if ((requestModel.QuestionId.HasValue && requestModel.QuestionId.Value != item.QuestionId ||
                 requestModel.PersonalityTypeId.HasValue && requestModel.PersonalityTypeId.Value != item.PersonalityTypeId) &&
                _db.UserAnswers.Any(x => x.AnswerOptionId == id))
                return Conflict("Answered options cannot change question or personality type.");

            if (requestModel.QuestionId != null)
            {
                item.QuestionId = requestModel.QuestionId.Value;
            }

            if (requestModel.OptionText != null && !string.IsNullOrWhiteSpace(requestModel.OptionText))
            {
                item.OptionText = requestModel.OptionText.Trim();
            }

            if (requestModel.PersonalityTypeId != null)
            {
                item.PersonalityTypeId = requestModel.PersonalityTypeId.Value;
            }

            int result = _db.SaveChanges();
            transaction.Commit();

            string message = result > 0
                ? "Updating Successful."
                : "Updating Failed.";

            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAnswerOption(int id)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT AnswerOptionId FROM AnswerOptions WITH (XLOCK, HOLDLOCK) WHERE AnswerOptionId = {id}");
            var answerOption = _db.AnswerOptions.FirstOrDefault(x => x.AnswerOptionId == id);

            if (answerOption is null)
            {
                return NotFound();
            }

            if (_db.UserAnswers.Any(x => x.AnswerOptionId == id))
                return Conflict("Answer option has recorded answers.");

            _db.AnswerOptions.Remove(answerOption);

            int result = _db.SaveChanges();
            transaction.Commit();

            string message = result > 0
                ? "Deleting Successful."
                : "Deleting Failed.";

            return Ok(message);
        }
    }
}