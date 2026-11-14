using System.ComponentModel.DataAnnotations;

namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models
{
    public class AnswerOptionsRequestModels
    {
            public class AnswerOptionCreateRequestModel
            {
                [Range(1, int.MaxValue)]
                public int QuestionId { get; set; }

                [StringLength(300)]
                [Required]
                public string OptionText { get; set; } = string.Empty;

                [Range(1, int.MaxValue)]
                public int PersonalityTypeId { get; set; }
            }

            public class AnswerOptionUpdateRequestModel
            {
                [Range(1, int.MaxValue)]
                public int? QuestionId { get; set; }

                [StringLength(300)]
                public string? OptionText { get; set; }

                [Range(1, int.MaxValue)]
                public int? PersonalityTypeId { get; set; }
            }
        }
    

}
