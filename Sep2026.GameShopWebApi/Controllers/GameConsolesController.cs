using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sep2026.GameShopWebApi.Models;
using Sep2026.GameShopWebApi.Utils;
using Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameConsolesController : ControllerBase
    {
        private  GameShopAppDbContext _db;
        private ModelConverter _modelConverter; //entity model to dto model converter

        public GameConsolesController()
        {
            _db = new GameShopAppDbContext();
            _modelConverter = new ModelConverter();
        }


        //view all 
        [HttpGet]
        public IActionResult AllGameConsoles()
        {
            var gameConsoles = _db.GameConsoles.ToList();
            if (gameConsoles.IsNullOrEmpty())
                return NoContent();

            var response = gameConsoles.Select(_modelConverter.ConvertGameConsoleResponse);

            return Ok(response);
        }

        //get by Id
        [HttpGet("{id}")]
        public IActionResult GetGameConsoleById(int id)
        {
            var gameConsole = _db.GameConsoles.FirstOrDefault(game => game.Id == id);
            if (gameConsole == null)
                return NotFound("No Result");

            return Ok(_modelConverter.ConvertGameConsoleResponse(gameConsole));
        }

        [HttpPost]
        public IActionResult CreateGameConsole(GameConsoleModel.CreateRequest createRequest)
        {
            using var transaction = _db.BeginSchedulingTransaction();
            
            if (createRequest.CategoryId < 1)
                return BadRequest("Invalid Category Id");
            

            var validCategory = _db.ConsoleCategories
                .FirstOrDefault(category => category.Id == createRequest.CategoryId && !category.IsDeleted);
            
            if (validCategory == null)
                return BadRequest("Invalid Category Id");

            //consoleNumber format : PS4-1,PS5-1 etc
            var consoleNumber = NextConsoleNumber(validCategory.Name);
            var newGameConsole = new GameConsole
            {
                CategoryId = validCategory.Id,
                ConsoleNumber = consoleNumber,
                IsDeleted = false,
                CurrentStatus = "Available"

            };

            _db.GameConsoles.Add(newGameConsole);
            _db.SaveChanges();
            transaction.Commit();
            return Ok(_modelConverter.ConvertGameConsoleResponse(newGameConsole));

        }

        //update
        [HttpPatch("{id}")]
        public IActionResult UpdateGameConsole(int id , GameConsoleModel.UpdateRequest updateRequest)
        {
            using var transaction = _db.BeginSchedulingTransaction();

            if (updateRequest == null)
                return BadRequest("Must define a body");

            var targetConsole = _db.GameConsoles.FirstOrDefault(c => c.Id == id);            
            if (targetConsole == null)
                return NotFound("Console not found");
            
            //update consoleNumber
            if (updateRequest.ConsoleNumber != null)
            {
                string consoleNumber = updateRequest.ConsoleNumber.Trim();
                if (consoleNumber.Length == 0 || consoleNumber.Length > 20)
                    return BadRequest("Console number must contain 1 to 20 characters.");
                if (_db.GameConsoles.Any(console => console.Id != id && console.ConsoleNumber == consoleNumber))
                    return Conflict("Console number already exists.");
                targetConsole.ConsoleNumber = consoleNumber;
            }

            //update currentStatus
            if (updateRequest.CurrentStatus != null)
            {
                if (!IsValidConsoleStatus(updateRequest.CurrentStatus))
                    return BadRequest("Invalid console status.");
                targetConsole.CurrentStatus = updateRequest.CurrentStatus;
            }

            //update category
            // Changing category allocates an unused console number.
            if (updateRequest.CategoryId.HasValue && updateRequest.CategoryId.Value < 1)
                return BadRequest("Invalid Category Id.");
            if (updateRequest.CategoryId.HasValue && updateRequest.CategoryId.Value != targetConsole.CategoryId)
            {
                var validCategory = _db.ConsoleCategories.FirstOrDefault(category => category.Id == updateRequest.CategoryId && !category.IsDeleted);
                if (validCategory == null)
                    return BadRequest("Invalid Category Id.");
                targetConsole.CategoryId = validCategory.Id;
                targetConsole.ConsoleNumber = NextConsoleNumber(validCategory.Name);
            }

            int status = _db.SaveChanges();
            transaction.Commit();
            return status > 0 ? Ok(_modelConverter.ConvertGameConsoleResponse(targetConsole)) : BadRequest("Fail to update.");

        }

        //delete (default soft delete )
        [HttpDelete("{id}")]
        public IActionResult DeleteGameConsole(int id , [FromQuery] bool isHardDelete = false)
        {
            using var transaction = _db.BeginSchedulingTransaction();
            var targetConsole = _db.GameConsoles.FirstOrDefault(g => g.Id == id);

            if (targetConsole == null)
                return NotFound($"Game console with Id {id} is not found.");

            //block delete operation if childs exsit.
            bool isBookingsExist = _db.Bookings
                .Any(booking => 
                    booking.ConsoleId == targetConsole.Id &&
                    (isHardDelete || !booking.IsDeleted));

            bool isSessionsExist = _db.GameSessions
                .Any(gameSession => 
                    gameSession.ConsoleId == targetConsole.Id &&
                    (isHardDelete || !gameSession.IsDeleted));

            if (isBookingsExist || isSessionsExist)
                return BadRequest($"Console with Id {id} having  bookings or sessions.\nTry to delete or move them first.");
            
            //delete
            if (isHardDelete)
            {
                _db.GameConsoles.Remove(targetConsole);
            }
   
            else
                targetConsole.IsDeleted = true;

            int status = _db.SaveChanges();
            transaction.Commit();
            return status == 0 ? BadRequest("Couldn't delete your request") : Ok("Successfully deleted");

        }


        private string NextConsoleNumber(string categoryName)
        {
            int number = 1;
            while (true)
            {
                string suffix = "-" + number;
                string candidate = categoryName[..Math.Min(categoryName.Length, 20 - suffix.Length)] + suffix;
                if (!_db.GameConsoles.Any(console => console.ConsoleNumber == candidate))
                    return candidate;
                number++;
            }
        }

        //helper function to check enum
        private bool IsValidConsoleStatus(string inputType)
        {
            List<GameConsoleModel.CurrentStatusTypes> types = Enum.GetValues<GameConsoleModel.CurrentStatusTypes>().ToList();
            bool isValid = types.Any( t => t.ToString().Equals(inputType,StringComparison.OrdinalIgnoreCase));
            return isValid;
        }
    }
}