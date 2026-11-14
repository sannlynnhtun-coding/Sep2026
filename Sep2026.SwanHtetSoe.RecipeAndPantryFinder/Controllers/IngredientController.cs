using Microsoft.AspNetCore.Mvc;
using Sep2026.RecipeAndPantryFinder.Models.Request;
using Sep2026.RecipeAndPantryFinder.Services;

namespace Sep2026.RecipeAndPantryFinder.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IngredientController : ControllerBase
{
    private readonly IIngredientService _ingredientService;

    public IngredientController(IIngredientService ingredientService)
    {
        _ingredientService = ingredientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetIngredients()
    {
        var items = await _ingredientService.GetIngredientsAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIngredient(int id)
    {
        var item = await _ingredientService.GetIngredientByIdAsync(id);
        if (item == null)
        {
            return NotFound($"Ingredient with id {id} not found for recipe.");
        }
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> CreateIngredient(int recipeId, IngredientCreateRequestModel request)
    {
        var id = await _ingredientService.CreateIngredientAsync(recipeId, request);
        return Ok(new { Message = "Ingredient created successfully.", Id = id });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateIngredient(int id,IngredientUpdateRequestModel request)
    {
        var updated = await _ingredientService.UpdateIngredientAsync(id, request);
        if (!updated)
        {
            return NotFound($"Ingredient with id {id} not found");
        }
        return Ok("Ingredient updated successfully.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIngredient(int id)
    {
        var deleted = await _ingredientService.DeleteIngredientAsync(id);
        if (!deleted)
        {
            return NotFound($"Ingredient with id {id} not found .");
        }
        return Ok("Ingredient deleted successfully.");
    }
}
