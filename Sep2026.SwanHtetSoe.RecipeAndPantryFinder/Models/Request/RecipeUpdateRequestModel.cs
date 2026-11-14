using System.ComponentModel.DataAnnotations;

namespace Sep2026.RecipeAndPantryFinder.Models.Request;

public class RecipeUpdateRequestModel
{
    [StringLength(200)]
    public string? Title { get; set; }

    public string? Instructions { get; set; }

    [Range(0, int.MaxValue)]
    public int? ReadyInMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int? Servings { get; set; }
}
