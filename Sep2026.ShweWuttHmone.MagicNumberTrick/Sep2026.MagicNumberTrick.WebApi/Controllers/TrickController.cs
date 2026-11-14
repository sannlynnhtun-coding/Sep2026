using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sep2026.MagicNumberTrick.Database.Models;
using Sep2026.MagicNumberTrick.WebApi.Models;

namespace Sep2026.MagicNumberTrick.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrickController : ControllerBase
{
    private readonly AppDbContext _db;

    public TrickController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("reveal")]
    public IActionResult RevealNumber(RevealRequestModel requestModel)
    {
        try
        {
            var suppliedCardNumbers = requestModel.Answers
                .Select(x => x.CardNo)
                .ToList();

            if (suppliedCardNumbers.Distinct().Count() != 7 ||
                suppliedCardNumbers.Any(cardNo => cardNo is < 1 or > 7))
            {
                throw new ArgumentException("Answers must contain each card number from 1 through 7 exactly once.");
            }

            var matchedCardNumbers = requestModel.Answers
                .Where(x => x.IsNumberPresent)
                .Select(x => x.CardNo)
                .OrderBy(x => x)
                .ToList();

            var number = _db.MagicNumberCards
                .AsNoTracking()
                .Where(x => matchedCardNumbers.Contains(x.CardNo))
                .Sum(x => x.KeyValue);

            if (number is < 1 or > 100)
            {
                throw new ArgumentException("The selected answers do not produce a number between 1 and 100.");
            }

            var result = new RevealResponseModel
            {
                Number = number,
                MatchedCardNumbers = matchedCardNumbers
            };

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
