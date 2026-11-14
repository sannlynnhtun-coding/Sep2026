namespace Sep2026.ReceipeDb.Models;

public class RecipeIngredient
{
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public string IngredientName { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Unit { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;
}
