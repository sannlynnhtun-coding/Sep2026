using System;
using System.Collections.Generic;

namespace Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

public partial class GameConsole
{
    public int Id { get; set; }

    public string ConsoleNumber { get; set; } = null!;

    public int CategoryId { get; set; }

    public string CurrentStatus { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ConsoleCategory Category { get; set; } = null!;

    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
