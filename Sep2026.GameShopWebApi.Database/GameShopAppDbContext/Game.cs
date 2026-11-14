using System;
using System.Collections.Generic;

namespace Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

public partial class Game
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int? CategoryId { get; set; }

    public int TotalStock { get; set; }

    public int AvailableStock { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ConsoleCategory? Category { get; set; }

    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
