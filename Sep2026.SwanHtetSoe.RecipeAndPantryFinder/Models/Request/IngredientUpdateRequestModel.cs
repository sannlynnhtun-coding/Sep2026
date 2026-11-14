using System.ComponentModel.DataAnnotations;

namespace Sep2026.RecipeAndPantryFinder.Models.Request;

public class IngredientUpdateRequestModel
{
    [Range(1, int.MaxValue)]
    public int? RecipeId { get; set; }
    [StringLength(100)]
    public string? IngredientName { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? Amount { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }
}
