using System.ComponentModel.DataAnnotations;

namespace Sep2026.RecipeAndPantryFinder.Models.Request;

public class PantryItemCreateRequestModel
{
    [Required]
    [StringLength(100)]
    public string ItemName { get; set; } = null!;

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal Quantity { get; set; }

    [Required]
    [StringLength(50)]
    public string Unit { get; set; } = null!;
}
