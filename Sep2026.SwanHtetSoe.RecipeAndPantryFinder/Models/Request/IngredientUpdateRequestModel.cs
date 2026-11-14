namespace Sep2026.RecipeAndPantryFinder.Models.Request;

public class IngredientUpdateRequestModel
{
    public int? RecipeId { get; set; }
    public string? IngredientName { get; set; }

    public decimal? Amount { get; set; }

    public string? Unit { get; set; }
}
