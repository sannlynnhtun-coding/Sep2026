namespace Sep2026.AyeChan.ExpenseTracker.WebApi.Models;

public class BudgetRequestModel
{
    public int CategoryId { get; set; }
    public int BudgetYear { get; set; }
    public int BudgetMonth { get; set; }
    public decimal Amount { get; set; }
}