namespace Sep2026.RecipeAndPantryFinder.Models.Request;

public class PantryItemUpdateRequestModel
{
    public string? ItemName { get; set; }

    public decimal? Quantity { get; set; }

    public string? Unit { get; set; }
}
