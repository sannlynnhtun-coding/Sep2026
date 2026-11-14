using System.ComponentModel.DataAnnotations;

namespace Sep2026.MagicNumberTrick.WebApi.Models;

public class CardCreateRequestModel
{
    [Range(1, 7)]
    public int CardNo { get; set; }

    [Range(1, 100)]
    public int KeyValue { get; set; }

    [Required]
    public string CardName { get; set; } = null!;

    [Required]
    public List<int> Numbers { get; set; } = [];
}

public class CardUpdateRequestModel
{
    [Range(1, 7)]
    public int? CardNo { get; set; }

    [Range(1, 100)]
    public int? KeyValue { get; set; }

    [MaxLength(100)]
    public string? CardName { get; set; }

    [MinLength(1)]
    public List<int>? Numbers { get; set; }
}
