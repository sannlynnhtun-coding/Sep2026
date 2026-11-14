using System;
using System.Collections.Generic;

namespace Sep2026.ZodiacHoroscope.Database.AppDbContextModels;

public partial class Horoscope
{
    public int HoroscopeId { get; set; }

    public int ZodiacId { get; set; }

    public DateOnly HoroscopeDate { get; set; }

    public string Overall { get; set; } = null!;

    public string Love { get; set; } = null!;

    public string Career { get; set; } = null!;

    public string Health { get; set; } = null!;

    public string Finance { get; set; } = null!;

    public string LuckyColor { get; set; } = null!;

    public int LuckyNumber { get; set; }

    public virtual ZodiacSign Zodiac { get; set; } = null!;
}
