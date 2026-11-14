namespace Sep2026.RecipeAndPantryFinder.Models.Request;

public class RecipeCreateRequestModel
{
    public string Title { get; set; } = null!;

    public string Instructions { get; set; } = null!;

    public int ReadyInMinutes { get; set; }

    public int Servings { get; set; }
}
