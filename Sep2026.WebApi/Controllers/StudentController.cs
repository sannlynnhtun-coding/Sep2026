using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sep2026.ConsoleApp5.Database.AppDbContextModels;
using Sep2026.WebApi.Models;

namespace Sep2026.WebApi.Controllers
{
    // api/student
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StudentController()
        {
            _db = new AppDbContext();
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            var lst = _db.Students.ToList();
            return Ok(lst);
        }

        [HttpGet("Edit/{id}")]
        public IActionResult GetStudent(int id)
        {
            var item = _db.Students.FirstOrDefault(x => x.Id == id);
            if (item is null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpPost]
        public IActionResult CreateStudent(StudentCreateRequestModel requestModel)
        {
            _db.Students.Add(new Student
            {
                ClassNo = requestModel.ClassNo?.Trim(),
                Name = requestModel.Name.Trim(),
                No = requestModel.No.Trim(),
            });
            int result = _db.SaveChanges();
            string message = result > 0 ? "Saving Successful." : "Saving Failed.";

            return Ok(message);
        }

        [HttpPut("{id}")]
        public IActionResult UpsertStudent(int id, StudentUpsertRequestModel requestModel)
        {
            var item = _db.Students.FirstOrDefault(x => x.Id == id);
            if (item is null)
            {
                return NotFound();
            }

            item.No = requestModel.No.Trim();
            item.ClassNo = requestModel.ClassNo?.Trim();
            item.Name = requestModel.Name.Trim();

            int result = _db.SaveChanges();
            string message = result > 0 ? "Updating Successful." : "Updating Failed.";

            return Ok(message);
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateStudent(int id, StudentUpdateRequestModel requestModel)
        {
            var item = _db.Students.FirstOrDefault(x => x.Id == id);
            if (item is null)
            {
                return NotFound();
            }

            if (requestModel.No != null && !requestModel.No.IsNullOrEmpty())
            {
                item.No = requestModel.No.Trim();
            }

            if (requestModel.ClassNo != null && !requestModel.ClassNo.IsNullOrEmpty())
            {
                item.ClassNo = requestModel.ClassNo?.Trim();
            }

            if (requestModel.Name != null && !requestModel.Name.IsNullOrEmpty())
            {
                item.Name = requestModel.Name.Trim();
            }

            int result = _db.SaveChanges();
            string message = result > 0 ? "Updating Successful." : "Updating Failed.";

            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            var item = _db.Students.FirstOrDefault(x => x.Id == id);
            if (item is null)
            {
                return NotFound();
            }

            _db.Students.Remove(item);

            int result = _db.SaveChanges();
            string message = result > 0 ? "Deleting Successful." : "Deleting Failed.";

            return Ok(message);
        }
    }
}
