namespace Sep2026.RecipeAndPantryFinder.Models.Request;

public class RecipeUpdateRequestModel
{
    public string? Title { get; set; }

    public string? Instructions { get; set; }

    public int? ReadyInMinutes { get; set; }

    public int? Servings { get; set; }
}
