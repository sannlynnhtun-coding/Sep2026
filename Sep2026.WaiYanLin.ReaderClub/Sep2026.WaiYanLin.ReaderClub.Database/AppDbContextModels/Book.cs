using System;
using System.Collections.Generic;

namespace Sep2026.ReaderClub.Database.AppDbContextModels;

public partial class Book
{
    public int BookId { get; set; }

    public string BookNo { get; set; } = null!;

    public string BookTitle { get; set; } = null!;

    public string? Author { get; set; }

    public int Quantity { get; set; }

    public int AvailableQuantity { get; set; }

    public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
}
