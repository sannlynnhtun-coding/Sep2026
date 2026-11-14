using System;
using System.Collections.Generic;

namespace Sep2026.NanHsuThetPaing.Battleship.Database.AppDbContextModels;

public partial class BattleshipGame
{
    public int GameId { get; set; }

    public string Status { get; set; } = null!;

    public int ShotsUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<BattleshipShip> BattleshipShips { get; set; } = new List<BattleshipShip>();

    public virtual ICollection<BattleshipShot> BattleshipShots { get; set; } = new List<BattleshipShot>();
}
