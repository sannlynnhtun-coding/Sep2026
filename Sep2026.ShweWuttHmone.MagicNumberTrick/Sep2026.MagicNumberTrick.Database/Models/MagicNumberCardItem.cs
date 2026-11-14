namespace Sep2026.MagicNumberTrick.Database.Models;

public class MagicNumberCardItem
{
    public int CardItemId { get; set; }
    public int CardId { get; set; }
    public int Number { get; set; }
    public MagicNumberCard Card { get; set; } = null;
}
