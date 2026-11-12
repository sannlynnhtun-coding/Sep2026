namespace Sep2026.RecipeAndPantryFinder.Models.Response;

public class RecipeResponseModel
{
    public int RecipeId { get; set; }

    public string Title { get; set; } = null!;

    public int ReadyInMinutes { get; set; }

    public int Servings { get; set; }
}
