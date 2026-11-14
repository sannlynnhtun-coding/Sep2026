namespace Sep2026.RecipeAndPantryFinder.Models.Response;

public class PantryItemResponseModel
{
    public int PantryItemId { get; set; }

    public string ItemName { get; set; } = null!;

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = null!;
}
