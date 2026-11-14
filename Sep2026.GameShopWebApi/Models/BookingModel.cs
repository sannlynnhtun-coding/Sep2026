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
            public int ConsoleId { get; set; }
            public string? CustomerName { get; set; } //optional
            public string? CustomerPhone { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public decimal DepositAmount { get; set; }
        }

        public class UpdateRequest
        {
            public string? CustomerName { get; set; }
            public string? CustomerPhone { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string? BookingStatus { get; set; } //Pending,Confirmed,Cancelled
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
