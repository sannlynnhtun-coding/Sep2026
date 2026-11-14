using System.ComponentModel.DataAnnotations;

﻿namespace Sep2026.ZodiacHoroscopeWebApi.Models
{
    public class HoroscopeCreateRequest
    {
        [Range(1, int.MaxValue)]
        public int ZodiacId { get; set; }
        public DateOnly HoroscopeDate { get; set; }
        [StringLength(1000)]
        public string Overall { get; set; } = "";
        [StringLength(1000)]
        public string Love { get; set; } = "";
        [StringLength(1000)]
        public string Career { get; set; } = "";
        [StringLength(1000)]
        public string Health { get; set; } = "";
        [StringLength(1000)]
        public string Finance { get; set; } = "";
        [StringLength(50)]
        public string LuckyColor { get; set; } = "";
        [Range(0, int.MaxValue)]
        public int LuckyNumber { get; set; }
    }

    public class HoroscopeUpdateRequest
    {
        [Range(1, int.MaxValue)]
        public int? ZodiacId { get; set; }
        public DateOnly? HoroscopeDate { get; set; }
        [StringLength(1000)]
        public string? Overall { get; set; }
        [StringLength(1000)]
        public string? Love { get; set; }
        [StringLength(1000)]
        public string? Career { get; set; }
        [StringLength(1000)]
        public string? Health { get; set; }
        [StringLength(1000)]
        public string? Finance { get; set; }
        [StringLength(50)]
        public string? LuckyColor { get; set; }
        [Range(0, int.MaxValue)]
        public int? LuckyNumber { get; set; }
    }
}

