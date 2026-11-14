using System;
using System.Collections.Generic;

namespace Sep2026.AyeChan.ExpenseTracker.Database.AppDbContextModels;

public partial class TblExpense
{
    public int ExpenseId { get; set; }

    public int CategoryId { get; set; }

    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; }

    public string? Note { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual TblCategory Category { get; set; } = null!;
}
