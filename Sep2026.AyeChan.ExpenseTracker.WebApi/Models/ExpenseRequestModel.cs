namespace Sep2026.AyeChan.ExpenseTracker.WebApi.Models;

public class ExpenseRequestModel
{
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Note { get; set; }
}