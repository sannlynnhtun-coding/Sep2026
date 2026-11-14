using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;
using Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SaleController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetSaleList()
        {
            var lst = _db.Sales
                .Where(x=> x.DeleteFlag == false)                    
                .ToList();
            return Ok(lst);
        }

        [HttpGet("{id}")]
        public IActionResult GetSale(int id)
        {
            var sale = _db.Sales.FirstOrDefault(x => x.SaleId == id && x.DeleteFlag == false);
            if (sale == null)
            {
                return NotFound();
            }
            return Ok(sale);
        }

        [HttpPost]
        public IActionResult CreateSale(CreateSaleRequestModel requestModel)
        {
            _db.Sales.Add(new Sale
            {
                SaleNumber = requestModel.SaleNumber,
                CustomerId = requestModel.CustomerId,
                EmployeeId = requestModel.EmployeeId,
                Subtotal = requestModel.Subtotal,
                DiscountAmount = requestModel.DiscountAmount,
                TaxAmount = requestModel.TaxAmount,
                TotalAmount = requestModel.TotalAmount,
                SaleDateUtc = DateTime.UtcNow,
                DeleteFlag = false
            });
            int result = _db.SaveChanges();
            string message = result > 0 ? "Create successful" : "Create failed";
            return Ok(message);
        }
    }
}
