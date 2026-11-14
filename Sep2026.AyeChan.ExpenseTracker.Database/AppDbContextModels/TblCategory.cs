using System;
using System.Collections.Generic;

namespace Sep2026.AyeChan.ExpenseTracker.Database.AppDbContextModels;

public partial class TblCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public bool DeleteFlag { get; set; }

    public virtual ICollection<TblBudget> TblBudgets { get; set; } = new List<TblBudget>();

    public virtual ICollection<TblExpense> TblExpenses { get; set; } = new List<TblExpense>();
}
