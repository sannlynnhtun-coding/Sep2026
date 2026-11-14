using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.EntityFrameworkCore;
using Sep2026.GameShopWebApi.Models;
using Sep2026.GameShopWebApi.Utils;
using Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsoleCategoriesController : ControllerBase
    {
        private GameShopAppDbContext _db;
        private ModelConverter _modelConverter;
        public ConsoleCategoriesController()
        {
            _db = new GameShopAppDbContext();
            _modelConverter = new ModelConverter();
        }

        [HttpGet]
        public IActionResult GetAllConsoleCategories()
        {
            var response = _db.ConsoleCategories.ToList().Select(_modelConverter.ConvertConsoleCategoryResponse);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetConsoleCategoryById(int id)
        {
            var category = _db.ConsoleCategories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(_modelConverter.ConvertConsoleCategoryResponse(category));
        }

        [HttpPost]
        public IActionResult CreateConsoleCategory(ConsoleCategoryModel.CreateRequest createRequest)
        {
            if (createRequest == null) { return BadRequest("Must define a body"); }

            if (string.IsNullOrEmpty(createRequest.Name)) { return BadRequest("Category's name must be defined"); }

            if (createRequest.HourlyPrice < 5000m) { return BadRequest("Hourly price should have minimum 5000mmk"); }

            var newCategory = new ConsoleCategory
            {
                HourlyRate = createRequest.HourlyPrice,
                Name = createRequest.Name,
                IsDeleted = false,
            };

            _db.ConsoleCategories.Add(newCategory);
            int status = _db.SaveChanges();

            return status > 0 ? Ok(_modelConverter.ConvertConsoleCategoryResponse(newCategory)) : BadRequest("Fail to create new console category");

        }

        [HttpPatch("{id}")]
        public IActionResult UpdateCategory(int id, ConsoleCategoryModel.UpdateRequest updateRequest)
        {
            
            var targetCategory = _db.ConsoleCategories.FirstOrDefault(c => c.Id == id);

            if (targetCategory == null) { return NotFound("Console category with this Id doesn't exsit"); }

            if (!string.IsNullOrEmpty(updateRequest.Name)) {
                targetCategory.Name = updateRequest.Name;
            }

            if (updateRequest.HourlyPrice.HasValue && updateRequest.HourlyPrice.Value > 5000m) {
                
                targetCategory.HourlyRate = updateRequest.HourlyPrice.Value;                
            }

            int status = _db.SaveChanges();
            return status > 0 ? Ok(_modelConverter.ConvertConsoleCategoryResponse(targetCategory)) : BadRequest("Fail to update.");

        }
        //delete
        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(int id, [FromQuery] bool isHardDelete = false)
        {
            var targetCategory = _db.ConsoleCategories.FirstOrDefault(c => c.Id == id);
            if (targetCategory == null) return NotFound("Category with this Id doesn't exsit");

            //block delete
            bool isConsoleExist = _db.GameConsoles.Any(console => console.CategoryId == targetCategory.Id && console.IsDeleted == false);
            bool isGameExist = targetCategory.Games.Any(game => game.CategoryId == targetCategory.Id && game.IsDeleted == false);

            if (isConsoleExist || isGameExist)
            {
                return BadRequest($"Category {targetCategory.Name} having consoles and games.Try to remove them first.");
            }

            //hard delete
            if (isHardDelete == true)
            {

                _db.ConsoleCategories.Remove(targetCategory);
                
            }
            //soft delete
            else
            {
                targetCategory.IsDeleted = true;

            }

            int status = _db.SaveChanges();
            return status > 0 ? Ok("Successfully deleted") : BadRequest("Fail to delete");

        }
    }
}
