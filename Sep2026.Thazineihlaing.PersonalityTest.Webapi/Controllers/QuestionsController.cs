
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;
using static Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models.QuestionRequestModel;

namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private AppDbContext _db;

        public QuestionsController()
        {
            _db = new AppDbContext();
        }

        [HttpGet]
        public IActionResult GetAllQuestions()
        {
            var lst = _db.Questions.ToList();
            return Ok(lst);
        }

        [HttpGet("Edit/{id}")]
        public IActionResult GetQuestion(int id)
        {
            var question = _db.Questions.FirstOrDefault(x => x.QuestionId == id );
            if (question is null)
            {
                return NotFound("");
            }
            return Ok(question);
        }



        [HttpPost]
        public IActionResult CreateQuestion(QuestionCreateRequestModel requestModel)
        {
            _db.Questions.Add(new Question
            {
                QuestionText = requestModel.QuestionText.Trim(),

            });
            int result = _db.SaveChanges();
            string message = result > 0 ? "Create Question Successful" : "Create Question Failed.";
            return Ok(message);
        }



        [HttpPatch("{id}")]
        public IActionResult UpdateQuestion(int id, QuestionUpdateRequestModel requestModel)
        {
            var item = _db.Questions.FirstOrDefault(x => x.QuestionId == id);

            if (item is null)
            {
                return NotFound();
            }

            if (requestModel.QuestionText !=null && !requestModel.QuestionText.IsNullOrEmpty())
            {
                item.QuestionText = requestModel.QuestionText.Trim();
            }

            int result = _db.SaveChanges();

            string message = result > 0? "Updating Successful.": "Updating Failed.";

            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteItems(int id)
        {
            var question = _db.Questions.FirstOrDefault(x => x.QuestionId == id);
            if (question is null)
            { return NotFound(); }

            if (_db.AnswerOptions.Any(x => x.QuestionId == id) || _db.UserAnswers.Any(x => x.QuestionId == id))
                return Conflict("Question has answer options or recorded answers.");

            _db.Questions.Remove(question);
            int result = _db.SaveChanges();
            string message = result > 0 ? "Deleting successful" : "Deleting Fail";
            return Ok(message);

        }



    }
}
