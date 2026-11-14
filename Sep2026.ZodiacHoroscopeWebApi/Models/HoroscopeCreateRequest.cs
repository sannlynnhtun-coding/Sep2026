namespace Sep2026.ZodiacHoroscopeWebApi.Models
{
    public class HoroscopeCreateRequest
    {
        public int ZodiacId { get; set; }
        public DateOnly HoroscopeDate { get; set; }
        public string Overall { get; set; } = "";
        public string Love { get; set; } = "";
        public string Career { get; set; } = "";
        public string Health { get; set; } = "";
        public string Finance { get; set; } = "";
        public string LuckyColor { get; set; } = "";
        public int LuckyNumber { get; set; }
    }

    public class HoroscopeUpdateRequest
    {
        public int? ZodiacId { get; set; }
        public DateOnly? HoroscopeDate { get; set; }
        public string? Overall { get; set; }
        public string? Love { get; set; }
        public string? Career { get; set; }
        public string? Health { get; set; }
        public string? Finance { get; set; }
        public string? LuckyColor { get; set; }
        public int? LuckyNumber { get; set; }
    }
}

