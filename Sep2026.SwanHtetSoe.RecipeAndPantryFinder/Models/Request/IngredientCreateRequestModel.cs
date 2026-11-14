namespace Sep2026.RecipeAndPantryFinder.Models.Request;

public class IngredientCreateRequestModel
{
    public string IngredientName { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Unit { get; set; } = null!;
}
