namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models
{
    public class UserAnswerCreateRequestModel
    {
        public int TestId { get; set; }

        public int QuestionId { get; set; }

        public int AnswerOptionId { get; set; }
    }
}