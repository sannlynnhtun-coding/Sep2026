using System.ComponentModel.DataAnnotations;

namespace Sep2026.ReaderClub.WebApi.Models
{
    public class BookReviewCreateRequestModel
    {
        public int ReviewerId { get; set; }
        [Required, StringLength(255)]
        public string? BookTitle { get; set; }

        [Required, StringLength(255)]
        public string? Author { get; set; }

        [Required, StringLength(255)]
        public string? ReviewTitle { get; set; }

        [Required]
        public string? ReviewContent { get; set; }

        public DateOnly ReviewDate { get; set; }
    }

    public class BookReviewUpdateRequestModel
    {
        [StringLength(255)]
        public string? BookTitle { get; set; }
        [StringLength(255)]
        public string? Author { get; set; }
        [StringLength(255)]
        public string? ReviewTitle { get; set; }
        public string? ReviewContent { get; set; }
        public DateOnly? ReviewDate { get; set; }
    }
}
