using System;
using System.Collections.Generic;

namespace Sep2026.ReaderClub.Database.AppDbContextModels;

public partial class Borrower
{
    public int BorrowerId { get; set; }

    public string No { get; set; } = null!;

    public string Name { get; set; } = null!;

    public long? PhoneNo { get; set; }

    public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
}
