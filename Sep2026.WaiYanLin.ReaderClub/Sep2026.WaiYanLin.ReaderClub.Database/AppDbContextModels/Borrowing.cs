using System;
using System.Collections.Generic;

namespace Sep2026.ReaderClub.Database.AppDbContextModels;

public partial class Borrowing
{
    public int BorrowingId { get; set; }

    public string BookNo { get; set; } = null!;

    public string BorrowerNo { get; set; } = null!;

    public DateTime BorrowDate { get; set; }

    public bool IsReturned { get; set; }

    public virtual Book BookNoNavigation { get; set; } = null!;

    public virtual Borrower BorrowerNoNavigation { get; set; } = null!;
}
