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
            public string? Name { get; set; }
            public decimal HourlyPrice { get; set; }

        }
        public class UpdateRequest 
        {
            public string? Name { get; set; }
            public decimal? HourlyPrice { get; set; }
        }
    }
}
