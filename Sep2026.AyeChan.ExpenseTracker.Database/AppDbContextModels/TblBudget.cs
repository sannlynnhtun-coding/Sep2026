using System;
using System.Collections.Generic;

namespace Sep2026.AyeChan.ExpenseTracker.Database.AppDbContextModels;

public partial class TblBudget
{
    public int BudgetId { get; set; }

    public int CategoryId { get; set; }

    public int BudgetYear { get; set; }

    public int BudgetMonth { get; set; }

    public decimal Amount { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual TblCategory Category { get; set; } = null!;
}
