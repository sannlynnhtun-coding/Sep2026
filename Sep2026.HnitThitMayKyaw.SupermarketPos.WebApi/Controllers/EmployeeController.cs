using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;
using Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EmployeeController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetEmployeeList()
        {
            var employees = _db.Employees
                .Where(x => x.DeleteFlag == false)
                .ToList();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public IActionResult GetEmployee(int id)
        {
            var employee = _db.Employees.FirstOrDefault(x => x.EmployeeId  == id && x.DeleteFlag==false);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpPost]
        public IActionResult CreateEmployee(UpdateEmployeesRequestModel requestModel)
        {
            _db.Employees.Add(new Employee
            {
                FirstName = requestModel.FirstName,
                LastName = requestModel.LastName,
                Phone = requestModel.Phone,
                DeleteFlag = false,
                CreatedAtUtc = DateTime.UtcNow
            });
            
            int result = _db.SaveChanges();
            string message = result > 0 ? "Create success" : "Create failure";
            return Ok(message);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, UpdateEmployeesRequestModel requestModel)
        {
            var employee = _db.Employees.FirstOrDefault(x => x.EmployeeId  == id && x.DeleteFlag==false);
            if (employee == null)
            {
                return NotFound();
            }
            employee.FirstName = requestModel.FirstName;
            employee.LastName = requestModel.LastName;
            employee.Phone = requestModel.Phone;
            
            int result = _db.SaveChanges();
            string message = result > 0 ? "Update success" : "Update failure";
            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = _db.Employees.FirstOrDefault(x => x.EmployeeId  == id && x.DeleteFlag==false);
            if (employee == null)
            {
                return NotFound();
            }
            employee.DeleteFlag = true;
            int result = _db.SaveChanges();
            string message = result > 0 ? "Delete success" : "Delete failure";
            return Ok(message);
        }
        
    }
}
