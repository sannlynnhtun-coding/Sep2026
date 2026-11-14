using Microsoft.EntityFrameworkCore;
using Sep2026.RecipeAndPantryFinder.Models.Request;
using Sep2026.RecipeAndPantryFinder.Models.Response;
using Sep2026.ReceipeDb.Models;

namespace Sep2026.RecipeAndPantryFinder.Services;

public class PantryService : IPantryService
{
    private readonly AppDbContext _db;

    public PantryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PantryItemResponseModel>> GetPantryItemsAsync()
    {
        return await _db.PantryItems
            .OrderBy(p => p.PantryItemId)
            .Select(p => new PantryItemResponseModel
            {
                PantryItemId = p.PantryItemId,
                ItemName = p.ItemName,
                Quantity = p.Quantity,
                Unit = p.Unit
            })
            .ToListAsync();
    }

    public async Task<PantryItemResponseModel?> GetPantryItemByIdAsync(int id)
    {
        var item = await _db.PantryItems.FirstOrDefaultAsync(p => p.PantryItemId == id);

        if (item == null)
        {
            return null;
        }

        return new PantryItemResponseModel
        {
            PantryItemId = item.PantryItemId,
            ItemName = item.ItemName,
            Quantity = item.Quantity,
            Unit = item.Unit
        };
    }

    public async Task<int> CreatePantryItemAsync(PantryItemCreateRequestModel request)
    {
        var item = new PantryItem
        {
            ItemName = request.ItemName,
            Quantity = request.Quantity,
            Unit = request.Unit
        };

        _db.PantryItems.Add(item);
        await _db.SaveChangesAsync();

        return item.PantryItemId;
    }

    public async Task<bool> UpdatePantryItemAsync(int id, PantryItemUpdateRequestModel request)
    {
        var item = await _db.PantryItems.FirstOrDefaultAsync(p => p.PantryItemId == id);

        if (item == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(request.ItemName))
        {
            item.ItemName = request.ItemName.Trim();
        }

        if (request.Quantity.HasValue)
        {
            item.Quantity = request.Quantity.Value;
        }

        if (!string.IsNullOrEmpty(request.Unit))
        {
            item.Unit = request.Unit.Trim();
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePantryItemAsync(int id)
    {
        var item = await _db.PantryItems.FirstOrDefaultAsync(p => p.PantryItemId == id);

        if (item == null)
        {
            return false;
        }

        _db.PantryItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}
