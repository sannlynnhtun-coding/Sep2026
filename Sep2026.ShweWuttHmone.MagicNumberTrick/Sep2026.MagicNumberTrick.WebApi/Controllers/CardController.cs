using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sep2026.MagicNumberTrick.Database.Models;
using Sep2026.MagicNumberTrick.WebApi.Models;

namespace Sep2026.MagicNumberTrick.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CardController : ControllerBase
{
    private readonly AppDbContext _db;

    public CardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetCards()
    {
        var cards = _db.MagicNumberCards
            .AsNoTracking()
            .OrderBy(x => x.CardNo)
            .Select(x => new CardResponseModel
            {
                CardId = x.CardId,
                CardNo = x.CardNo,
                KeyValue = x.KeyValue,
                CardName = x.CardName,
                Numbers = x.CardItems
                    .OrderBy(item => item.Number)
                    .Select(item => item.Number)
                    .ToList()
            }).ToList();

        return Ok(cards);
    }

    [HttpGet("{cardNo:int}")]
    public ActionResult GetCard(int cardNo)
    {
        var card = _db.MagicNumberCards
            .AsNoTracking()
            .Where(x => x.CardNo == cardNo)
            .Select(x => new CardResponseModel
            {
                CardId = x.CardId,
                CardNo = x.CardNo,
                KeyValue = x.KeyValue,
                CardName = x.CardName,
                Numbers = x.CardItems
                    .OrderBy(item => item.Number)
                    .Select(item => item.Number)
                    .ToList()
            }).FirstOrDefault();

        if (card is null)
        {
            return NotFound($"Card number '{cardNo}' was not found.");
        }

        return Ok(card);
    }

    [HttpPost]
    public IActionResult CreateCard(CardCreateRequestModel requestModel)
    {
        try
        {
            ValidateNumbers(requestModel.Numbers);

            if (_db.MagicNumberCards.Any(x => x.CardNo == requestModel.CardNo))
            {
                throw new InvalidOperationException($"Card number '{requestModel.CardNo}' already exists.");
            }

            var card = new MagicNumberCard
            {
                CardNo = requestModel.CardNo,
                KeyValue = requestModel.KeyValue,
                CardName = requestModel.CardName.Trim(),
                CardItems = requestModel.Numbers
                    .Distinct()
                    .OrderBy(number => number)
                    .Select(number => new MagicNumberCardItem { Number = number })
                    .ToList()
            };

            _db.MagicNumberCards.Add(card);
            _db.SaveChanges();
            var response = ToResponse(card);

            return CreatedAtAction(nameof(GetCard), new { cardNo = response.CardNo }, response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPatch("{cardId:int}")]
    public IActionResult UpdateCard(int cardId, CardUpdateRequestModel requestModel)
    {
        try
        {
            var card = _db.MagicNumberCards
                .Include(x => x.CardItems)
                .FirstOrDefault(x => x.CardId == cardId);

            if (card is null)
            {
                return NotFound($"Card ID '{cardId}' was not found.");
            }

            if (requestModel.CardNo.HasValue && requestModel.CardNo.Value != card.CardNo)
            {
                if (_db.MagicNumberCards.Any(
                            x => x.CardNo == requestModel.CardNo.Value &&
                            x.CardId != cardId))
                {
                    throw new InvalidOperationException($"Card number '{requestModel.CardNo.Value}' already exists.");
                }

                card.CardNo = requestModel.CardNo.Value;
            }

            if (requestModel.KeyValue.HasValue)
            {
                card.KeyValue = requestModel.KeyValue.Value;
            }

            if (requestModel.CardName is not null)
            {
                if (string.IsNullOrWhiteSpace(requestModel.CardName))
                {
                    throw new ArgumentException("CardName cannot be empty.");
                }

                card.CardName = requestModel.CardName.Trim();
            }

            if (requestModel.Numbers is not null)
            {
                ValidateNumbers(requestModel.Numbers);
                _db.MagicNumberCardItems.RemoveRange(card.CardItems);
                card.CardItems = requestModel.Numbers
                    .Distinct()
                    .OrderBy(number => number)
                    .Select(number => new MagicNumberCardItem
                    {
                        CardId = card.CardId,
                        Number = number
                    }).ToList();
            }

            _db.SaveChanges();
            return Ok(ToResponse(card));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpDelete("{cardId:int}")]
    public IActionResult DeleteCard(int cardId)
    {
        var card = _db.MagicNumberCards.FirstOrDefault(x => x.CardId == cardId);

        if (card is null)
        {
            return NotFound($"Card ID '{cardId}' was not found.");
        }

        _db.MagicNumberCards.Remove(card);
        _db.SaveChanges();
        return NoContent();
    }

    private static void ValidateNumbers(IEnumerable<int> numbers)
    {
        var values = numbers.ToList();

        if (values.Count == 0)
        {
            throw new ArgumentException("At least one card number is required.");
        }

        if (values.Any(number => number is < 1 or > 100))
        {
            throw new ArgumentException("Every number on a card must be between 1 and 100.");
        }

        if (values.Count != values.Distinct().Count())
        {
            throw new ArgumentException("A card cannot contain the same number more than once.");
        }
    }

    private static CardResponseModel ToResponse(MagicNumberCard card)
    {
        return new CardResponseModel
        {
            CardId = card.CardId,
            CardNo = card.CardNo,
            KeyValue = card.KeyValue,
            CardName = card.CardName,
            Numbers = card.CardItems
                .OrderBy(item => item.Number)
                .Select(item => item.Number)
                .ToList()
        };
    }
}
