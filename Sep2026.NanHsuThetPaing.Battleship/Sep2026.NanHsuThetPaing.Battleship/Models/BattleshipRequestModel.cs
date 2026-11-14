namespace Sep2026.BattleShipWebApi.Models
{
    public class FireRequest
    {
        public int Row { get; set; }   // 0 - 9

        public int Col { get; set; }   // 0 - 9
    }

    public class PlaceShipsRequest
    {
        public List<ShipPlacement> Ships { get; set; } = new();
    }

    public class ShipPlacement
    {
        public string Type { get; set; } = null!; 

        public int Row { get; set; }

        public int Col { get; set; }

        public bool IsHorizontal { get; set; } 
    }
}
