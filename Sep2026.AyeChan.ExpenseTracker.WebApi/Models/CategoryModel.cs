namespace Sep2026.AyeChan.ExpenseTracker.WebApi.Models;

public class CategoryModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool DeleteFlag { get; set; }
}