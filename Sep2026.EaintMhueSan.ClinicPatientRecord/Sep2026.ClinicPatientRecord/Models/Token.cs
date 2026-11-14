namespace Sep2026.ClinicPatientRecord.API.Models
{
    public class Token
    {
        public int TokenID { get; set; }

        public int PatientID { get; set; }

        public int DoctorID { get; set; }

        public int TokenNumber { get; set; }

        public DateTime TokenDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public string TokenCode =>
            $"T{TokenNumber:D3}";
    }
}