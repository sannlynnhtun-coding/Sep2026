using System;
using System.Collections.Generic;

namespace Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

public partial class ConsoleCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal HourlyRate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<GameConsole> GameConsoles { get; set; } = new List<GameConsole>();

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
