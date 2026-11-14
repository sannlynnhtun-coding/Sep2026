using Microsoft.AspNetCore.Mvc;
using Sep2026.ClinicPatientRecord.API.Service;

namespace Sep2026.ClinicPatientRecord.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorService _doctorService;

        public DoctorsController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // GET: api/doctors
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _doctorService.GetAllAsync();

            return Ok(doctors);
        }
    }
}