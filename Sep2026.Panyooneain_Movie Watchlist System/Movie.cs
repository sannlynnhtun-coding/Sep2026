using System.ComponentModel.DataAnnotations;

﻿
namespace Sep2026.Panyooneain_Movie_Watchlist_System
{
    public class Movie
    {
        public int MovieId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Genre { get; set; } = string.Empty;

        public int ReleaseYear { get; set; }

        public decimal Rating { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
