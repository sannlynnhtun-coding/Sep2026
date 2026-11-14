using Sep2026.RecipeAndPantryFinder.Models.Request;
using Sep2026.RecipeAndPantryFinder.Models.Response;

namespace Sep2026.RecipeAndPantryFinder.Services;

public interface IIngredientService
{
    Task<List<IngredientResponseModel>> GetIngredientsAsync();

    Task<IngredientResponseModel?> GetIngredientByIdAsync(int id);

    Task<int> CreateIngredientAsync(int recipeId, IngredientCreateRequestModel request);

    Task<bool> UpdateIngredientAsync(int id, IngredientUpdateRequestModel request);

    Task<bool> DeleteIngredientAsync(int id);
}
