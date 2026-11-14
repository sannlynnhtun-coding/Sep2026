using System;
using System.Collections.Generic;

namespace Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

public partial class Booking
{
    public int Id { get; set; }

    public int ConsoleId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal TotalHours { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal DepositAmount { get; set; }

    public string BookingStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual GameConsole Console { get; set; } = null!;

    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
