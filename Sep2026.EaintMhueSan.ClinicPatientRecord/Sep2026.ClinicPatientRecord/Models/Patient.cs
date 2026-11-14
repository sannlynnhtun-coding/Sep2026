namespace Sep2026.ClinicPatientRecord.API.Models
{
    public class Patient
    {
        public int PatientID { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int Age { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateTime RegistrationDate { get; set; }
    }
}