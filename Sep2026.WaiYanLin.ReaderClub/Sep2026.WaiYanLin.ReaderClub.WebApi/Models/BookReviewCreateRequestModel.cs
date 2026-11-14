using System.ComponentModel.DataAnnotations;

namespace Sep2026.ReaderClub.WebApi.Models
{
    public class BookReviewCreateRequestModel
    {
        public int ReviewerId { get; set; }
        [Required]
        public string? BookTitle { get; set; }

        [Required]
        public string? Author { get; set; }

        [Required]
        public string? ReviewTitle { get; set; }

        [Required]
        public string? ReviewContent { get; set; }

        public DateOnly ReviewDate { get; set; }
    }

    public class BookReviewUpdateRequestModel
    {
        public string? BookTitle { get; set; }
        public string? Author { get; set; }
        public string? ReviewTitle { get; set; }
        public string? ReviewContent { get; set; }
        public DateOnly? ReviewDate { get; set; }
    }
}
