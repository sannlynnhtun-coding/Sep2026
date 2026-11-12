namespace Sep2026.RecipeAndPantryFinder.Models.Response;

public class RecipeDetailResponseModel
{
    public int RecipeId { get; set; }

    public string Title { get; set; } = null!;

    public string Instructions { get; set; } = null!;

    public int ReadyInMinutes { get; set; }

    public int Servings { get; set; }

    public List<IngredientResponseModel> Ingredients { get; set; } = new();
}
