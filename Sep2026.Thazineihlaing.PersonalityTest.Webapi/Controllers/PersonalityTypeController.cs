
using Microsoft.AspNetCore.Mvc;
using Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;
using static Sep2026.Thazineihlaing.PersonalityTest.Webapi.Models.PersonalityTypeRequestModels;

namespace Sep2026.Thazineihlaing.PersonalityTest.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalityTypesController : ControllerBase
    {
        private AppDbContext _db;

        public PersonalityTypesController()
        {
            _db = new AppDbContext();
        }

        [HttpGet]
        public IActionResult GetAllPersonalityTypes()
        {
            var lst = _db.PersonalityTypes.ToList();

            return Ok(lst);
        }

        [HttpGet("Edit/{id}")]
        public IActionResult GetPersonalityType(int id)
        {
            var personalityType = _db.PersonalityTypes.FirstOrDefault(x => x.PersonalityTypeId == id);

            if (personalityType is null)
            {
                return NotFound("");
            }

            return Ok(personalityType);
        }

        [HttpPost]
        public IActionResult CreatePersonalityType(PersonalityTypeCreateRequestModel requestModel)
        {
            _db.PersonalityTypes.Add(new PersonalityType
            {
                TypeName = requestModel.TypeName.Trim(),
                Description = requestModel.Description?.Trim()
            });

            int result = _db.SaveChanges();

            string message = result > 0
                ? "Create Personality Type Successful"
                : "Create Personality Type Failed.";

            return Ok(message);
        }

        [HttpPatch("{id}")]
        public IActionResult UpdatePersonalityType(int id, PersonalityTypeUpdateRequestModel requestModel)
        {
            var item = _db.PersonalityTypes.FirstOrDefault(x => x.PersonalityTypeId == id);

            if (item is null)
            {
                return NotFound();
            }

            if (requestModel.TypeName != null && !string.IsNullOrWhiteSpace(requestModel.TypeName))
            {
                item.TypeName = requestModel.TypeName.Trim();
            }

            if (requestModel.Description != null)
            {
                item.Description = requestModel.Description.Trim();
            }

            int result = _db.SaveChanges();

            string message = result > 0
                ? "Updating Successful."
                : "Updating Failed.";

            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePersonalityType(int id)
        {
            var personalityType = _db.PersonalityTypes.FirstOrDefault(x => x.PersonalityTypeId == id);

            if (personalityType is null)
            {
                return NotFound();
            }

            _db.PersonalityTypes.Remove(personalityType);

            int result = _db.SaveChanges();

            string message = result > 0
                ? "Deleting Successful."
                : "Deleting Failed.";

            return Ok(message);
        }
    }
}
