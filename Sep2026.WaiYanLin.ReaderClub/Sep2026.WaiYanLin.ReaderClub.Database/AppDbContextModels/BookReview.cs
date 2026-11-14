using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sep2026.ReaderClub.Database.AppDbContextModels;

public partial class BookReview
{
    public int BookReviewId { get; set; }

    public string? BookTitle { get; set; }

    public string? Author { get; set; }

    public string? ReviewTitle { get; set; }

    public string? ReviewContent { get; set; }

    public DateOnly? ReviewDate { get; set; }

    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
