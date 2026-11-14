namespace Sep2026.RecipeAndPantryFinder.Models.Response;

public class IngredientResponseModel
{
    public int Id { get; set; }

    public string IngredientName { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Unit { get; set; } = null!;
}
