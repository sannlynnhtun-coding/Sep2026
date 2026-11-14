using System.ComponentModel.DataAnnotations;

using System.Security.Permissions;

namespace Sep2026.GameShopWebApi.Models
{
    public class ConsoleCategoryModel
    {
        public class Response
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public decimal HourlyPrice { get; set; }
            public bool IsDeleted { get; set; }
        }
        public class CreateRequest
        {
            [Required]
            [StringLength(50)]
            public string? Name { get; set; }
            [Range(typeof(decimal), "5000", "9999999999999999.99")]
            public decimal HourlyPrice { get; set; }

        }
        public class UpdateRequest 
        {
            [StringLength(50, MinimumLength = 1)]
            public string? Name { get; set; }
            [Range(typeof(decimal), "5000", "9999999999999999.99")]
            public decimal? HourlyPrice { get; set; }
        }
    }
}
