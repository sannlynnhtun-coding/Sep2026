namespace Sep2026.ReceipeDb.Models;

public class PantryItem
{
    public int PantryItemId { get; set; }

    public string ItemName { get; set; } = null!;

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = null!;
}
