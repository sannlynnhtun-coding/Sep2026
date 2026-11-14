namespace Sep2026.MagicNumberTrick.Database.Models;

public class MagicNumberCard
{
    public int CardId { get; set; }
    public int CardNo { get; set; }
    public int KeyValue { get; set; }
    public string CardName { get; set; } = null!;
    public virtual ICollection<MagicNumberCardItem> CardItems { get; set; } = new List<MagicNumberCardItem>();
}
