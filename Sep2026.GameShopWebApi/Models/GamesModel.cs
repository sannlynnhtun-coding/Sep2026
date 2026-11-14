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
            public string? Title { get; set; }
            public int? CategoryId { get; set; }
            public int TotalStock { get; set; }
        }

        public class UpdateRequest
        {
            public string? Title { get; set; }
            public int? CategoryId { get; set; }
            public int? TotalStock { get; set; }
            public int? AvailableStock { get; set; }
        }
    }
}
