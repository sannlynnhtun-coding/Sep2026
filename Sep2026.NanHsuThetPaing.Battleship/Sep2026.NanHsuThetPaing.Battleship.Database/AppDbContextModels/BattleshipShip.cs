using System;
using System.Collections.Generic;

namespace Sep2026.NanHsuThetPaing.Battleship.Database.AppDbContextModels;

public partial class BattleshipShip
{
    public int ShipId { get; set; }

    public int GameId { get; set; }

    public bool IsPlayerShip { get; set; }

    public string Type { get; set; } = null!;

    public int Size { get; set; }

    public int StartRow { get; set; }

    public int StartCol { get; set; }

    public bool IsHorizontal { get; set; }

    public int Hits { get; set; }

    public bool IsSunk { get; set; }

    public virtual BattleshipGame Game { get; set; } = null!;
}
