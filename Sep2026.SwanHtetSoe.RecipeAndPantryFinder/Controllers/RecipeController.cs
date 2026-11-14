using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Sep2026.RecipeAndPantryFinder.Models.Request;
using Sep2026.RecipeAndPantryFinder.Services;

namespace Sep2026.RecipeAndPantryFinder.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase
{
    private readonly IRecipeService _recipeService;

    public RecipeController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecipes()
    {
        var recipes = await _recipeService.GetRecipesAsync();
        return Ok(recipes);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRecipes()
    {
        var results = await _recipeService.SearchRecipesByPantryAsync();
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecipe(int id)
    {
        var recipe = await _recipeService.GetRecipeByIdAsync(id);
        if (recipe == null)
        {
            return NotFound($"Recipe with id {id} not found.");
        }
        return Ok(recipe);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecipe(RecipeCreateRequestModel request)
    {
        var id = await _recipeService.CreateRecipeAsync(request);
        return Ok(new { Message = "Recipe created successfully.", RecipeId = id });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateRecipe(int id, RecipeUpdateRequestModel request)
    {
        var updated = await _recipeService.UpdateRecipeAsync(id, request);
        if (!updated)
        {
            return NotFound($"Recipe with id {id} not found.");
        }
        return Ok("Recipe updated successfully.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        var deleted = await _recipeService.DeleteRecipeAsync(id);
        if (!deleted)
        {
            return NotFound($"Recipe with id {id} not found.");
        }
        return Ok("Recipe deleted successfully.");
    }
}
