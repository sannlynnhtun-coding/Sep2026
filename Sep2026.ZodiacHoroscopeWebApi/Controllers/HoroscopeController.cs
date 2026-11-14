using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.ZodiacHoroscope.Database.AppDbContextModels;
using Sep2026.ZodiacHoroscopeWebApi.Models;

namespace Sep2026.ZodiacHoroscopeWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoroscopeController : ControllerBase
    {
            private readonly AppDbContext _context;

            public HoroscopeController(AppDbContext context)
            {
                _context = context;
            }

            [HttpGet]
            public IActionResult GetAllHoroscopes()
            {
                var horoscopeList = _context.Horoscopes.ToList();

                return Ok(horoscopeList);
            }

            [HttpGet("{id}")]
            public IActionResult GetHoroscopeById(int id)
            {
                var horoscope = _context.Horoscopes
                    .FirstOrDefault(x => x.HoroscopeId == id);

                if (horoscope is null)
                {
                    return NotFound("Horoscope not found.");
                }

                return Ok(horoscope);
            }

            
            [HttpGet("daily/{zodiacId}")]
            public IActionResult GetDailyHoroscope(int zodiacId)
            {
                var zodiac = _context.ZodiacSigns
                    .FirstOrDefault(x => x.ZodiacId == zodiacId);

                if (zodiac is null)
                {
                    return NotFound("Zodiac not found.");
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.Today);

                var horoscope = _context.Horoscopes
                    .FirstOrDefault(x =>
                        x.ZodiacId == zodiacId &&
                        x.HoroscopeDate == today);

                if (horoscope is null)
                {
                    return NotFound("Today's horoscope is not available.");
                }

                return Ok(horoscope);
            }

            [HttpGet("date/{date}")]
            public IActionResult GetHoroscopeByDate(DateOnly date)
            {
                var horoscopeList = _context.Horoscopes
                    .Where(x => x.HoroscopeDate == date)
                    .ToList();

                if (horoscopeList.Count == 0)
                {
                    return NotFound("No horoscope found for this date.");
                }

                return Ok(horoscopeList);
            }
           
            [HttpPost]
            public IActionResult CreateHoroscope(HoroscopeCreateRequest request)
            {
                
                var zodiac = _context.ZodiacSigns
                    .FirstOrDefault(x => x.ZodiacId == request.ZodiacId);

                if (zodiac == null)
                {
                    return NotFound("Zodiac not found.");
                }

               
                if (request.LuckyNumber < 0)
                {
                    return BadRequest("LuckyNumber cannot be negative.");
                }

               
                Horoscope horoscope = new Horoscope
                {
                    ZodiacId = request.ZodiacId,
                    HoroscopeDate = request.HoroscopeDate,
                    Overall = request.Overall,
                    Love = request.Love,
                    Career = request.Career,
                    Health = request.Health,
                    Finance = request.Finance,
                    LuckyColor = request.LuckyColor,
                    LuckyNumber = request.LuckyNumber
                };

                _context.Horoscopes.Add(horoscope);
                _context.SaveChanges();

                return Ok(horoscope);
            }
          
            [HttpPatch("{id}")]
            public IActionResult UpdateHoroscope(int id, HoroscopeUpdateRequest request)
            {
                var horoscope = _context.Horoscopes
                    .FirstOrDefault(x => x.HoroscopeId == id);

                if (horoscope is null)
                {
                    return NotFound("Horoscope not found.");
                }
                if (request.ZodiacId.HasValue)
                {
                    var zodiac = _context.ZodiacSigns
                        .FirstOrDefault(x => x.ZodiacId == request.ZodiacId.Value);

                    if (zodiac is null)
                    {
                        return BadRequest("Zodiac not found.");
                    }

                    horoscope.ZodiacId = request.ZodiacId.Value;
                }

                if (request.HoroscopeDate.HasValue)
                    horoscope.HoroscopeDate = request.HoroscopeDate.Value;

                if (request.Overall != null)
                    horoscope.Overall = request.Overall;

                if (request.Love != null)
                    horoscope.Love = request.Love;

                if (request.Career != null)
                    horoscope.Career = request.Career;

                if (request.Health != null)
                    horoscope.Health = request.Health;

                if (request.Finance != null)
                    horoscope.Finance = request.Finance;

                if (request.LuckyColor != null)
                    horoscope.LuckyColor = request.LuckyColor;

                if (request.LuckyNumber.HasValue)
                {
                    if (request.LuckyNumber.Value < 0)
                    {
                        return BadRequest("LuckyNumber cannot be negative.");
                    }

                    horoscope.LuckyNumber = request.LuckyNumber.Value;
                }

                _context.SaveChanges();

                return Ok(horoscope);
            }

           
            [HttpDelete("{id}")]
            public IActionResult DeleteHoroscope(int id)
            {
                var horoscope = _context.Horoscopes
                    .FirstOrDefault(x => x.HoroscopeId == id);

                if (horoscope is null)
                {
                    return NotFound("Horoscope not found.");
                }

                _context.Horoscopes.Remove(horoscope);

                _context.SaveChanges();

                return Ok("Horoscope deleted successfully.");
            }
        }
    }

