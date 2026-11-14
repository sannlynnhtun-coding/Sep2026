using Microsoft.AspNetCore.Mvc;
using Sep2026.ClinicPatientRecord.API.Dto;
using Sep2026.ClinicPatientRecord.API.Service;

namespace Sep2026.ClinicPatientRecord.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public TokensController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        // POST: api/tokens
        [HttpPost]
        public async Task<IActionResult> Generate(TokenCreateDto dto)
        {
            try
            {
                var token = await _tokenService.GenerateAsync(
                    dto.PatientID,
                    dto.DoctorID
                );

                return Ok(new
                {
                    success = true,
                    message = "Token generated successfully.",
                    tokenID = token.TokenID,
                    tokenNumber = token.TokenCode,
                    patientID = token.PatientID,
                    doctorID = token.DoctorID,
                    tokenDate = token.TokenDate
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}