using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Sep2026.BattleShipWebApi.Controllers;
using Sep2026.BattleShipWebApi.Models;
using BattleContext = Sep2026.NanHsuThetPaing.Battleship.Database.AppDbContextModels.AppDbContext;

static class BattleshipSmoke
{
    public static async Task Run()
    {
        using var db = new BattleContext();
        db.Database.GetService<IRelationalDatabaseCreator>().CreateTables();
        Assert(new GamesController().CreateGame() is CreatedAtActionResult, "Game creation failed.");
        int id = db.BattleshipGames.Single().GameId;
        Assert(new GamesController().PlaceShipsRandom(id) is OkObjectResult, "Random fleet placement failed.");
        var shots = await Race(Enumerable.Range(0, 8).Select(_ => (Func<IActionResult>)(() => new GamesController().Fire(id, new() { Row = 0, Col = 0 }))).ToArray());
        Assert(shots.Count(r => r is OkObjectResult) == 1 && shots.Count(r => r is BadRequestObjectResult) == 7,
            "Concurrent identical shots must accept exactly one turn.");
        Assert(db.BattleshipGames.AsNoTracking().Single().ShotsUsed == 1 && db.BattleshipShots.Count(x => !x.ByComputer) == 1 &&
            db.BattleshipShots.Count(x => x.ByComputer) == 1, "Concurrent shots lost counters or created duplicate turns.");

        for (int i = 0; i < 8; i++)
        {
            Assert(new GamesController().Restart(id) is OkObjectResult, "Restart failed.");
            Assert(new GamesController().PlaceShipsRandom(id) is OkObjectResult, "Placement failed.");
            var result = await Race(() => new GamesController().Fire(id, new() { Row = 0, Col = 0 }), () => new GamesController().Restart(id));
            Assert(result[0] is OkObjectResult or BadRequestObjectResult && result[1] is OkObjectResult, "Fire/restart returned an unexpected result.");
            var restarted = db.BattleshipGames.AsNoTracking().Single();
            Assert(restarted.Status == "Placing" && restarted.ShotsUsed == 0 && !db.BattleshipShots.Any() &&
                db.BattleshipShips.Count(x => !x.IsPlayerShip) == 5 && !db.BattleshipShips.Any(x => x.IsPlayerShip),
                "Fire/restart race left a partial or stale board.");
        }

        var manual = new PlaceShipsRequest
        {
            Ships = new[] { "Carrier", "Battleship", "Cruiser", "Submarine", "Destroyer" }
                .Select((type, row) => new ShipPlacement { Type = type, Row = row, Col = 0, IsHorizontal = true }).ToList()
        };
        var placements = await Race(() => new GamesController().PlaceShips(id, manual), () => new GamesController().PlaceShipsRandom(id));
        Assert(placements.Count(r => r is OkObjectResult) == 1 && placements.Count(r => r is BadRequestObjectResult) == 1 &&
            db.BattleshipShips.Count(x => x.IsPlayerShip) == 5, "Concurrent placements created more than one player fleet.");
        var distinct = await Race(() => new GamesController().Fire(id, new() { Row = 0, Col = 0 }), () => new GamesController().Fire(id, new() { Row = 0, Col = 1 }));
        Assert(distinct.All(r => r is OkObjectResult) && db.BattleshipGames.AsNoTracking().Single().ShotsUsed == 2,
            "Distinct concurrent turns lost a shot counter.");
        var giveUp = await Race(() => new GamesController().Fire(id, new() { Row = 0, Col = 2 }), () => new GamesController().GiveUp(id), () => new GamesController().GetGame(id));
        Assert(giveUp[0] is OkObjectResult or BadRequestObjectResult && giveUp[1] is OkObjectResult && giveUp[2] is OkObjectResult,
            "Fire/give-up/read race failed.");
        Assert(db.BattleshipGames.AsNoTracking().Single().Status == "GaveUp", "A turn overwrote game completion.");

        // A failed fleet insert must not publish a game with no computer ships.
        db.Database.ExecuteSqlRaw("CREATE TRIGGER RejectBattleFleet ON BattleshipShip AFTER INSERT AS THROW 50001, 'Fixture insert failure', 1;");
        try
        {
            try { new GamesController().CreateGame(); throw new Exception("Expected fleet insert failure."); }
            catch (DbUpdateException) { }
        }
        finally
        {
            db.Database.ExecuteSqlRaw("DROP TRIGGER RejectBattleFleet");
        }
        Assert(db.BattleshipGames.Count() == 1, "Failed creation left an orphan game.");
        Console.WriteLine("PASS: Battleship concurrent shots, placements, restart/give-up/read, and atomic game creation.");
    }

    private static async Task<IActionResult[]> Race(params Func<IActionResult>[] actions)
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tasks = actions.Select(action => Task.Run(async () => { await gate.Task; return action(); })).ToArray();
        gate.SetResult();
        return await Task.WhenAll(tasks);
    }

    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
}
