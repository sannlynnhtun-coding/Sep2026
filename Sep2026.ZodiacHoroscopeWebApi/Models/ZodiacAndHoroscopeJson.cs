namespace Sep2026.ZodiacHoroscopeWebApi.Models
{
    public class ZodiacAndHoroscopeJson
    {

        public class ResponseModel
        {
            public Zodiacdata[] ZodiacData { get; set; }
                = Array.Empty<Zodiacdata>();

            public Horoscopedata[] HoroscopeData { get; set; }
                = Array.Empty<Horoscopedata>();
        }

        public class Zodiacdata
        {
            public int zodiacId { get; set; }
            public string zodiacName { get; set; } = "";
            public string symbol { get; set; } = "";
            public string element { get; set; } = "";
            public string dateRange { get; set; } = "";
            public string description { get; set; } = "";
        }

        public class Horoscopedata
        {
            public int horoscopeId { get; set; }
            public int zodiacId { get; set; }
            public string horoscopeDate { get; set; } = "";
            public string overall { get; set; } = "";
            public string love { get; set; } = "";
            public string career { get; set; } = "";
            public string health { get; set; } = "";
            public string finance { get; set; } = "";
            public string luckyColor { get; set; } = "";
            public int luckyNumber { get; set; }
        }
    }
}


