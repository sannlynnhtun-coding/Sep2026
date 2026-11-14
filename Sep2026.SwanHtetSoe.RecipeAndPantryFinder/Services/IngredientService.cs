using Microsoft.EntityFrameworkCore;
using Sep2026.RecipeAndPantryFinder.Models.Request;
using Sep2026.RecipeAndPantryFinder.Models.Response;
using Sep2026.ReceipeDb.Models;

namespace Sep2026.RecipeAndPantryFinder.Services;

public class IngredientService : IIngredientService
{
    private readonly AppDbContext _db;

    public IngredientService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<IngredientResponseModel>> GetIngredientsAsync()
    {
        return await _db.RecipeIngredients
            .OrderBy(i => i.Id)
            .Select(i => new IngredientResponseModel
            {
                Id = i.Id,
                IngredientName = i.IngredientName,
                Amount = i.Amount,
                Unit = i.Unit
            })
            .ToListAsync();
    }

    public async Task<IngredientResponseModel?> GetIngredientByIdAsync(int id)
    {
        var item = await _db.RecipeIngredients
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return null;
        }

        return new IngredientResponseModel
        {
            Id = item.Id,
            IngredientName = item.IngredientName,
            Amount = item.Amount,
            Unit = item.Unit
        };
    }

    public async Task<int> CreateIngredientAsync(int recipeId, IngredientCreateRequestModel request)
    {
        var recipeExists = await _db.Recipes.AnyAsync(r => r.RecipeId == recipeId);
        if (!recipeExists)
        {
            throw new KeyNotFoundException($"Recipe with id {recipeId} not found.");
        }

        var ingredient = new RecipeIngredient
        {
            RecipeId = recipeId,
            IngredientName = request.IngredientName.Trim(),
            Amount = request.Amount,
            Unit = request.Unit.Trim()
        };

        _db.RecipeIngredients.Add(ingredient);
        await _db.SaveChangesAsync();

        return ingredient.Id;
    }

    public async Task<bool> UpdateIngredientAsync(int id, IngredientUpdateRequestModel request)
    {
        var item = await _db.RecipeIngredients
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return false;
        }

        if(request.RecipeId.HasValue && request.RecipeId.Value > 0)
        {
            item.RecipeId = request.RecipeId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.IngredientName))
        {
            item.IngredientName = request.IngredientName.Trim();
        }

        if (request.Amount.HasValue && request.Amount.Value >= 0)
        {
            item.Amount = request.Amount.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Unit))
        {
            item.Unit = request.Unit.Trim();
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteIngredientAsync(int id)
    {
        var item = await _db.RecipeIngredients
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return false;
        }

        _db.RecipeIngredients.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}
