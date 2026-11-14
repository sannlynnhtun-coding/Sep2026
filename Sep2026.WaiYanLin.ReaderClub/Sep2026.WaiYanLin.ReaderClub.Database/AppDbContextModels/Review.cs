using System;
using System.Collections.Generic;

namespace Sep2026.ReaderClub.Database.AppDbContextModels;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? ReviewerId { get; set; }

    public int? BookReviewId { get; set; }

    public virtual BookReview? BookReview { get; set; }

    public virtual Reviewer? Reviewer { get; set; }
}
