using System.ComponentModel.DataAnnotations;
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
    Assert(new GamesController().UpdateGame(game.Id, new() { AvailableStock = 1 }) is BadRequestObjectResult,
        "Reserved stock cannot be advertised as available");
    Assert(new GamesController().UpdateGame(game.Id, new() { TotalStock = 0 }) is BadRequestObjectResult,
        "Total stock must cover active sessions");
    var freeGame = new Game { Title = "Free", TotalStock = 2, AvailableStock = 2 };
    var gameOnlyCategory = new ConsoleCategory { Name = "GameOnly", HourlyRate = 10 };
    var categoryGame = new Game { Title = "Categorized", Category = gameOnlyCategory, TotalStock = 1, AvailableStock = 1 };
    var numberedCategory = new ConsoleCategory { Name = "Sequence", HourlyRate = 10 };
    var deletedCategory = new ConsoleCategory { Name = "Deleted", HourlyRate = 10, IsDeleted = true };
    var longCategory = new ConsoleCategory { Name = new string('L', 40), HourlyRate = 10 };
    db.AddRange(freeGame, categoryGame, numberedCategory, deletedCategory, longCategory);
    db.SaveChanges();
    Assert(new GamesController().UpdateGame(freeGame.Id, new() { AvailableStock = 3 }) is BadRequestObjectResult,
        "Available-only stock patch cannot exceed existing total");
    Assert(new GamesController().UpdateGame(freeGame.Id, new() { TotalStock = 1 }) is BadRequestObjectResult,
        "Total-only stock patch cannot undercut existing available stock");
    Ok(new GamesController().UpdateGame(freeGame.Id, new() { TotalStock = 1, AvailableStock = 1 }));

    Assert(new ConsoleCategoriesController().DeleteCategory(gameOnlyCategory.Id) is BadRequestObjectResult,
        "Category deletion must query unloaded game children");
    Ok(new GamesController().DeleteGame(categoryGame.Id));
    Ok(new ConsoleCategoriesController().DeleteCategory(gameOnlyCategory.Id));
    Assert(new ConsoleCategoriesController().DeleteCategory(gameOnlyCategory.Id, true) is BadRequestObjectResult,
        "Hard category deletion must retain soft-deleted game FK protection");
    Ok(new GamesController().DeleteGame(categoryGame.Id, true));
    Ok(new ConsoleCategoriesController().DeleteCategory(gameOnlyCategory.Id, true));

    var firstNumbered = (GameConsoleModel.Response)Ok(new GameConsolesController().CreateGameConsole(new() { CategoryId = numberedCategory.Id }));
    var secondNumbered = (GameConsoleModel.Response)Ok(new GameConsolesController().CreateGameConsole(new() { CategoryId = numberedCategory.Id }));
    Ok(new GameConsolesController().DeleteGameConsole(firstNumbered.Id, true));
    var reusedNumber = (GameConsoleModel.Response)Ok(new GameConsolesController().CreateGameConsole(new() { CategoryId = numberedCategory.Id }));
    Assert(reusedNumber.ConsoleNumber != secondNumbered.ConsoleNumber, "Deleting a console must not cause number collisions");
    var concurrentConsoles = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        (GameConsoleModel.Response)Ok(new GameConsolesController().CreateGameConsole(new() { CategoryId = numberedCategory.Id })))));
    Assert(concurrentConsoles.Select(c => c.ConsoleNumber).Distinct().Count() == 2, "Concurrent console numbers must be unique");
    Assert(new GameConsolesController().UpdateGameConsole(reusedNumber.Id, new() { ConsoleNumber = secondNumbered.ConsoleNumber }) is ConflictObjectResult,
        "Duplicate explicit console rename must return conflict");
    new GameConsolesController().UpdateGameConsole(reusedNumber.Id, new() { CategoryId = numberedCategory.Id });
    Assert(db.GameConsoles.AsNoTracking().Single(c => c.Id == reusedNumber.Id).ConsoleNumber == reusedNumber.ConsoleNumber,
        "Same-category update must not rename the console");
    var longNumber = (GameConsoleModel.Response)Ok(new GameConsolesController().CreateGameConsole(new() { CategoryId = longCategory.Id }));
    Assert(longNumber.ConsoleNumber!.Length <= 20, "Generated number must fit its database column");
    Assert(new GameConsolesController().CreateGameConsole(new() { CategoryId = deletedCategory.Id }) is BadRequestObjectResult,
        "Deleted categories cannot receive consoles");
    Assert(new GamesController().CreateGame(new() { CategoryId = deletedCategory.Id, Title = "Deleted category", TotalStock = 1 }) is BadRequestObjectResult,
        "Deleted categories cannot receive games");
    Assert(new GamesController().UpdateGame(freeGame.Id, new() { CategoryId = deletedCategory.Id }) is BadRequestObjectResult,
        "Games cannot be moved into deleted categories");
    Assert(new GameConsolesController().UpdateGameConsole(reusedNumber.Id, new() { CategoryId = deletedCategory.Id }) is BadRequestObjectResult,
        "Consoles cannot be moved into deleted categories");

    var cleanupConsole = new GameConsole { Category = category, ConsoleNumber = "Cleanup", CurrentStatus = "Available" };
    var cleanupGame = new Game { Title = "Cleanup", TotalStock = 1, AvailableStock = 1 };
    db.AddRange(cleanupConsole, cleanupGame);
    db.SaveChanges();
    var cleanupSession = (GameSessionModel.Response)Ok(new GameSessionsController().CreateGameSession(new()
    {
        ConsoleId = cleanupConsole.Id, GameId = cleanupGame.Id, StartTime = start.AddHours(4), EndTime = start.AddHours(5)
    }));
    foreach (DateTime invalidEnd in new[] { start.AddHours(4), start.AddHours(4).AddMinutes(-1) })
        Assert(new GameSessionsController().UpdateGameSession(cleanupSession.Id, new() { EndTime = invalidEnd }) is BadRequestObjectResult,
            "Session updates must reject zero or negative intervals");
    Ok(new GameSessionsController().DeleteGameSession(cleanupSession.Id));
    Assert(new GamesController().DeleteGame(cleanupGame.Id, true) is BadRequestObjectResult,
        "Hard game deletion must retain soft-deleted session FK protection");
    Assert(new GameConsolesController().DeleteGameConsole(cleanupConsole.Id, true) is BadRequestObjectResult,
        "Hard console deletion must retain soft-deleted session FK protection");
    Ok(new GamesController().DeleteGame(cleanupGame.Id));
    Ok(new GameConsolesController().DeleteGameConsole(cleanupConsole.Id));
    Ok(new GameSessionsController().DeleteGameSession(cleanupSession.Id, true));
    Ok(new GamesController().DeleteGame(cleanupGame.Id, true));
    Ok(new GameConsolesController().DeleteGameConsole(cleanupConsole.Id, true));

    var consoleOnlyCategory = new ConsoleCategory { Name = "ConsoleOnly", HourlyRate = 10 };
    db.ConsoleCategories.Add(consoleOnlyCategory);
    db.SaveChanges();
    var softConsole = (GameConsoleModel.Response)Ok(new GameConsolesController().CreateGameConsole(new() { CategoryId = consoleOnlyCategory.Id }));
    Ok(new GameConsolesController().DeleteGameConsole(softConsole.Id));
    Ok(new ConsoleCategoriesController().DeleteCategory(consoleOnlyCategory.Id));
    Assert(new ConsoleCategoriesController().DeleteCategory(consoleOnlyCategory.Id, true) is BadRequestObjectResult,
        "Hard category deletion must retain soft-deleted console FK protection");
    Ok(new GameConsolesController().DeleteGameConsole(softConsole.Id, true));
    Ok(new ConsoleCategoriesController().DeleteCategory(consoleOnlyCategory.Id, true));

    var renamed = await Task.WhenAll(new[] { secondNumbered.Id, concurrentConsoles[0].Id }.Select(id => Task.Run(() =>
        new GameConsolesController().UpdateGameConsole(id, new() { ConsoleNumber = "Same-Race-Number" }))));
    Assert(renamed.Count(r => r is OkObjectResult) == 1 && renamed.Count(r => r is ConflictObjectResult) == 1,
        "Concurrent console renames must return one success and one conflict");

    var futureStart = start.AddHours(20);
    foreach (DateTime invalidEnd in new[] { futureStart, futureStart.AddMinutes(-1) })
    {
        Assert(new BookingController().CreateBooking(new() { ConsoleId = reusedNumber.Id, CustomerPhone = "123", StartTime = futureStart, EndTime = invalidEnd }) is BadRequestObjectResult,
            "Bookings must reject zero or negative intervals");
        Assert(new GameSessionsController().CreateGameSession(new() { ConsoleId = reusedNumber.Id, StartTime = futureStart, EndTime = invalidEnd }) is BadRequestObjectResult,
            "Sessions must reject zero or negative intervals");
    }
    Assert(new BookingController().CreateBooking(new() { ConsoleId = reusedNumber.Id, CustomerPhone = "123", StartTime = futureStart, EndTime = futureStart.AddHours(1), DepositAmount = -1 }) is BadRequestObjectResult,
        "Negative deposits must be rejected");
    Assert(new BookingController().CreateBooking(new() { ConsoleId = reusedNumber.Id, CustomerPhone = "123", StartTime = futureStart, EndTime = futureStart.AddHours(1), DepositAmount = 11 }) is BadRequestObjectResult,
        "Deposits above total must be rejected");
    var deposited = (BookingModel.Response)Ok(new BookingController().CreateBooking(new()
    {
        ConsoleId = reusedNumber.Id, CustomerPhone = "123", StartTime = futureStart, EndTime = futureStart.AddHours(1), DepositAmount = 5
    }));
    foreach (DateTime invalidEnd in new[] { futureStart, futureStart.AddMinutes(-1) })
        Assert(new BookingController().UpdateBooking(deposited.Id, new() { EndTime = invalidEnd }) is BadRequestObjectResult,
            "Booking updates must reject zero or negative intervals");
    Assert(new BookingController().UpdateBooking(deposited.Id, new() { DepositAmount = -1 }) is BadRequestObjectResult,
        "Booking updates must reject negative deposits");
    Assert(new BookingController().UpdateBooking(deposited.Id, new() { EndTime = futureStart.AddMinutes(15) }) is BadRequestObjectResult,
        "Shortening a booking cannot leave deposit above the new total");
    var extended = (BookingModel.Response)Ok(new BookingController().UpdateBooking(deposited.Id, new() { EndTime = futureStart.AddHours(2), DepositAmount = 15 }));
    Assert(extended.DepositAmount == 15 && extended.TotalAmount == 20, "Deposit validation must use the effective new total");
    Ok(new BookingController().DeleteBooking(deposited.Id));
    Assert(new GameConsolesController().DeleteGameConsole(reusedNumber.Id, true) is BadRequestObjectResult,
        "Hard console deletion must retain soft-deleted booking FK protection");
    Ok(new BookingController().DeleteBooking(deposited.Id, true));
    Ok(new GameConsolesController().DeleteGameConsole(reusedNumber.Id, true));
    var durationConsole = new GameConsole { Category = category, ConsoleNumber = "Duration", CurrentStatus = "Available" };
    db.GameConsoles.Add(durationConsole);
    db.SaveChanges();
    DateTime durationStart = start.AddHours(300);
    foreach (double hours in new[] { 100d, 99.995d })
    {
        Assert(new BookingController().CreateBooking(new() { ConsoleId = durationConsole.Id, CustomerPhone = "123", StartTime = durationStart, EndTime = durationStart.AddHours(hours) }) is BadRequestObjectResult,
            "Booking duration must fit rounded decimal(4,2)");
        Assert(new GameSessionsController().CreateGameSession(new() { ConsoleId = durationConsole.Id, StartTime = durationStart, EndTime = durationStart.AddHours(hours) }) is BadRequestObjectResult,
            "Session duration must fit rounded decimal(4,2)");
    }
    var durationBooking = (BookingModel.Response)Ok(new BookingController().CreateBooking(new() { ConsoleId = durationConsole.Id, CustomerPhone = "123", StartTime = durationStart, EndTime = durationStart.AddHours(1) }));
    Assert(new BookingController().UpdateBooking(durationBooking.Id, new() { EndTime = durationStart.AddHours(100) }) is BadRequestObjectResult,
        "Booking duration update must reject SQL overflow");
    Ok(new BookingController().UpdateBooking(durationBooking.Id, new() { EndTime = durationStart.AddHours(99.994) }));
    Assert(db.Bookings.AsNoTracking().Single(b => b.Id == durationBooking.Id).TotalHours == 99.99m,
        "Valid boundary booking duration must round to stored 99.99");
    Ok(new BookingController().DeleteBooking(durationBooking.Id, true));
    var durationSession = (GameSessionModel.Response)Ok(new GameSessionsController().CreateGameSession(new() { ConsoleId = durationConsole.Id, StartTime = durationStart, EndTime = durationStart.AddHours(1) }));
    Assert(new GameSessionsController().UpdateGameSession(durationSession.Id, new() { EndTime = durationStart.AddHours(100) }) is BadRequestObjectResult,
        "Session duration update must reject SQL overflow");
    Ok(new GameSessionsController().UpdateGameSession(durationSession.Id, new() { EndTime = durationStart.AddHours(99.994) }));
    Assert(db.GameSessions.AsNoTracking().Single(b => b.Id == durationSession.Id).TotalHours == 99.99m,
        "Valid boundary session duration must round to stored 99.99");
    Ok(new GameSessionsController().DeleteGameSession(durationSession.Id, true));

    var earliestConsole = new GameConsole { Category = category, ConsoleNumber = "Earliest", CurrentStatus = "Available" };
    db.GameConsoles.Add(earliestConsole);
    db.SaveChanges();
    var earliestBooking = (BookingModel.Response)Ok(new BookingController().CreateBooking(new()
    {
        ConsoleId = earliestConsole.Id, CustomerPhone = "123", StartTime = DateTime.MinValue, EndTime = DateTime.MinValue.AddHours(1)
    }));
    var earliestSession = (GameSessionModel.Response)Ok(new GameSessionsController().CreateGameSession(new()
    {
        ConsoleId = earliestConsole.Id, BookingId = earliestBooking.Id, StartTime = DateTime.MinValue, EndTime = DateTime.MinValue.AddHours(1), PaidAmount = 10
    }));
    Ok(new GameSessionsController().DeleteGameSession(earliestSession.Id, true));
    Ok(new BookingController().DeleteBooking(earliestBooking.Id, true));

    var expensive = new ConsoleCategory { Name = "Expensive", HourlyRate = 9999999999999999.99m };
    var expensiveConsole = new GameConsole { Category = expensive, ConsoleNumber = "Expensive-1", CurrentStatus = "Available" };
    db.GameConsoles.Add(expensiveConsole);
    db.SaveChanges();
    Assert(new BookingController().CreateBooking(new() { ConsoleId = expensiveConsole.Id, CustomerPhone = "123", StartTime = durationStart, EndTime = durationStart.AddHours(2) }) is BadRequestObjectResult,
        "Booking total must fit decimal(18,2)");
    Assert(new GameSessionsController().CreateGameSession(new() { ConsoleId = expensiveConsole.Id, StartTime = durationStart, EndTime = durationStart.AddHours(2) }) is BadRequestObjectResult,
        "Session total must fit decimal(18,2)");
    var expensiveBooking = (BookingModel.Response)Ok(new BookingController().CreateBooking(new() { ConsoleId = expensiveConsole.Id, CustomerPhone = "123", StartTime = durationStart, EndTime = durationStart.AddHours(1) }));
    Assert(new BookingController().UpdateBooking(expensiveBooking.Id, new() { EndTime = durationStart.AddHours(2) }) is BadRequestObjectResult,
        "Booking total update must reject SQL overflow");
    Ok(new BookingController().DeleteBooking(expensiveBooking.Id, true));
    var expensiveSession = (GameSessionModel.Response)Ok(new GameSessionsController().CreateGameSession(new() { ConsoleId = expensiveConsole.Id, StartTime = durationStart, EndTime = durationStart.AddHours(1) }));
    Assert(new GameSessionsController().UpdateGameSession(expensiveSession.Id, new() { EndTime = durationStart.AddHours(2) }) is BadRequestObjectResult,
        "Session total update must reject SQL overflow");
    Ok(new GameSessionsController().DeleteGameSession(expensiveSession.Id, true));

    Assert(new ConsoleCategoriesController().CreateConsoleCategory(new() { Name = " ", HourlyPrice = 5000 }) is BadRequestObjectResult,
        "Whitespace category names must be rejected");
    Assert(new GamesController().CreateGame(new() { Title = " ", TotalStock = 1 }) is BadRequestObjectResult,
        "Whitespace game titles must be rejected");
    var priced = (ConsoleCategoryModel.Response)Ok(new ConsoleCategoriesController().CreateConsoleCategory(new() { Name = "Price", HourlyPrice = 6000 }));
    var minimum = (ConsoleCategoryModel.Response)Ok(new ConsoleCategoriesController().UpdateCategory(priced.Id, new() { HourlyPrice = 5000 }));
    Assert(minimum.HourlyPrice == 5000, "Minimum hourly price must work on update as on create");
    var centConsole = new GameConsole { CategoryId = priced.Id, ConsoleNumber = "Cents", CurrentStatus = "Available" };
    db.GameConsoles.Add(centConsole);
    db.SaveChanges();
    DateTime centStart = start.AddHours(600);
    var roundedCreate = (BookingModel.Response)Ok(new BookingController().CreateBooking(new()
    {
        ConsoleId = centConsole.Id, CustomerPhone = "123", StartTime = centStart, EndTime = centStart.AddHours(1).AddMilliseconds(-1), DepositAmount = 5000
    }));
    Assert(roundedCreate.TotalAmount == 5000, "Deposit validation must compare the SQL-rounded bill on create");
    Ok(new BookingController().DeleteBooking(roundedCreate.Id, true));
    var fractionalDeposit = (BookingModel.Response)Ok(new BookingController().CreateBooking(new()
    {
        ConsoleId = centConsole.Id, CustomerPhone = "123", StartTime = centStart, EndTime = centStart.AddHours(1).AddMilliseconds(-1), DepositAmount = 5000.004m
    }));
    Assert(fractionalDeposit.DepositAmount == 5000 && fractionalDeposit.TotalAmount == 5000, "Fractional deposit must compare and return stored cents");
    Ok(new BookingController().DeleteBooking(fractionalDeposit.Id, true));
    var roundedUpdate = (BookingModel.Response)Ok(new BookingController().CreateBooking(new()
    {
        ConsoleId = centConsole.Id, CustomerPhone = "123", StartTime = centStart, EndTime = centStart.AddHours(1), DepositAmount = 5000
    }));
    var fractionalUpdate = (BookingModel.Response)Ok(new BookingController().UpdateBooking(roundedUpdate.Id, new() { DepositAmount = 5000.004m }));
    Assert(fractionalUpdate.DepositAmount == 5000, "Fractional deposit updates must use stored cents");
    var shortened = (BookingModel.Response)Ok(new BookingController().UpdateBooking(roundedUpdate.Id, new()
    {
        EndTime = centStart.AddHours(1).AddMilliseconds(-1)
    }));
    Assert(shortened.TotalAmount == 5000 && shortened.DepositAmount == 5000, "One-millisecond shortening must compare the SQL-rounded bill");
    Ok(new BookingController().DeleteBooking(roundedUpdate.Id, true));
    var centSession = (GameSessionModel.Response)Ok(new GameSessionsController().CreateGameSession(new()
    {
        ConsoleId = centConsole.Id, StartTime = centStart, EndTime = centStart.AddHours(1).AddMilliseconds(-1), PaidAmount = 5000.004m
    }));
    Assert(centSession.PaidAmount == 5000, "Session create payment must use stored cents");
    Assert(centSession.TotalAmount == 5000, "Session create bill must round to stored cents");
    var centSessionUpdate = (GameSessionModel.Response)Ok(new GameSessionsController().UpdateGameSession(centSession.Id, new()
    {
        EndTime = centStart.AddHours(1).AddMilliseconds(-2), PaidAmount = 5000.004m
    }));
    Assert(centSessionUpdate.PaidAmount == 5000, "Session payment update must use stored cents");
    Assert(centSessionUpdate.TotalAmount == 5000, "Session update bill must round to stored cents");
    Ok(new GameSessionsController().DeleteGameSession(centSession.Id, true));
    var paidBooking = (BookingModel.Response)Ok(new BookingController().CreateBooking(new()
    {
        ConsoleId = centConsole.Id, CustomerPhone = "123", StartTime = centStart, EndTime = centStart.AddHours(1), DepositAmount = 4999.995m
    }));
    var paidSession = (GameSessionModel.Response)Ok(new GameSessionsController().CreateGameSession(new()
    {
        ConsoleId = centConsole.Id, BookingId = paidBooking.Id, StartTime = centStart, EndTime = centStart.AddHours(1), PaidAmount = 0.004m
    }));
    Assert(paidSession.PaidAmount == 5000, "Booked payments must compare the sum of stored deposit/payment cents");
    Ok(new GameSessionsController().DeleteGameSession(paidSession.Id, true));
    Ok(new BookingController().DeleteBooking(paidBooking.Id, true));
    Assert(new ConsoleCategoriesController().UpdateCategory(priced.Id, new() { HourlyPrice = 4999 }) is BadRequestObjectResult,
        "Below-minimum hourly price must be rejected");
    Assert(new ConsoleCategoriesController().UpdateCategory(priced.Id, new() { Name = " " }) is BadRequestObjectResult,
        "Whitespace category updates must be rejected");

    object[] invalidRequests =
    [
        new GamesModel.CreateRequest { Title = new string('x', 151), TotalStock = 1 },
        new GamesModel.CreateRequest { Title = " ", TotalStock = 1 },
        new GamesModel.UpdateRequest { TotalStock = -1 },
        new ConsoleCategoryModel.CreateRequest { Name = new string('x', 51), HourlyPrice = 5000 },
        new ConsoleCategoryModel.UpdateRequest { HourlyPrice = decimal.MaxValue },
        new BookingModel.CreateRequest { ConsoleId = 1, CustomerPhone = new string('1', 21), StartTime = start, EndTime = start.AddHours(1) },
        new BookingModel.UpdateRequest { DepositAmount = -1 },
        new BookingModel.UpdateRequest { BookingStatus = new string('x', 21) },
        new GameConsoleModel.UpdateRequest { ConsoleNumber = new string('x', 21) },
        new GameConsoleModel.UpdateRequest { CurrentStatus = new string('x', 21) },
        new GameConsoleModel.UpdateRequest { CategoryId = 0 },
        new GameSessionModel.CreateRequest { ConsoleId = 1, GameId = 0, StartTime = start, EndTime = start.AddHours(1) },
        new GameSessionModel.UpdateRequest { PaidAmount = decimal.MaxValue }
    ];
    Assert(invalidRequests.All(request => !Valid(request)), "Schema-bound request validation missed invalid input");
    Assert(Valid(new GamesModel.UpdateRequest()) && Valid(new GameConsoleModel.UpdateRequest { CurrentStatus = "Available" }) &&
        Valid(new ConsoleCategoryModel.UpdateRequest { HourlyPrice = 5000 }), "Optional fields or valid minimum values were rejected");
    Console.WriteLine("Game Shop SQL regression checks passed: stock bounds, deletion FKs, numbering/concurrency, categories, deposits, duration/money bounds and DTO validation.");
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

static bool Valid(object request) => Validator.TryValidateObject(request, new ValidationContext(request), [], true);
