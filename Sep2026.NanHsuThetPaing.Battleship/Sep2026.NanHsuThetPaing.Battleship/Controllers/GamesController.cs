using Microsoft.EntityFrameworkCore;
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.BattleShipWebApi.Models;
using Sep2026.NanHsuThetPaing.Battleship.Database.AppDbContextModels;

namespace Sep2026.BattleShipWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly AppDbContext _db;

        private const int BoardSize = 10;

        private static readonly (string Type, int Size)[] Fleet =
        {
            ("Carrier", 5),
            ("Battleship", 4),
            ("Cruiser", 3),
            ("Submarine", 3),
            ("Destroyer", 2)
        };

        public GamesController()
        {
            _db = new AppDbContext();
        }

        /// <summary>Start a new game. The computer hides its ships. Then place yours.</summary>

        [HttpPost]
        public IActionResult CreateGame()
        {
            using var transaction = _db.Database.BeginTransaction();
            var game = new BattleshipGame
            {
                Status = "Placing",
                ShotsUsed = 0,
                CreatedAt = DateTime.Now
            };
            _db.BattleshipGames.Add(game);
            _db.SaveChanges(); 

            _db.BattleshipShips.AddRange(PlaceShipsRandomly(game.GameId, isPlayerShip: false));
            _db.SaveChanges();
            transaction.Commit();

            return CreatedAtAction(nameof(GetGame), new { id = game.GameId }, new
            {
                gameId = game.GameId,
                status = game.Status,
                message = "Game created! The computer has hidden its ships. " +
                          $"Now place yours with POST /api/games/{game.GameId}/ships " +
                          $"(or POST /api/games/{game.GameId}/ships/random).",
                ships = Fleet.Select(f => $"{f.Type} ({f.Size})")
            });
        }

        /// <summary>Place your 5 ships on your board. Each ship goes right (horizontal) or down from its start cell.</summary>

        [HttpPost("{id}/ships")]
        public IActionResult PlaceShips(int id, PlaceShipsRequest requestModel)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT GameId FROM BattleshipGame WITH (UPDLOCK, HOLDLOCK) WHERE GameId = {id}");
            var game = _db.BattleshipGames.FirstOrDefault(x => x.GameId == id);
            if (game is null)
            {
                return NotFound($"Game {id} not found.");
            }

            if (game.Status != "Placing")
            {
                return BadRequest("Your ships are already placed.");
            }

            var expected = Fleet.Select(f => f.Type).OrderBy(t => t).ToList();
            var given = requestModel.Ships.Select(s => s.Type).OrderBy(t => t).ToList();
            if (!expected.SequenceEqual(given, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Send exactly these 5 ships, once each: " + string.Join(", ", Fleet.Select(f => f.Type)));
            }

            var placed = new List<BattleshipShip>();
            foreach (var p in requestModel.Ships)
            {
                var (type, size) = Fleet.First(f => f.Type.Equals(p.Type, StringComparison.OrdinalIgnoreCase));

                var ship = new BattleshipShip
                {
                    GameId = id,
                    IsPlayerShip = true,
                    Type = type,
                    Size = size,
                    StartRow = p.Row,
                    StartCol = p.Col,
                    IsHorizontal = p.IsHorizontal,
                    Hits = 0,
                    IsSunk = false
                };

                for (int i = 0; i < size; i++)
                {
                    int r = p.IsHorizontal ? p.Row : p.Row + i;
                    int c = p.IsHorizontal ? p.Col + i : p.Col;

                    if (r < 0 || r >= BoardSize || c < 0 || c >= BoardSize)
                    {
                        return BadRequest($"{type} goes off the board. It needs {size} cells from ({p.Row}, {p.Col}).");
                    }

                    var other = placed.FirstOrDefault(s => Covers(s, r, c));
                    if (other is not null)
                    {
                        return BadRequest($"{type} overlaps the {other.Type} at ({r}, {c}).");
                    }
                }

                placed.Add(ship);
            }

            _db.BattleshipShips.AddRange(placed);
            game.Status = "Playing";
            _db.SaveChanges();
            transaction.Commit();

            return Ok(new
            {
                status = game.Status,
                message = "Your ships are placed. Fire with POST /api/games/" + id + "/fire",
                yourBoard = DrawBoard(placed, new List<BattleshipShot>(), showShips: true)
            });
        }

        /// <summary>Don't want to choose? Place your 5 ships at random.</summary>

        [HttpPost("{id}/ships/random")]
        public IActionResult PlaceShipsRandom(int id)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT GameId FROM BattleshipGame WITH (UPDLOCK, HOLDLOCK) WHERE GameId = {id}");
            var game = _db.BattleshipGames.FirstOrDefault(x => x.GameId == id);
            if (game is null)
            {
                return NotFound($"Game {id} not found.");
            }

            if (game.Status != "Placing")
            {
                return BadRequest("Your ships are already placed.");
            }

            var placed = PlaceShipsRandomly(id, isPlayerShip: true);
            _db.BattleshipShips.AddRange(placed);
            game.Status = "Playing";
            _db.SaveChanges();
            transaction.Commit();

            return Ok(new
            {
                status = game.Status,
                message = "Your ships were placed at random. Fire with POST /api/games/" + id + "/fire",
                yourBoard = DrawBoard(placed, new List<BattleshipShot>(), showShips: true)
            });
        }

        /// <summary>Your turn: fire at the computer's board. The computer then fires back at yours.</summary>

        [HttpPost("{id}/fire")]
        public IActionResult Fire(int id, FireRequest requestModel)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT GameId FROM BattleshipGame WITH (UPDLOCK, HOLDLOCK) WHERE GameId = {id}");
            var game = _db.BattleshipGames.FirstOrDefault(x => x.GameId == id);
            if (game is null)
            {
                return NotFound($"Game {id} not found.");
            }

            if (game.Status == "Placing")
            {
                return BadRequest("Place your ships first.");
            }

            if (game.Status != "Playing")
            {
                return BadRequest($"This game is finished ({game.Status}). Start a new game or use PUT /api/games/{id}/restart.");
            }

            if (requestModel.Row < 0 || requestModel.Row >= BoardSize || requestModel.Col < 0 || requestModel.Col >= BoardSize)
            {
                return BadRequest("Row and col must be between 0 and 9.");
            }

            var ships = _db.BattleshipShips.Where(x => x.GameId == id).ToList();
            var shots = _db.BattleshipShots.Where(x => x.GameId == id).ToList();

            var computerShips = ships.Where(s => !s.IsPlayerShip).ToList();
            var playerShips = ships.Where(s => s.IsPlayerShip).ToList();

            bool alreadyFired = shots.Any(s => !s.ByComputer && s.Row == requestModel.Row && s.Col == requestModel.Col);
            if (alreadyFired)
            {
                return BadRequest($"You already fired at ({requestModel.Row}, {requestModel.Col}).");
            }

            var (yourResult, yourHitShip) = ApplyShot(computerShips, requestModel.Row, requestModel.Col);

            _db.BattleshipShots.Add(new BattleshipShot
            {
                GameId = id,
                ByComputer = false,
                Row = requestModel.Row,
                Col = requestModel.Col,
                Result = yourResult
            });

            game.ShotsUsed++;

            int enemyShipsLeft = computerShips.Count(s => !s.IsSunk);
            if (enemyShipsLeft == 0)
            {
                game.Status = "Won";
                _db.SaveChanges();
                transaction.Commit();

                return Ok(new
                {
                    you = new { row = requestModel.Row, col = requestModel.Col, result = yourResult, ship = yourHitShip?.Type },
                    computer = (object?)null,
                    yourShipsLeft = playerShips.Count(s => !s.IsSunk),
                    enemyShipsLeft,
                    shotsUsed = game.ShotsUsed,
                    status = game.Status,
                    message = $"You sank the {yourHitShip!.Type}! All enemy ships destroyed. You won in {game.ShotsUsed} shots!"
                });
            }

            var computerShots = shots.Where(s => s.ByComputer).ToList();
            var (cRow, cCol) = ChooseComputerTarget(computerShots, playerShips);

            var (computerResult, computerHitShip) = ApplyShot(playerShips, cRow, cCol);

            _db.BattleshipShots.Add(new BattleshipShot
            {
                GameId = id,
                ByComputer = true,
                Row = cRow,
                Col = cCol,
                Result = computerResult
            });

            int yourShipsLeft = playerShips.Count(s => !s.IsSunk);
            if (yourShipsLeft == 0)
            {
                game.Status = "Lost";
            }

            _db.SaveChanges();
            transaction.Commit();

            string message = Describe("You", yourResult, yourHitShip) + " " +
                             Describe("Computer", computerResult, computerHitShip);
            if (game.Status == "Lost")
            {
                message += " All your ships are gone. You lost!";
            }

            return Ok(new
            {
                you = new { row = requestModel.Row, col = requestModel.Col, result = yourResult, ship = yourHitShip?.Type },
                computer = new { row = cRow, col = cCol, result = computerResult, ship = computerHitShip?.Type },
                yourShipsLeft,
                enemyShipsLeft,
                shotsUsed = game.ShotsUsed,
                status = game.Status,
                message
            });
        }

        /// <summary>See the game status, the enemy board (your shots) and your board (computer's shots).</summary>

        [HttpGet("{id}")]
        public IActionResult GetGame(int id)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT GameId FROM BattleshipGame WITH (UPDLOCK, HOLDLOCK) WHERE GameId = {id}");
            var game = _db.BattleshipGames.FirstOrDefault(x => x.GameId == id);
            if (game is null)
            {
                return NotFound($"Game {id} not found.");
            }

            var ships = _db.BattleshipShips.Where(x => x.GameId == id).ToList();
            var shots = _db.BattleshipShots.Where(x => x.GameId == id).ToList();

            var computerShips = ships.Where(s => !s.IsPlayerShip).ToList();
            var playerShips = ships.Where(s => s.IsPlayerShip).ToList();
            bool finished = game.Status == "Won" || game.Status == "Lost" || game.Status == "GaveUp";

            transaction.Commit();
            return Ok(new
            {
                gameId = game.GameId,
                status = game.Status,
                shotsUsed = game.ShotsUsed,
                yourShipsLeft = playerShips.Count(s => !s.IsSunk),
                enemyShipsLeft = computerShips.Count(s => !s.IsSunk),
                enemyShipsSunk = computerShips.Where(s => s.IsSunk).Select(s => s.Type),
                yourShipsSunk = playerShips.Where(s => s.IsSunk).Select(s => s.Type),
                legend = "X = hit, ~ = miss, O = ship, . = unknown",
                // Enemy ships stay hidden until the game is over
                enemyBoard = DrawBoard(computerShips, shots.Where(s => !s.ByComputer).ToList(), showShips: finished),
                // You can always see your own ships
                yourBoard = DrawBoard(playerShips, shots.Where(s => s.ByComputer).ToList(), showShips: true)
            });
        }

        /// <summary>Start the same game over: clears all shots and ships, and the computer hides new ships.</summary>

        [HttpPut("{id}/restart")]
        public IActionResult Restart(int id)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT GameId FROM BattleshipGame WITH (UPDLOCK, HOLDLOCK) WHERE GameId = {id}");
            var game = _db.BattleshipGames.FirstOrDefault(x => x.GameId == id);
            if (game is null)
            {
                return NotFound($"Game {id} not found.");
            }

            var oldShots = _db.BattleshipShots.Where(x => x.GameId == id).ToList();
            var oldShips = _db.BattleshipShips.Where(x => x.GameId == id).ToList();
            _db.BattleshipShots.RemoveRange(oldShots);
            _db.BattleshipShips.RemoveRange(oldShips);

            game.Status = "Placing";
            game.ShotsUsed = 0;
            _db.BattleshipShips.AddRange(PlaceShipsRandomly(id, isPlayerShip: false));

            _db.SaveChanges();
            transaction.Commit();

            return Ok(new
            {
                gameId = id,
                status = game.Status,
                message = $"Game restarted! The computer hid new ships. Place yours with POST /api/games/{id}/ships " +
                          "(or /ships/random)."
            });
        }

        /// <summary>Give up the game. The computer's ships are revealed.</summary>

        [HttpPatch("{id}/give-up")]
        public IActionResult GiveUp(int id)
        {
            using var transaction = _db.Database.BeginTransaction();
            _db.Database.ExecuteSqlInterpolated($"SELECT GameId FROM BattleshipGame WITH (UPDLOCK, HOLDLOCK) WHERE GameId = {id}");
            var game = _db.BattleshipGames.FirstOrDefault(x => x.GameId == id);
            if (game is null)
            {
                return NotFound($"Game {id} not found.");
            }

            if (game.Status != "Playing" && game.Status != "Placing")
            {
                return BadRequest($"This game is already finished ({game.Status}).");
            }

            game.Status = "GaveUp";
            _db.SaveChanges();

            var computerShips = _db.BattleshipShips.Where(x => x.GameId == id && !x.IsPlayerShip).ToList();
            var yourShots = _db.BattleshipShots.Where(x => x.GameId == id && !x.ByComputer).ToList();

            transaction.Commit();
            return Ok(new
            {
                gameId = id,
                status = game.Status,
                message = "You gave up. Here is where the computer's ships were (O).",
                enemyShips = computerShips.Select(s => new
                {
                    s.Type,
                    s.Size,
                    row = s.StartRow,
                    col = s.StartCol,
                    s.IsHorizontal,
                    s.IsSunk
                }),
                enemyBoard = DrawBoard(computerShips, yourShots, showShips: true)
            });
        }

        private static bool Covers(BattleshipShip ship, int row, int col)
        {
            if (ship.IsHorizontal)
                return row == ship.StartRow && col >= ship.StartCol && col < ship.StartCol + ship.Size;
            return col == ship.StartCol && row >= ship.StartRow && row < ship.StartRow + ship.Size;
        }

        private static (string Result, BattleshipShip? Ship) ApplyShot(List<BattleshipShip> ships, int row, int col)
        {
            var ship = ships.FirstOrDefault(s => Covers(s, row, col));
            if (ship is null)
                return ("miss", null);

            ship.Hits++;
            if (ship.Hits == ship.Size)
            {
                ship.IsSunk = true;
                return ("sunk", ship);
            }
            return ("hit", ship);
        }

        private static (int Row, int Col) ChooseComputerTarget(List<BattleshipShot> computerShots, List<BattleshipShip> playerShips)
        {
            bool Tried(int r, int c) => computerShots.Any(s => s.Row == r && s.Col == c);
            bool OnBoard(int r, int c) => r >= 0 && r < BoardSize && c >= 0 && c < BoardSize;

            var openHits = computerShots
                .Where(s => s.Result == "hit")
                .Where(s => playerShips.Any(p => !p.IsSunk && Covers(p, s.Row, s.Col)))
                .ToList();

            var targets = new List<(int, int)>();
            foreach (var h in openHits)
            {
                foreach (var (dr, dc) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                {
                    int r = h.Row + dr, c = h.Col + dc;
                    if (OnBoard(r, c) && !Tried(r, c))
                        targets.Add((r, c));
                }
            }

            if (targets.Count > 0)
                return targets[Random.Shared.Next(targets.Count)];

            var free = new List<(int, int)>();
            for (int r = 0; r < BoardSize; r++)
                for (int c = 0; c < BoardSize; c++)
                    if (!Tried(r, c))
                        free.Add((r, c));

            return free[Random.Shared.Next(free.Count)];
        }

        private static string Describe(string who, string result, BattleshipShip? ship) => result switch
        {
            "miss" => $"{who} missed.",
            "hit" => $"{who} hit a ship!",
            _ => $"{who} sank the {ship!.Type}!"
        };

        private static List<BattleshipShip> PlaceShipsRandomly(int gameId, bool isPlayerShip)
        {
            var placed = new List<BattleshipShip>();

            foreach (var (type, size) in Fleet)
            {
                while (true)
                {
                    bool horizontal = Random.Shared.Next(2) == 0;
                    int row = Random.Shared.Next(horizontal ? BoardSize : BoardSize - size + 1);
                    int col = Random.Shared.Next(horizontal ? BoardSize - size + 1 : BoardSize);

                    var ship = new BattleshipShip
                    {
                        GameId = gameId,
                        IsPlayerShip = isPlayerShip,
                        Type = type,
                        Size = size,
                        StartRow = row,
                        StartCol = col,
                        IsHorizontal = horizontal,
                        Hits = 0,
                        IsSunk = false
                    };

                    bool overlaps = Enumerable.Range(0, size).Any(i =>
                    {
                        int r = horizontal ? row : row + i;
                        int c = horizontal ? col + i : col;
                        return placed.Any(p => Covers(p, r, c));
                    });

                    if (!overlaps)
                    {
                        placed.Add(ship);
                        break;
                    }
                }
            }

            return placed;
        }

        private static List<string> DrawBoard(List<BattleshipShip> ships, List<BattleshipShot> shots, bool showShips)
        {
            var lines = new List<string> { "   0 1 2 3 4 5 6 7 8 9" };

            for (int r = 0; r < BoardSize; r++)
            {
                var line = $"{r}  ";
                for (int c = 0; c < BoardSize; c++)
                {
                    var shot = shots.FirstOrDefault(s => s.Row == r && s.Col == c);
                    char cell;
                    if (shot is not null)
                        cell = shot.Result == "miss" ? '~' : 'X';
                    else if (showShips && ships.Any(s => Covers(s, r, c)))
                        cell = 'O';
                    else
                        cell = '.';
                    line += cell + " ";
                }
                lines.Add(line.TrimEnd());
            }

            return lines;
        }
    }
}
