namespace Sep2026.ClinicPatientRecord.API.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;
    }
}