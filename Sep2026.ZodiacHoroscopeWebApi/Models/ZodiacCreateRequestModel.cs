using System.ComponentModel.DataAnnotations;

﻿namespace Sep2026.ZodiacHoroscopeWebApi.Models
{
    public class ZodiacCreateRequestModel
    {
        [Required]
        [StringLength(50)]
        public string ZodiacName { get; set; } = "";
        [Required]
        [StringLength(10)]
        public string Symbol { get; set; } = "";
        [Required]
        [StringLength(20)]
        public string Element { get; set; } = "";
        [Required]
        [StringLength(50)]
        public string DateRange { get; set; } = "";
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = "";
    }

    public class ZodiacUpdateRequestModel
    {
        [StringLength(50)]
        public string? ZodiacName { get; set; }
        [StringLength(10)]
        public string? Symbol { get; set; }
        [StringLength(20)]
        public string? Element { get; set; }
        [StringLength(50)]
        public string? DateRange { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
    }
}
 
