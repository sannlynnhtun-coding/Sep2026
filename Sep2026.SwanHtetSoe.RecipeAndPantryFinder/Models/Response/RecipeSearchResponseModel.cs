namespace Sep2026.RecipeAndPantryFinder.Models.Response;

public class RecipeSearchResponseModel
{
    public int RecipeId { get; set; }

    public string Title { get; set; } = null!;

    public int MatchedIngredientCount { get; set; }

    public int TotalIngredientCount { get; set; }

    public List<string> MissingIngredients { get; set; } = new();
}
