using System.ComponentModel.DataAnnotations;

namespace Sep2026.GameShopWebApi.Models
{
    public class BookingModel
    {
        public class Response
        {
            public int Id { get; set; }
            public int ConsoleId { get; set; }
            public string? CustomerName { get; set; }
            public string? CustomerPhone { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public decimal TotalHours { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal DepositAmount { get; set; }
            public string? BookingStatus { get; set; }// Pending , Confirmed , Cancelled
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class CreateRequest
        {
            [Range(1, int.MaxValue)]
            public int ConsoleId { get; set; }
            [StringLength(100)]
            public string? CustomerName { get; set; } //optional
            [Required]
            [StringLength(20)]
            public string? CustomerPhone { get; set; }
            [Required]
            public DateTime? StartTime { get; set; }
            [Required]
            public DateTime? EndTime { get; set; }
            [Range(typeof(decimal), "0", "9999999999999999.99")]
            public decimal DepositAmount { get; set; }
        }

        public class UpdateRequest
        {
            [StringLength(100)]
            public string? CustomerName { get; set; }
            [StringLength(20)]
            public string? CustomerPhone { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            [StringLength(20)]
            public string? BookingStatus { get; set; } //Pending,Confirmed,Cancelled
            [Range(typeof(decimal), "0", "9999999999999999.99")]
            public decimal? DepositAmount { get; set; }
        }
        public class OverlapResponse
        {
            public DateTime? ReqStartTime { get; set; } = null;
            public DateTime? ReqEndTime { get; set; } = null;
            public List<Response> OverlapBookings { get; set; } = new List<Response>();
            public List<GameSessionModel.Response> OverlapSessions { get; set; } = new List<GameSessionModel.Response>();


        }
        //BookingStatusTypes
        public enum StatusTypes
        {
            Pending, // not sure or not paying valid deposite
            Confirmed,// after paying valid deposite
            Completed,// customer arrived (dead end )
            Cancelled// dead end 
        }
    }
}
