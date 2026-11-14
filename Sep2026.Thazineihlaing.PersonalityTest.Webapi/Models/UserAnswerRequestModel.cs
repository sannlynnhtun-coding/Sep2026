using System.ComponentModel.DataAnnotations;

namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models
{
    public class UserAnswerCreateRequestModel
    {
        [Range(1, int.MaxValue)]
        public int TestId { get; set; }

        [Range(1, int.MaxValue)]
        public int QuestionId { get; set; }

        [Range(1, int.MaxValue)]
        public int AnswerOptionId { get; set; }
    }
}