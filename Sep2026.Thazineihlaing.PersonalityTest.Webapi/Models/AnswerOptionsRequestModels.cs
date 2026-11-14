namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models
{
    public class AnswerOptionsRequestModels
    {
            public class AnswerOptionCreateRequestModel
            {
                public int QuestionId { get; set; }

                public string OptionText { get; set; } = string.Empty;

                public int PersonalityTypeId { get; set; }
            }

            public class AnswerOptionUpdateRequestModel
            {
                public int? QuestionId { get; set; }

                public string? OptionText { get; set; }

                public int? PersonalityTypeId { get; set; }
            }
        }
    

}
