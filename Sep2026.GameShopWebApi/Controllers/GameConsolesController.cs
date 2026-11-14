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
            
            if (createRequest.CategoryId < 1)
                return BadRequest("Invalid Category Id");
            

            var validCategory = _db.ConsoleCategories
                .Include(g => g.GameConsoles)
                .FirstOrDefault(category => category.Id == createRequest.CategoryId);
            
            if (validCategory == null)
                return BadRequest("Invalid Category Id");

            //consoleNumber format : PS4-1,PS5-1 etc
            var consoleNumber = $"{validCategory.Name}-{validCategory.GameConsoles.Count() + 1}";
            var newGameConsole = new GameConsole
            {
                CategoryId = validCategory.Id,
                ConsoleNumber = consoleNumber,
                IsDeleted = false,
                CurrentStatus = "Available"

            };

            _db.GameConsoles.Add(newGameConsole);
            _db.SaveChanges();
            
            return Ok(_modelConverter.ConvertGameConsoleResponse(newGameConsole));

        }

        //update
        [HttpPatch("{id}")]
        public IActionResult UpdateGameConsole(int id , GameConsoleModel.UpdateRequest updateRequest)
        {

            if (updateRequest == null)
                return BadRequest("Must define a body");

            var targetConsole = _db.GameConsoles.FirstOrDefault(c => c.Id == id);            
            if (targetConsole == null)
                return NotFound("Console not found");
            
            //update consoleNumber
            if (!string.IsNullOrEmpty(updateRequest.ConsoleNumber))
            {
                targetConsole.ConsoleNumber = updateRequest.ConsoleNumber;
            }

            //update currentStatus
            if (!string.IsNullOrEmpty(updateRequest.CurrentStatus) && IsValidConsoleStatus(updateRequest.CurrentStatus))
            {

                targetConsole.CurrentStatus = updateRequest.CurrentStatus;
            }

            //update category
            //note : changing category creates new console number 
            //example : if ps5 to ps4,count the current max ( id + 1 ) of ps4 console.
            var validConsoleCategory = _db.ConsoleCategories.Include(category => category.GameConsoles).FirstOrDefault(c => c.Id == updateRequest.CategoryId);
            if (validConsoleCategory != null)
            {
                int consoleByCategoryCount = validConsoleCategory.GameConsoles.Count();
                string newConsoleNumber = $"{validConsoleCategory.Name}-{consoleByCategoryCount + 1}";
                
                targetConsole.CategoryId = validConsoleCategory.Id;
                targetConsole.ConsoleNumber = newConsoleNumber;
            }

            int status = _db.SaveChanges();
            return status > 0 ? Ok(_modelConverter.ConvertGameConsoleResponse(targetConsole)) : BadRequest("Fail to update.");

        }

        //delete (default soft delete )
        [HttpDelete("{id}")]
        public IActionResult DeleteGameConsole(int id , [FromQuery] bool isHardDelete = false)
        {
            var targetConsole = _db.GameConsoles.FirstOrDefault(g => g.Id == id);

            if (targetConsole == null)
                return NotFound($"Game console with Id {id} is not found.");

            //block delete operation if childs exsit.
            bool isBookingsExist = _db.Bookings
                .Any(booking => 
                    booking.ConsoleId == targetConsole.Id &&
                    booking.IsDeleted == false);

            bool isSessionsExist = _db.GameSessions
                .Any(gameSession => 
                    gameSession.ConsoleId == targetConsole.Id &&
                    gameSession.IsDeleted == false);

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
            return status == 0 ? BadRequest("Couldn't delete your request") : Ok("Successfully deleted");

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