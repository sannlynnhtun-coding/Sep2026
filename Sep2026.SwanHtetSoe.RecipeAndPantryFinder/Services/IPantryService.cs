using Sep2026.RecipeAndPantryFinder.Models.Request;
using Sep2026.RecipeAndPantryFinder.Models.Response;

namespace Sep2026.RecipeAndPantryFinder.Services;

public interface IPantryService
{
    Task<List<PantryItemResponseModel>> GetPantryItemsAsync();

    Task<PantryItemResponseModel?> GetPantryItemByIdAsync(int id);

    Task<int> CreatePantryItemAsync(PantryItemCreateRequestModel request);

    Task<bool> UpdatePantryItemAsync(int id, PantryItemUpdateRequestModel request);

    Task<bool> DeletePantryItemAsync(int id);
}
