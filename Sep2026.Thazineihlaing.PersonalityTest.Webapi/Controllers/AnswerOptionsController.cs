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
            var answerOption = _db.AnswerOptions.FirstOrDefault(x => x.QuestionId == id);
            if (answerOption is null)
            {
                return NotFound("");
            }
            return Ok(answerOption);
        }

        [HttpPost]
        public IActionResult CreateAnswerOption(AnswerOptionCreateRequestModel requestModel)
        {
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
            var item = _db.AnswerOptions.FirstOrDefault(x => x.AnswerOptionId == id);

            if (item is null)
            {
                return NotFound();
            }

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

            string message = result > 0
                ? "Updating Successful."
                : "Updating Failed.";

            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAnswerOption(int id)
        {
            var answerOption = _db.AnswerOptions.FirstOrDefault(x => x.AnswerOptionId == id);

            if (answerOption is null)
            {
                return NotFound();
            }

            _db.AnswerOptions.Remove(answerOption);

            int result = _db.SaveChanges();

            string message = result > 0
                ? "Deleting Successful."
                : "Deleting Failed.";

            return Ok(message);
        }
    }
}