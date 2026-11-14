using System.ComponentModel.DataAnnotations;

namespace Sep2026.MagicNumberTrick.WebApi.Models;

public class RevealRequestModel
{
    [Required]
    [MinLength(7)]
    [MaxLength(7)]
    public List<CardAnswerRequestModel> Answers { get; set; } = [];
}

public class CardAnswerRequestModel
{
    [Range(1, 7)]
    public int CardNo { get; set; }

    public bool IsNumberPresent { get; set; }
}

public class RevealResponseModel
{
    public int Number { get; set; }
    public IReadOnlyCollection<int> MatchedCardNumbers { get; set; } = [];
}
