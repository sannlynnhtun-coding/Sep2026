using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.GameShopWebApi.Models;
using Sep2026.GameShopWebApi.Utils;
using Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private GameShopAppDbContext _db;
        private ModelConverter _modelConverter;

        public GamesController()
        {
            _db = new GameShopAppDbContext();
            _modelConverter = new ModelConverter();
        }

        // GET all games
        [HttpGet]
        public IActionResult GetAllGames()
        {
            var games = _db.Games.ToList();
            var response = games.Select(_modelConverter.ConvertGameResponse);
            return Ok(response);
        }

        // GET game by Id
        [HttpGet("{id}")]
        public IActionResult GetGameById(int id)
        {
            var game = _db.Games.FirstOrDefault(g => g.Id == id);
            if (game == null)
                return NotFound($"Game with Id {id} doesn't exist.");

            return Ok(_modelConverter.ConvertGameResponse(game));
        }

        // POST create game
        [HttpPost]
        public IActionResult CreateGame(GamesModel.CreateRequest createRequest)
        {
            if (createRequest == null)
                return BadRequest("Must define a body");

            if (string.IsNullOrEmpty(createRequest.Title))
                return BadRequest("Game title must be defined");

            if (createRequest.TotalStock < 1)
                return BadRequest("Total stock must be at least 1");

            // validate category if provided
            if (createRequest.CategoryId.HasValue)
            {
                var validCategory = _db.ConsoleCategories.FirstOrDefault(c => c.Id == createRequest.CategoryId.Value);
                if (validCategory == null)
                    return BadRequest($"Invalid Category Id {createRequest.CategoryId}");
            }

            var newGame = new Game
            {
                Title = createRequest.Title,
                CategoryId = createRequest.CategoryId,
                TotalStock = createRequest.TotalStock,
                AvailableStock = createRequest.TotalStock,
                IsDeleted = false
            };

            _db.Games.Add(newGame);
            int status = _db.SaveChanges();

            return status > 0 ? Ok(_modelConverter.ConvertGameResponse(newGame)) : BadRequest("Failed to create game");
        }

        // PATCH update game
        [HttpPatch("{id}")]
        public IActionResult UpdateGame(int id, GamesModel.UpdateRequest updateRequest)
        {
            using var transaction = _db.BeginSchedulingTransaction();
            if (updateRequest == null)
                return BadRequest("Must define a body");

            var targetGame = _db.Games.FirstOrDefault(g => g.Id == id);
            if (targetGame == null)
                return NotFound($"Game with Id {id} doesn't exist.");

            // update title
            if (!string.IsNullOrEmpty(updateRequest.Title))
            {
                targetGame.Title = updateRequest.Title;
            }

            // update category
            if (updateRequest.CategoryId.HasValue)
            {
                var validCategory = _db.ConsoleCategories.FirstOrDefault(c => c.Id == updateRequest.CategoryId.Value);
                if (validCategory != null)
                {
                    targetGame.CategoryId = validCategory.Id;
                }
            }

            // update total stock
            if (updateRequest.TotalStock.HasValue && updateRequest.TotalStock.Value >= 0)
            {
                targetGame.TotalStock = updateRequest.TotalStock.Value;
            }
            

            // update available stock
            if (updateRequest.AvailableStock.HasValue && updateRequest.AvailableStock.Value >= 0)
            {
                if (updateRequest.TotalStock.HasValue && updateRequest.AvailableStock.Value > updateRequest.TotalStock.Value)
                    return BadRequest("Please check your available stock number.");
                
                targetGame.AvailableStock = updateRequest.AvailableStock.Value;
            }

            int status = _db.SaveChanges();
            transaction.Commit();
            return status > 0 ? Ok(_modelConverter.ConvertGameResponse(targetGame)) : BadRequest("Failed to update game");
        }

        // DELETE game (default soft delete)
        [HttpDelete("{id}")]
        public IActionResult DeleteGame(int id, [FromQuery] bool isHardDelete = false)
        {
            using var transaction = _db.BeginSchedulingTransaction();
            var targetGame = _db.Games.FirstOrDefault(g => g.Id == id);
            if (targetGame == null)
                return NotFound($"Game with Id {id} doesn't exist.");

            //block delete if child exist.
            bool isSessionExist = _db.GameSessions.Any(s => s.GameId == id && s.IsDeleted == false);

            if (isSessionExist)
                return BadRequest($"Game '{targetGame.Title}' has game session. Try to move or delete them first.");

            //delete
            if (isHardDelete)
            {
                _db.Games.Remove(targetGame);
            }
            else
            {
                targetGame.IsDeleted = true;
            }

            int status = _db.SaveChanges();
            transaction.Commit();
            return status > 0 ? Ok("Successfully deleted") : BadRequest("Failed to delete game");
        }
    }
}
