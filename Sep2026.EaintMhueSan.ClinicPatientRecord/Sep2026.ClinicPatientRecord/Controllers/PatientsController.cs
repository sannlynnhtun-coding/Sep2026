using Microsoft.Data.SqlClient;
﻿using Microsoft.AspNetCore.Mvc;
using Sep2026.ClinicPatientRecord.API.Dto;
using Sep2026.ClinicPatientRecord.API.Models;
using Sep2026.ClinicPatientRecord.API.Service;

namespace Sep2026.ClinicPatientRecord.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;

        public PatientsController(PatientService patientService)
        {
            _patientService = patientService;
        }

        // 1. Get All Patients
        [HttpGet]
        public async Task<ActionResult<List<Patient>>> GetAll()
        {
            var patients = await _patientService.GetAllAsync();

            return Ok(patients);
        }

        // 2. Get Patient By ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetById(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);

            if (patient == null)
            {
                return NotFound(new
                {
                    message = "Patient not found."
                });
            }

            return Ok(patient);
        }

        // 3. Register Patient
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] PatientCreateDto dto)
        {
            var newPatientId =
                await _patientService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = newPatientId },
                new
                {
                    message = "Patient registered successfully.",
                    patientId = newPatientId
                });
        }

        // 4. Update Patient
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] PatientUpdateDto dto)
        {
            var updated =
                await _patientService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Patient not found."
                });
            }

            return Ok(new
            {
                message = "Patient updated successfully."
            });
        }

        // 5. Delete Patient
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted;
            try
            {
                deleted = await _patientService.DeleteAsync(id);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return Conflict(new { message = "Patient has token records and cannot be deleted." });
            }

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Patient not found."
                });
            }

            return Ok(new
            {
                message = "Patient deleted successfully."
            });
        }
    }
}