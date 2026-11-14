using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.ZodiacHoroscope.Database.AppDbContextModels;
using Sep2026.ZodiacHoroscopeWebApi.Models;
using System.Globalization;

namespace Sep2026.ZodiacHoroscopeWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZodiacController : ControllerBase
    {
            private readonly AppDbContext _context;

            public ZodiacController(AppDbContext context)
            {
                _context = context;
            }

           
            [HttpGet]
            public IActionResult GetAllZodiac()
            {
                var zodiacList = _context.ZodiacSigns.ToList();

                return Ok(zodiacList);
            }

         
            [HttpGet("{id}")]
            public IActionResult GetZodiacById(int id)
            {
                var zodiac = _context.ZodiacSigns
                    .FirstOrDefault(x => x.ZodiacId == id);

                if (zodiac is null)
                {
                    return NotFound("Zodiac not found.");
                }

                return Ok(zodiac);
            }

           
            [HttpGet("birthday/{date}")]
            public IActionResult GetZodiacByBirthday(string date)
            {
                if (!DateOnly.TryParse(
                    date,
                    out DateOnly birthdayDate))
                {
                    return BadRequest("Invalid date format.");
                }

                int birthday = birthdayDate.Month * 100 + birthdayDate.Day;

                var zodiacList = _context.ZodiacSigns.ToList();

                foreach (var zodiac in zodiacList)
                {
                    string[] dateParts = zodiac.DateRange.Split(" - ");

                    if (dateParts.Length != 2)
                        continue;

                    if (!DateOnly.TryParseExact(
                        dateParts[0].Trim(),
                        "MMMM d",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateOnly startDate))
                        continue;

                    if (!DateOnly.TryParseExact(
                        dateParts[1].Trim(),
                        "MMMM d",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateOnly endDate))
                        continue;

                    int start = startDate.Month * 100 + startDate.Day;
                    int end = endDate.Month * 100 + endDate.Day;

                    if (start <= end)
                    {
                        if (birthday >= start && birthday <= end)
                        {
                            return Ok(zodiac);
                        }
                    }
                    else
                    {
                        if (birthday >= start || birthday <= end)
                        {
                            return Ok(zodiac);
                        }
                    }
                }

                return NotFound("Zodiac not found.");
            }

            [HttpPost]
            public IActionResult CreateZodiac(ZodiacCreateRequestModel request)
            {
                if (string.IsNullOrWhiteSpace(request.ZodiacName) ||
                    string.IsNullOrWhiteSpace(request.Symbol) ||
                    string.IsNullOrWhiteSpace(request.Element) ||
                    string.IsNullOrWhiteSpace(request.DateRange) ||
                    string.IsNullOrWhiteSpace(request.Description))
                {
                    return BadRequest("Please provide all Zodiac information.");
                }

                ZodiacSign zodiac = new ZodiacSign
                {
                    ZodiacName = request.ZodiacName,
                    Symbol = request.Symbol,
                    Element = request.Element,
                    DateRange = request.DateRange,
                    Description = request.Description
                };

                _context.ZodiacSigns.Add(zodiac);

                _context.SaveChanges();

                return Ok(zodiac);
            }

          
            [HttpPatch("{id}")]
            public IActionResult UpdateZodiac(int id, ZodiacUpdateRequestModel request)
            {
                var zodiac = _context.ZodiacSigns
                    .FirstOrDefault(x => x.ZodiacId == id);
                if (zodiac is null)
                {
                    return NotFound("Zodiac not found.");
                }

                if (request.ZodiacName != null)
                    zodiac.ZodiacName = request.ZodiacName;

                if (request.Symbol != null)
                    zodiac.Symbol = request.Symbol;

                if (request.Element != null)
                    zodiac.Element = request.Element;

                if (request.DateRange != null)
                    zodiac.DateRange = request.DateRange;

                if (request.Description != null)
                    zodiac.Description = request.Description;

                _context.SaveChanges();

                return Ok(zodiac);
            }

         
            [HttpDelete("{id}")]
            public IActionResult DeleteZodiac(int id)
            {
                var zodiac = _context.ZodiacSigns
                    .FirstOrDefault(x => x.ZodiacId == id);

                if (zodiac is null)
                {
                    return NotFound("Zodiac not found.");
                }

                bool hasHoroscope = _context.Horoscopes
                    .Any(x => x.ZodiacId == id);

                if (hasHoroscope)
                {
                    return BadRequest("Horoscope records exist for this Zodiac.");
                }

                _context.ZodiacSigns.Remove(zodiac);

                _context.SaveChanges();

                return Ok("Zodiac deleted successfully.");
            }
        }
    }

