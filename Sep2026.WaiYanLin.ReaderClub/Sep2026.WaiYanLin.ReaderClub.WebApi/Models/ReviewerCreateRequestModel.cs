using Sep2026.ReaderClub.Database.AppDbContextModels;
using System.Text.Json.Serialization;

namespace Sep2026.ReaderClub.WebApi.Models
{
    public class ReviewerCreateRequestModel
    {

        public string ReviewerName { get; set; } = null!;

        public string ProfileDescription { get; set; } = null!;

    }

    public class ReviewerUpdateRequestModel
    {

        public string? ReviewerName { get; set; }

        public string? ProfileDescription { get; set; }

    }
}
