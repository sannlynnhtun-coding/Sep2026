using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Sep2026.RecipeAndPantryFinder.Services;

namespace Sep2026.RecipeAndPantryFinder.Controllers;

[ApiController]
[Route("api/spoonacular")]
public class SpoonacularController(SpoonacularService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery, Range(1, 100)] int number = 10)
        => Ok(await service.GetRecipesAsync(query, number));

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetRecipe(int id)
    {
        var recipe = await service.GetRecipeByIdAsync(id);
        return recipe == null ? NotFound() : Ok(recipe);
    }

    [HttpGet("{id:int:min(1)}/ingredients")]
    public async Task<IActionResult> GetIngredients(int id)
    {
        var ingredients = await service.GetIngredientsByRecipeIdAsync(id);
        return ingredients == null ? NotFound() : Ok(ingredients);
    }

    [HttpGet("pantry")]
    public async Task<IActionResult> SearchByPantry()
        => Ok(await service.SearchRecipesByPantryAsync());
}
