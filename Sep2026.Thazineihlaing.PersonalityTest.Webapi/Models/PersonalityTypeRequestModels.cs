namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models
{
    public class PersonalityTypeRequestModels
    {

            public class PersonalityTypeCreateRequestModel
            {
                public string TypeName { get; set; } = string.Empty;
                public string? Description { get; set; }
            }

            public class PersonalityTypeUpdateRequestModel
            {
                public string? TypeName { get; set; }
                public string? Description { get; set; }
            }
        }

}
