using System.ComponentModel.DataAnnotations;

namespace Sep2026.GameShopWebApi.Models
{
    public class GamesModel
    {
        public class Response
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public int? CategoryId { get; set; }
            public int TotalStock { get; set; }
            public int AvailableStock { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class CreateRequest
        {
            [Required]
            [StringLength(150)]
            public string? Title { get; set; }
            [Range(1, int.MaxValue)]
            public int? CategoryId { get; set; }
            [Range(1, int.MaxValue)]
            public int TotalStock { get; set; }
        }

        public class UpdateRequest
        {
            [StringLength(150, MinimumLength = 1)]
            public string? Title { get; set; }
            [Range(1, int.MaxValue)]
            public int? CategoryId { get; set; }
            [Range(0, int.MaxValue)]
            public int? TotalStock { get; set; }
            [Range(0, int.MaxValue)]
            public int? AvailableStock { get; set; }
        }
    }
}
