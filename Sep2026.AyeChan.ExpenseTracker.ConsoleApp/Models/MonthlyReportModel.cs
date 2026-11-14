namespace Sep2026.AyeChan.ExpenseTracker.ConsoleApp.Models;

public class MonthlyReportModel
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    // Calculated in C#, not mapped from SQL
    public bool IsOverBudget => RemainingAmount < 0;
    public string Status => IsOverBudget ? "Over Budget" : "OK";
}