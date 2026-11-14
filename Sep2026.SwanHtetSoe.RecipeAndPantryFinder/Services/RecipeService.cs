using Microsoft.EntityFrameworkCore;
using Sep2026.RecipeAndPantryFinder.Models.Request;
using Sep2026.RecipeAndPantryFinder.Models.Response;
using Sep2026.ReceipeDb.Models;

namespace Sep2026.RecipeAndPantryFinder.Services;

public class RecipeService : IRecipeService
{
    private readonly AppDbContext _db;

    public RecipeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RecipeResponseModel>> GetRecipesAsync()
    {
        return await _db.Recipes
            .OrderBy(r => r.RecipeId)
            .Select(r => new RecipeResponseModel
            {
                RecipeId = r.RecipeId,
                Title = r.Title,
                ReadyInMinutes = r.ReadyInMinutes,
                Servings = r.Servings
            })
            .ToListAsync();
    }

    public async Task<RecipeDetailResponseModel?> GetRecipeByIdAsync(int id)
    {
        var recipe = await _db.Recipes
            .Include(r => r.RecipeIngredients)
            .FirstOrDefaultAsync(r => r.RecipeId == id);

        if (recipe == null) return null;

        return new RecipeDetailResponseModel
        {
            RecipeId = recipe.RecipeId,
            Title = recipe.Title,
            Instructions = recipe.Instructions,
            ReadyInMinutes = recipe.ReadyInMinutes,
            Servings = recipe.Servings,
            Ingredients = recipe.RecipeIngredients
                .Select(i => new IngredientResponseModel
                {
                    Id = i.Id,
                    IngredientName = i.IngredientName,
                    Amount = i.Amount,
                    Unit = i.Unit
                })
                .ToList()
        };
    }

    public async Task<int> CreateRecipeAsync(RecipeCreateRequestModel request)
    {
        var recipe = new Recipe
        {
            Title = request.Title.Trim(),
            Instructions = request.Instructions.Trim(),
            ReadyInMinutes = request.ReadyInMinutes,
            Servings = request.Servings
        };

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();

        return recipe.RecipeId;
    }

    public async Task<bool> UpdateRecipeAsync(int id, RecipeUpdateRequestModel request)
    {
        var recipe = await _db.Recipes.FirstOrDefaultAsync(r => r.RecipeId == id);
        if (recipe == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            recipe.Title = request.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Instructions))
        {
            recipe.Instructions = request.Instructions.Trim();
        }

        if (request.ReadyInMinutes.HasValue && request.ReadyInMinutes.Value >= 0)
        {
            recipe.ReadyInMinutes = request.ReadyInMinutes.Value;
        }

        if (request.Servings.HasValue && request.Servings.Value > 0)
        {
            recipe.Servings = request.Servings.Value;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRecipeAsync(int id)
    {
        var recipe = await _db.Recipes.FirstOrDefaultAsync(r => r.RecipeId == id);
        if (recipe == null)
        {
            return false;
        }

        _db.Recipes.Remove(recipe);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<RecipeSearchResponseModel>> SearchRecipesByPantryAsync()
    {
        var pantryItems = await _db.PantryItems.ToListAsync();

        var pantryNames = pantryItems
            .Where(p => !string.IsNullOrWhiteSpace(p.ItemName))
            .Select(p => p.ItemName.Trim())
            .ToList();
        if (pantryNames.Count == 0)
            return [];

        var allRecipes = await _db.Recipes
            .Include(r => r.RecipeIngredients)
            .ToListAsync();

        return allRecipes
            .Select(r =>
            {
                var ingredientNames = r.RecipeIngredients
                    .Where(i => !string.IsNullOrWhiteSpace(i.IngredientName))
                    .Select(i => i.IngredientName.Trim())
                    .ToList();

                var matchedCount = ingredientNames
                    .Count(name => pantryNames.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase) || p.Contains(name, StringComparison.OrdinalIgnoreCase)));

                var missingIngredients = ingredientNames
                    .Where(name => !pantryNames.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase) || p.Contains(name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                return new RecipeSearchResponseModel
                {
                    RecipeId = r.RecipeId,
                    Title = r.Title,
                    MatchedIngredientCount = matchedCount,
                    TotalIngredientCount = ingredientNames.Count,
                    MissingIngredients = missingIngredients
                };
            })
            .Where(r => r.MatchedIngredientCount > 0)
            .OrderByDescending(r => r.MatchedIngredientCount)
            .ToList();
    }
}
