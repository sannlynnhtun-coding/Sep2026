namespace Sep2026.AyeChan.ExpenseTracker.WebApi.Models;

public class BudgetResponseModel
{
    public int BudgetId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int BudgetYear { get; set; }
    public int BudgetMonth { get; set; }
    public decimal Amount { get; set; }
}