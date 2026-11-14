namespace Sep2026.RecipeAndPantryFinder.Models.Request;

public class PantryItemCreateRequestModel
{
    public string ItemName { get; set; } = null!;

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = null!;
}
