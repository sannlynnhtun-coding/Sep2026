using System;
using System.Collections.Generic;

namespace Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

public partial class GameSession
{
    public int Id { get; set; }

    public int? BookingId { get; set; }

    public int ConsoleId { get; set; }

    public int? GameId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal TotalHours { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual GameConsole Console { get; set; } = null!;

    public virtual Game? Game { get; set; }
}
