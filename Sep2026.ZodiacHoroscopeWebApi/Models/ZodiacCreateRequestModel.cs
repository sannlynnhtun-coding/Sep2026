namespace Sep2026.ZodiacHoroscopeWebApi.Models
{
    public class ZodiacCreateRequestModel
    {
        public string ZodiacName { get; set; } = "";
        public string Symbol { get; set; } = "";
        public string Element { get; set; } = "";
        public string DateRange { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class ZodiacUpdateRequestModel
    {
        public string? ZodiacName { get; set; }
        public string? Symbol { get; set; }
        public string? Element { get; set; }
        public string? DateRange { get; set; }
        public string? Description { get; set; }
    }
}
 
