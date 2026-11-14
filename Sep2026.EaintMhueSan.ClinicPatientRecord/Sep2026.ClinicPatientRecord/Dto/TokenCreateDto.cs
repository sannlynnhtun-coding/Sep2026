using System.ComponentModel.DataAnnotations;

namespace Sep2026.ClinicPatientRecord.API.Dto
{
    public class TokenCreateDto
    {
        [Range(1, int.MaxValue)]
        public int PatientID { get; set; }

        [Range(1, int.MaxValue)]
        public int DoctorID { get; set; }
    }
}