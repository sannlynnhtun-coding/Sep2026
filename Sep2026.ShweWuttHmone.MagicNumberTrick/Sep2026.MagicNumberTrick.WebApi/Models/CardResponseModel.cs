namespace Sep2026.MagicNumberTrick.WebApi.Models;

public class CardResponseModel
{
    public int CardId { get; set; }
    public int CardNo { get; set; }
    public int KeyValue { get; set; }
    public string CardName { get; set; } = null;
    public IReadOnlyCollection<int> Numbers { get; set; } = [];
}
