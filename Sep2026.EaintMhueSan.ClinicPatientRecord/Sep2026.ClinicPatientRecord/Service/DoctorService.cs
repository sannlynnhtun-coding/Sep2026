using Sep2026.ClinicPatientRecord.API.Models;
using Sep2026.ClinicPatientRecord.API.Repositories;

namespace Sep2026.ClinicPatientRecord.API.Service
{
    public class DoctorService
    {
        private readonly DoctorRepository _doctorRepository;

        public DoctorService(DoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        // Get all doctors
        public async Task<List<Doctor>> GetAllAsync()
        {
            return await _doctorRepository.GetAllAsync();
        }
    }
}