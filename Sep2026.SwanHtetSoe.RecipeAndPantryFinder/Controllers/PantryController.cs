using Microsoft.AspNetCore.Mvc;
using Sep2026.RecipeAndPantryFinder.Models.Request;
using Sep2026.RecipeAndPantryFinder.Services;

namespace Sep2026.RecipeAndPantryFinder.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PantryController : ControllerBase
{
    private readonly IPantryService _pantryService;

    public PantryController(IPantryService pantryService)
    {
        _pantryService = pantryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPantryItems()
    {
        var items = await _pantryService.GetPantryItemsAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPantryItem(int id)
    {
        var item = await _pantryService.GetPantryItemByIdAsync(id);
        if (item == null)
        {
            return NotFound($"Pantry item with id {id} not found.");
        }
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePantryItem(PantryItemCreateRequestModel request)
    {
        var id = await _pantryService.CreatePantryItemAsync(request);
        return Ok(new { Message = "Pantry item created successfully.", PantryItemId = id });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdatePantryItem(int id,PantryItemUpdateRequestModel request)
    {
        var updated = await _pantryService.UpdatePantryItemAsync(id, request);
        if (!updated)
        {
            return NotFound($"Pantry item with id {id} not found.");
        }
        return Ok("Pantry item updated successfully.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePantryItem(int id)
    {
        var deleted = await _pantryService.DeletePantryItemAsync(id);
        if (!deleted)
        {
            return NotFound($"Pantry item with id {id} not found.");
        }
        return Ok("Pantry item deleted successfully.");
    }
}
