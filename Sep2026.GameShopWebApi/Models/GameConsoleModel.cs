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
            public string? ConsoleNumber { get; set; }
            public int CategoryId { get; set; }
            public string? CurrentStatus { get; set; }
        }
        public class CreateRequest
        {
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
