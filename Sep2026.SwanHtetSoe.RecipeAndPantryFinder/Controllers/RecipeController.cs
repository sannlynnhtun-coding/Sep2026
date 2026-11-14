using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetRecipes([FromQuery] string? query, [FromQuery, Range(1, 100)] int? number)
    {
        int n = number is null or <= 0 ? 10 : number.Value;
        var recipes = await _recipeService.GetRecipesAsync(query, n);
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
}
