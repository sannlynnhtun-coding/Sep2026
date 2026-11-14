namespace Sep2026.ReceipeDb.Models;

public class Recipe
{
    public int RecipeId { get; set; }

    public string Title { get; set; } = null!;

    public string Instructions { get; set; } = null!;

    public int ReadyInMinutes { get; set; }

    public int Servings { get; set; }

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
