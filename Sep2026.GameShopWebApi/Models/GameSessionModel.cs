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
            public int? BookingId { get; set; }
            public int? ConsoleId { get; set; }
            public int? GameId { get; set; } = null;
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public decimal PaidAmount { get; set; }
        }

        public class UpdateRequest
        {
            public int? GameId { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public decimal? PaidAmount { get; set; }
        }
    }
}
