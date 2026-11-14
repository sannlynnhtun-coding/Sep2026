using System.ComponentModel.DataAnnotations;

namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models
{
    public class QuestionRequestModel
    {
        public class QuestionCreateRequestModel
        {
            [StringLength(500)]
            [Required]
            public string QuestionText { get; set; } = string.Empty;
            public int QuestionID { get; set; } 
        }

        public class QuestionUpdateRequestModel
        {
            [StringLength(500)]
            public string? QuestionText { get;  set; }
        }
        
    }
}
