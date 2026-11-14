using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;
using Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CategoryController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetCategoryList()
        {
            var Lst = _db.Categories
                .Where(x=> x.DeleteFlag == false)                    
                .ToList();
            return Ok(Lst);
        }

        [HttpGet("{id}")]
        public IActionResult GetCategory(int id)
        {
            var category = _db.Categories.FirstOrDefault(x => x.CategoryId == id && x.DeleteFlag == false);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPost]
        public IActionResult CreateCategory(CreateCategoryRequestModel requestModel)
        {
            _db.Categories.Add(new Category
            {
                CategoryName = requestModel.CategoryName,
                Description = requestModel.Description,
                DeleteFlag = false,
                CreatedAtUtc = DateTime.UtcNow,
            });
            int result = _db.SaveChanges();
            string message = result > 0 ? "Create successful" : "Create failed";
            return Ok(message);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCategory(int id, UpdateCategoryRequestModel requestModel)
        {
            var category = _db.Categories.FirstOrDefault(x=> x.CategoryId == id && x.DeleteFlag == false);
            if (category is null)
            {
                return NotFound();
            }
            
            category.CategoryName = requestModel.CategoryName;
            category.Description = requestModel.Description;
            
            int result = _db.SaveChanges();
            string message = result > 0 ? "Update successful" : "Update failed";
            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            var category = _db.Categories.FirstOrDefault(x=> x.CategoryId == id && x.DeleteFlag == false);
            if (category is null)
            {
                return NotFound();
            }
            
            category.DeleteFlag = true;
            int result = _db.SaveChanges();
            string message = result > 0 ? "Delete successful" : "Delete failed";
            return Ok(message);
        }
    }
}
