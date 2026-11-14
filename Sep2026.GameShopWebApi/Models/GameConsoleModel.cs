using System.ComponentModel.DataAnnotations;

namespace Sep2026.GameShopWebApi.Models
{
    public class GameConsoleModel
    {
        public class Response
        {
            public int Id { get; set; }
            public string? ConsoleNumber { get; set; }
            public int CategoryId { get; set; }
            public bool IsDeleted { get; set; }
            public string? CurrentStatus { get; set; }
        }
        public class UpdateRequest
        {
            [StringLength(20, MinimumLength = 1)]
            public string? ConsoleNumber { get; set; }
            [Range(1, int.MaxValue)]
            public int? CategoryId { get; set; }
            [StringLength(20)]
            public string? CurrentStatus { get; set; }
        }
        public class CreateRequest
        {
            [Range(1, int.MaxValue)]
            public int CategoryId { get; set; } = 0;
        }

        public enum CurrentStatusTypes
        {
            Available,
            Taken,
            Unavailable
        }
    }
}
