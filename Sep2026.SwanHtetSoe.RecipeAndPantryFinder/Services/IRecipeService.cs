using Sep2026.RecipeAndPantryFinder.Models.Request;
using Sep2026.RecipeAndPantryFinder.Models.Response;

namespace Sep2026.RecipeAndPantryFinder.Services;

public interface IRecipeService
{
    Task<List<RecipeResponseModel>> GetRecipesAsync();

    Task<RecipeDetailResponseModel?> GetRecipeByIdAsync(int id);

    Task<int> CreateRecipeAsync(RecipeCreateRequestModel request);

    Task<bool> UpdateRecipeAsync(int id, RecipeUpdateRequestModel request);

    Task<bool> DeleteRecipeAsync(int id);

    Task<List<RecipeSearchResponseModel>> SearchRecipesByPantryAsync();
}
