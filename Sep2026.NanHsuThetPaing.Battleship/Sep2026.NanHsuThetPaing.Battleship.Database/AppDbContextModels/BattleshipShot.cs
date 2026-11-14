using System;
using System.Collections.Generic;

namespace Sep2026.NanHsuThetPaing.Battleship.Database.AppDbContextModels;

public partial class BattleshipShot
{
    public int ShotId { get; set; }

    public int GameId { get; set; }

    public bool ByComputer { get; set; }

    public int Row { get; set; }

    public int Col { get; set; }

    public string Result { get; set; } = null!;

    public virtual BattleshipGame Game { get; set; } = null!;
}
