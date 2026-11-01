namespace Sep2026.WebApi.Models
{
    public class StudentCreateRequestModel
    {
        public string No { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? ClassNo { get; set; }
    }

    public class StudentUpsertRequestModel
    {
        public string No { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? ClassNo { get; set; }
    }
}
