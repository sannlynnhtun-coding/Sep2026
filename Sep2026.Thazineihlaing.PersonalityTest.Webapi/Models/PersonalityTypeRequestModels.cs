using System.ComponentModel.DataAnnotations;

namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models
{
    public class PersonalityTypeRequestModels
    {

            public class PersonalityTypeCreateRequestModel
            {
                [StringLength(100)]
                [Required]
                public string TypeName { get; set; } = string.Empty;
                [StringLength(500)]
                public string? Description { get; set; }
            }

            public class PersonalityTypeUpdateRequestModel
            {
                [StringLength(100)]
                public string? TypeName { get; set; }
                [StringLength(500)]
                public string? Description { get; set; }
            }
        }

}
