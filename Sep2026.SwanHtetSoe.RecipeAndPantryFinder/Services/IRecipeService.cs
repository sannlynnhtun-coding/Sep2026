using Sep2026.RecipeAndPantryFinder.Models.Response;

namespace Sep2026.RecipeAndPantryFinder.Services;

public interface IRecipeService
{

    Task<List<RecipeResponseModel>> GetRecipesAsync(string? query, int number);

    Task<RecipeDetailResponseModel?> GetRecipeByIdAsync(int id);

    Task<List<RecipeSearchResponseModel>> SearchRecipesByPantryAsync();
}
