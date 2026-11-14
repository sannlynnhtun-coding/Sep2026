namespace Sep2026.AyeChan.ExpenseTracker.WebApi.Models;

public class ExpenseResponseModel
{
    public int ExpenseId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Note { get; set; }
}