namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models
{
    public class QuestionRequestModel
    {
        public class QuestionCreateRequestModel
        {
            public string QuestionText { get; set; } = string.Empty;
            public int QuestionID { get; set; } 
        }

        public class QuestionUpdateRequestModel
        {
            public string? QuestionText { get;  set; }
        }
        
    }
}
