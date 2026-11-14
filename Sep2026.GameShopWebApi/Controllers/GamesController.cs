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
            using var transaction = _db.BeginSchedulingTransaction();
            if (createRequest == null)
                return BadRequest("Must define a body");

            if (string.IsNullOrWhiteSpace(createRequest.Title))
                return BadRequest("Game title must be defined");

            if (createRequest.TotalStock < 1)
                return BadRequest("Total stock must be at least 1");

            // validate category if provided
            if (createRequest.CategoryId.HasValue)
            {
                var validCategory = _db.ConsoleCategories.FirstOrDefault(c => c.Id == createRequest.CategoryId.Value && !c.IsDeleted);
                if (validCategory == null)
                    return BadRequest($"Invalid Category Id {createRequest.CategoryId}");
            }

            var newGame = new Game
            {
                Title = createRequest.Title.Trim(),
                CategoryId = createRequest.CategoryId,
                TotalStock = createRequest.TotalStock,
                AvailableStock = createRequest.TotalStock,
                IsDeleted = false
            };

            _db.Games.Add(newGame);
            int status = _db.SaveChanges();
            transaction.Commit();

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
            if (updateRequest.Title != null)
            {
                if (string.IsNullOrWhiteSpace(updateRequest.Title))
                    return BadRequest("Game title cannot be whitespace.");
                targetGame.Title = updateRequest.Title.Trim();
            }

            // update category
            if (updateRequest.CategoryId.HasValue)
            {
                var validCategory = _db.ConsoleCategories.FirstOrDefault(c => c.Id == updateRequest.CategoryId.Value && !c.IsDeleted);
                if (validCategory == null)
                    return BadRequest("Invalid Category Id.");
                targetGame.CategoryId = validCategory.Id;
            }

            int totalStock = updateRequest.TotalStock ?? targetGame.TotalStock;
            int availableStock = updateRequest.AvailableStock ?? targetGame.AvailableStock;
            int reservedStock = _db.GameSessions.Count(session => session.GameId == id && !session.IsDeleted);
            if (totalStock < reservedStock || availableStock < 0 || availableStock > totalStock - reservedStock)
                return BadRequest("Stock must cover active sessions and cannot exceed total stock.");
            targetGame.TotalStock = totalStock;
            targetGame.AvailableStock = availableStock;

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
            bool isSessionExist = _db.GameSessions.Any(s => s.GameId == id && (isHardDelete || !s.IsDeleted));

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
