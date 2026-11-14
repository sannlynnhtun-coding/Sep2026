using Sep2026.ClinicPatientRecord.API.Dto;
using Sep2026.ClinicPatientRecord.API.Models;
using Sep2026.ClinicPatientRecord.API.Repositories;

namespace Sep2026.ClinicPatientRecord.API.Service
{
    public class PatientService
    {
        private readonly PatientRepository _patientRepository;

        public PatientService(PatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        // 1. Get All Patients
        public async Task<List<Patient>> GetAllAsync()
        {
            return await _patientRepository.GetAllAsync();
        }

        // 2. Get Patient By ID
        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _patientRepository.GetByIdAsync(id);
        }

        // 3. Register Patient
        public async Task<int> CreateAsync(PatientCreateDto dto)
        {
            var patient = new Patient
            {
                FullName = dto.FullName,
                Age = dto.Age,
                Gender = dto.Gender,
                Phone = dto.Phone,
                Address = dto.Address
            };

            return await _patientRepository.CreateAsync(patient);
        }

        // 4. Update Patient
        public async Task<bool> UpdateAsync(
            int id, PatientUpdateDto dto)
        {
            var patient = new Patient
            {
                FullName = dto.FullName,
                Age = dto.Age,
                Gender = dto.Gender,
                Phone = dto.Phone,
                Address = dto.Address
            };

            return await _patientRepository.UpdateAsync(id, patient);
        }

        // 5. Delete Patient
        public async Task<bool> DeleteAsync(int id)
        {
            return await _patientRepository.DeleteAsync(id);
        }
    }
}