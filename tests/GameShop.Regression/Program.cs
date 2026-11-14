using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sep2026.GameShopWebApi.Controllers;
using Sep2026.GameShopWebApi.Database.GameShopAppDbContext;
using Sep2026.GameShopWebApi.Models;

// Run against local SQL Server with integrated authentication; creates only a uniquely named test database.
string database = "GameShopRegression_" + Guid.NewGuid().ToString("N");
Environment.SetEnvironmentVariable("ConnectionStrings__GameShopDb", $"Server=.;Database={database};Integrated Security=True;TrustServerCertificate=True;");
using var db = new GameShopAppDbContext();
try
{
    db.Database.EnsureCreated();
    var category = new ConsoleCategory { Name = "Test", HourlyRate = 10 };
    var console = new GameConsole { Category = category, ConsoleNumber = "A", CurrentStatus = "Available" };
    var secondConsole = new GameConsole { Category = category, ConsoleNumber = "B", CurrentStatus = "Available" };
    var game = new Game { Title = "First", TotalStock = 1, AvailableStock = 1 };
    var otherGame = new Game { Title = "Second", TotalStock = 1, AvailableStock = 1 };
    db.AddRange(category, console, secondConsole, game, otherGame);
    db.SaveChanges();
    DateTime start = DateTime.Today.AddDays(1).AddHours(10);
    var booking = (BookingModel.Response)Ok(new BookingController().CreateBooking(new()
    {
        ConsoleId = console.Id, CustomerPhone = "123", StartTime = start.AddHours(-1), EndTime = start
    }));
    var request = new GameSessionModel.CreateRequest
    {
        ConsoleId = console.Id, StartTime = start, EndTime = start.AddHours(1), GameId = int.MaxValue
    };
    Assert(new GameSessionsController().CreateGameSession(request) is NotFoundObjectResult, "Unknown positive game ID must be rejected");
    request.GameId = game.Id;
    var session = (GameSessionModel.Response)Ok(new GameSessionsController().CreateGameSession(request));
    db.Entry(game).Reload();
    Assert(game.AvailableStock == 0, "Creation must reserve stock");
    Assert(new GameSessionsController().CreateGameSession(new()
    {
        ConsoleId = secondConsole.Id, GameId = game.Id, StartTime = start, EndTime = start.AddHours(1)
    }) is BadRequestObjectResult, "Out-of-stock games must be rejected");
    Assert(new BookingController().UpdateBooking(booking.Id, new() { StartTime = start.AddMinutes(30), EndTime = start.AddMinutes(90) }) is ConflictObjectResult,
        "Booking changes must reject another session");
    var overlaps = (BookingModel.OverlapResponse)Ok(new BookingController().CheckBookings(start.AddMinutes(30), start.AddMinutes(90)));
    Assert(overlaps.OverlapSessions.Any(s => s.Id == session.Id), "Partial overlaps must be returned");
    Ok(new GameSessionsController().UpdateGameSession(session.Id, new() { GameId = otherGame.Id }));
    db.Entry(game).Reload(); db.Entry(otherGame).Reload();
    Assert(game.AvailableStock == 1 && otherGame.AvailableStock == 0, "Swap must return old stock and reserve new stock");
    Ok(new GameSessionsController().DeleteGameSession(session.Id));
    Ok(new GameSessionsController().DeleteGameSession(session.Id, true));
    db.Entry(otherGame).Reload();
    Assert(otherGame.AvailableStock == 1, "Soft then hard delete must restore stock only once");

    // A completed booking and its session can be deleted in dependency order.
    var linkedRequest = new GameSessionModel.CreateRequest
    {
        ConsoleId = console.Id, BookingId = booking.Id, StartTime = start.AddHours(-1), EndTime = start, PaidAmount = 10
    };
    var linked = (GameSessionModel.Response)Ok(new GameSessionsController().CreateGameSession(linkedRequest));
    Ok(new GameSessionsController().DeleteGameSession(linked.Id));
    Assert(new BookingController().DeleteBooking(booking.Id, true) is BadRequestObjectResult, "Hard delete must retain FK protection for soft-deleted sessions");
    Ok(new BookingController().DeleteBooking(booking.Id));
    Ok(new GameSessionsController().DeleteGameSession(linked.Id, true));
    Ok(new BookingController().DeleteBooking(booking.Id, true));

    var results = await Task.WhenAll(new[] { console.Id, secondConsole.Id }.Select(id => Task.Run(() =>
        new GameSessionsController().CreateGameSession(new()
        {
            ConsoleId = id, GameId = game.Id, StartTime = start.AddHours(2), EndTime = start.AddHours(3)
        }))));
    Assert(results.Count(r => r is OkObjectResult) == 1, "Concurrent sessions must not over-reserve one game copy");
    db.Entry(game).Reload();
    Assert(game.AvailableStock == 0, "Concurrent stock must remain zero");
    Console.WriteLine("Game Shop SQL regression checks passed.");
}
finally
{
    db.Database.EnsureDeleted();
}

static object Ok(IActionResult result) => result is OkObjectResult ok ? ok.Value! : throw new Exception($"Expected 200, got {result.GetType().Name}");
static void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
