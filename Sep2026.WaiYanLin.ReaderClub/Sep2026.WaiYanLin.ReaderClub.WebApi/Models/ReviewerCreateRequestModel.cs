using System.ComponentModel.DataAnnotations;
﻿using Sep2026.ReaderClub.Database.AppDbContextModels;
using System.Text.Json.Serialization;

namespace Sep2026.ReaderClub.WebApi.Models
{
    public class ReviewerCreateRequestModel
    {

        [Required, StringLength(100)]
        public string ReviewerName { get; set; } = null!;

        [Required]
        public string ProfileDescription { get; set; } = null!;

    }

    public class ReviewerUpdateRequestModel
    {

        [StringLength(100)]
        public string? ReviewerName { get; set; }

        public string? ProfileDescription { get; set; }

    }
}
