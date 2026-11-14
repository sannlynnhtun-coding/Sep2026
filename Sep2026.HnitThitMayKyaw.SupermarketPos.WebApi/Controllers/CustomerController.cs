using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;
using Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CustomerController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetCustomerList()
        {
            var Lst = _db.Customers
                .Where(x=> x.DeleteFlag == false)                    
                .ToList();
            return Ok(Lst);
        }

        [HttpGet("{id}")]
        public IActionResult GetCustomer(int id)
        {
            var customer = _db.Customers.FirstOrDefault(x => x.CustomerId == id && x.DeleteFlag == false);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        [HttpPost]
        public IActionResult CreateCustomer(CreateCustomerRequestModel requestModel)
        {
            _db.Customers.Add(new Customer
            {
                FirstName = requestModel.FirstName,
                LastName = requestModel.LastName,
                Email = requestModel.Email,
                DeleteFlag = false,
                CreatedAtUtc = DateTime.UtcNow,
            });
            int result = _db.SaveChanges();
            string message = result > 0 ? "Create successful" : "Create failed";
            return Ok(message);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(int id, UpdateCustomerRequestModel requestModel)
        {
            var customer = _db.Customers.FirstOrDefault(x=> x.CustomerId == id && x.DeleteFlag == false);
            if (customer is null)
            {
                return NotFound();
            }
            
            customer.FirstName = requestModel.FirstName;
            customer.LastName = requestModel.LastName;
            customer.Email = requestModel.Email;
            
            int result = _db.SaveChanges();
            string message = result > 0 ? "Update successful" : "Update failed";
            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _db.Customers.FirstOrDefault(x => x.CustomerId == id && x.DeleteFlag == false);
            if (customer is null)
            {
                return NotFound();
            }
            
            _db.Customers.Remove(customer);
            int result = _db.SaveChanges();
            string message = result > 0 ? "Delete successful" : "Delete failed";
            return Ok(message);
        }
    }
}
