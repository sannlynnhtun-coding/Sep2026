using System.ComponentModel.DataAnnotations;

namespace Sep2026.GameShopWebApi.Models
{
    public class GameSessionModel
    {
        public class Response
        {
            public int Id { get; set; }
            public int? BookingId { get; set; }
            public int ConsoleId { get; set; }
            public int? GameId { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public decimal TotalHours { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal PaidAmount { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class CreateRequest
        {
            [Range(1, int.MaxValue)]
            public int? BookingId { get; set; }
            [Required]
            [Range(1, int.MaxValue)]
            public int? ConsoleId { get; set; }
            [Range(1, int.MaxValue)]
            public int? GameId { get; set; } = null;
            [Required]
            public DateTime? StartTime { get; set; }
            [Required]
            public DateTime? EndTime { get; set; }
            [Range(typeof(decimal), "0", "9999999999999999.99")]
            public decimal PaidAmount { get; set; }
        }

        public class UpdateRequest
        {
            [Range(1, int.MaxValue)]
            public int? GameId { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            [Range(typeof(decimal), "0", "9999999999999999.99")]
            public decimal? PaidAmount { get; set; }
        }
    }
}
