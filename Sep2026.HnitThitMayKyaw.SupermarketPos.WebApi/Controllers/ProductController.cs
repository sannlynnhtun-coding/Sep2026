using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;
using Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProductController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetProductList()
        {
            var lst = _db.Products
                .Where(x=> x.DeleteFlag == false)                    
                .ToList();
            return Ok(lst);
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = _db.Products.FirstOrDefault(x => x.ProductId == id && x.DeleteFlag == false);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public IActionResult CreateProduct(CreateProductRequestModel requestModel)
        {
            _db.Products.Add(new Product
            {
                Barcode = requestModel.Barcode,
                ProductName = requestModel.ProductName,
                CategoryId = requestModel.CategoryId,
                PurchasePrice = requestModel.PurchasePrice,
                SellingPrice = requestModel.SellingPrice,
                QuantityInStock = requestModel.QuantityInStock,
                DeleteFlag = false,
                CreatedAtUtc = DateTime.UtcNow,
            });
            int result = _db.SaveChanges();
            string message = result > 0 ? "Create successful" : "Create failed";
            return Ok(message);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, UpdateProductRequestModel requestModel)
        {
            var product = _db.Products.FirstOrDefault(x=> x.ProductId== id && x.DeleteFlag == false);
            if (product is null)
            {
                return NotFound();
            }
            
            product.Barcode = requestModel.Barcode;
            product.ProductName = requestModel.ProductName;
            product.CategoryId = requestModel.CategoryId;
            product.PurchasePrice = requestModel.PurchasePrice;
            product.SellingPrice = requestModel.SellingPrice;
            product.QuantityInStock = requestModel.QuantityInStock;
            
            int result = _db.SaveChanges();
            string message = result > 0 ? "Update successful" : "Update failed";
            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _db.Products.FirstOrDefault(x=> x.ProductId == id && x.DeleteFlag == false);
            if (product is null)
            {
                return NotFound();
            }
            
            _db.Products.Remove(product);
            int result = _db.SaveChanges();
            string message = result > 0 ? "Delete successful" : "Delete failed";
            return Ok(message);
        }
    }
}
