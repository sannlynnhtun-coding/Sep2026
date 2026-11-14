using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sep2026.GameShopWebApi.Models;
using Sep2026.GameShopWebApi.Utils;
using Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameSessionsController : ControllerBase
    {
        private GameShopAppDbContext _db;
        private ModelConverter _modelConverter;
        public GameSessionsController()
        {
            _db = new GameShopAppDbContext();
            _modelConverter = new ModelConverter();
        }
        [HttpGet]
        public IActionResult GetGameSessions()
        {
            var gameSessions = _db.GameSessions.ToList();
            var gameSessionsResponses = gameSessions.Select(_modelConverter.ConvertGameSessionResponse).ToList();
            return Ok(gameSessionsResponses);
        }

        [HttpGet("{id}")]
        public IActionResult GetGameSessionById(int id)
        {
            var gameSession = _db.GameSessions.FirstOrDefault(x => x.Id == id);
            if (gameSession == null)
                return NotFound($"Game session with Id {id} is not found.");

            return Ok(_modelConverter.ConvertGameSessionResponse(gameSession));
        }

        [HttpPost]
        public IActionResult CreateGameSession(GameSessionModel.CreateRequest createRequest)
        {
            using var transaction = _db.BeginSchedulingTransaction();

            if (createRequest == null)
                return BadRequest("Body must be defined");

            if (!createRequest.ConsoleId.HasValue)
                return BadRequest("Must define a seat or game console for this session.");

            //check console
            var reqConsole = _db.GameConsoles
                .Include(g => g.Category)
                .FirstOrDefault(g =>
                    g.Id == createRequest.ConsoleId.Value &&
                    g.CurrentStatus.ToLower() == "available" &&
                    g.IsDeleted == false
                 );

            if (reqConsole == null)
                return BadRequest($"Console with Id {createRequest.ConsoleId.Value} is currently unavailabe.");

            //check time interval
            if (!createRequest.StartTime.HasValue || !createRequest.EndTime.HasValue)
                return BadRequest("Time interval must be defined.");

            if (createRequest.StartTime.Value >= createRequest.EndTime.Value)
                return BadRequest("EndTime should not come first.");


            //check game ( allowed null )
            // note : this field can be null.later owner can add this.
            Game? reqGame = null;
            if (createRequest.GameId.HasValue)
            {
                reqGame = _db.Games.FirstOrDefault(game => game.Id == createRequest.GameId && game.IsDeleted == false);
                if (reqGame == null)
                    return NotFound("Game is not found.");
                if (reqGame.AvailableStock <= 0)
                    return BadRequest("Game is currently out of stock.");

            }

            if (createRequest.BookingId.HasValue && createRequest.BookingId.Value <= 0)
                return BadRequest("Invalid booking ID.");
            if (createRequest.PaidAmount < 0)
                return BadRequest("Paid amount cannot be negative.");

            //check booking if user provide bookingId value ( allowed null )
            //note : owner doesn't care about late, the endTime will be the same as booked endTme.
            bool isBooking = false;
            decimal totalAmount = 0;
            if (createRequest.BookingId.HasValue && createRequest.BookingId.Value > 0)
            {

                //check booking
                var reqBooking = _db.Bookings
                    .FirstOrDefault(booking =>
                        booking.Id == createRequest.BookingId &&
                        booking.BookingStatus.ToLower() != "cancelled" &&
                        booking.BookingStatus.ToLower() != "completed" &&
                        booking.IsDeleted == false
                        );
                if (reqBooking == null)
                    return NotFound($"Booking with Id {createRequest.BookingId} doesn't cancalled or completed");

                if (createRequest.ConsoleId != reqBooking.ConsoleId)
                    return BadRequest($"Booking with Id {createRequest.BookingId} didn't register game console ( {reqConsole.ConsoleNumber} )");

                //check if costumer arrived earlier than book starttime.Owner will ask them to wait.
                if (createRequest.StartTime.Value < reqBooking.StartTime.AddMinutes(-5))
                    return BadRequest($"Current session startTime is earlier than booked startTime.");

                //check if costumer arrived after the endTime.
                if (createRequest.StartTime.Value >= reqBooking.EndTime)
                    return BadRequest($"This session is  already expire");

                //check totalAmount vs depositAmount
                var totalPaidAmount = createRequest.PaidAmount + reqBooking.DepositAmount;
                if (totalPaidAmount < reqBooking.TotalAmount)
                    return BadRequest($"Insufficent amount.Required amount is {reqBooking.TotalAmount - totalPaidAmount}mmk.");

                if (totalPaidAmount > reqBooking.TotalAmount)
                    return BadRequest($"Exceeded amount {totalPaidAmount - reqBooking.TotalAmount}");


                isBooking = true;
                totalAmount = reqBooking.TotalAmount;
                //update booking if this transaction success.
                reqBooking.BookingStatus = "Completed";
                reqBooking.UpdatedAt = DateTime.Now;

                createRequest.PaidAmount = totalPaidAmount;
                createRequest.EndTime = reqBooking.EndTime;// to make sure the booking endtime is the same. 

            }

            //check time interval for booking and session
            //rule : checking bookings is for walkin customers , checking session is for duplicate detection (double creation of session).
            bool isBookingExist = false;
            if (!isBooking)
            {
                isBookingExist = _db.Bookings
                .Any(booking =>
                    booking.ConsoleId == createRequest.ConsoleId &&
                    booking.StartTime < createRequest.EndTime.Value &&
                    booking.EndTime > createRequest.StartTime.Value &&
                    booking.BookingStatus.ToLower() != "completed" &&
                    booking.BookingStatus.ToLower() != "cancelled" &&
                    booking.IsDeleted == false
                );
            }

            //final check of the booking customer 
            //note: this block can be move to booking check statement but I kept this here for visibility
            else
            {
                isBookingExist = _db.Bookings
               .Any(booking =>
                   booking.Id != createRequest.BookingId &&
                   booking.ConsoleId == createRequest.ConsoleId &&
                   booking.StartTime < createRequest.EndTime.Value &&
                   booking.EndTime > createRequest.StartTime.Value &&
                   booking.BookingStatus.ToLower() != "completed" &&
                   booking.BookingStatus.ToLower() != "cancelled" &&
                   booking.IsDeleted == false
                );

                

            }

            bool isSessionExist = _db.GameSessions
                .Any(session =>
                    session.ConsoleId == createRequest.ConsoleId &&
                    session.StartTime < createRequest.EndTime.Value &&
                    session.EndTime > createRequest.StartTime.Value &&
                    session.IsDeleted == false

                );
            //note : is this seat has session ? , if it is booking, is there overlapbooking ?
            if (isSessionExist || isBookingExist)
                return BadRequest($"Cannot create session due to conflict with bookings or sessions");

            //calculate totalhours and total amount for both type of customer
            TimeSpan sessionDuration = createRequest.EndTime.Value - createRequest.StartTime.Value;
            decimal totalHours = (decimal) sessionDuration.TotalHours;
            
            if (!isBooking)
            {
                totalAmount = reqConsole.Category.HourlyRate * totalHours;
            }
            reqConsole.CurrentStatus = "Taken";
            if (reqGame != null)
                reqGame.AvailableStock--;
            

            GameSession newGameSession = new GameSession
            {
                BookingId = createRequest.BookingId.HasValue ? createRequest.BookingId.Value : null, 
                ConsoleId = createRequest.ConsoleId.Value,
                GameId = createRequest.GameId.HasValue ? createRequest.GameId.Value : null,
                TotalHours = totalHours,
                TotalAmount = totalAmount,
                PaidAmount = createRequest.PaidAmount,
                StartTime = createRequest.StartTime.Value,
                EndTime = createRequest.EndTime.Value,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,


            };
            
            _db.GameSessions.Add(newGameSession);
            
            int status = _db.SaveChanges();
            transaction.Commit();
            return status > 0 ? Ok(_modelConverter.ConvertGameSessionResponse(newGameSession)) : BadRequest("Fail to create session");

        }

        // PATCH update game session
        [HttpPatch("{id}")]
        public IActionResult UpdateGameSession(int id, GameSessionModel.UpdateRequest updateRequest)
        {
            using var transaction = _db.BeginSchedulingTransaction();
            if (updateRequest == null)
                return BadRequest("Must define a body");

            bool hasChanges = false;
            var targetSession = _db.GameSessions.FirstOrDefault(s => s.Id == id);

            if (targetSession == null)
                return NotFound($"Game session with Id {id} doesn't exist.");

            if (targetSession.IsDeleted)
                return BadRequest("Cannot update a deleted game session.");

            // Update game with stock swap
            if (updateRequest.GameId.HasValue && updateRequest.GameId != targetSession.GameId)
            {
                var newGame = _db.Games.FirstOrDefault(g => g.Id == updateRequest.GameId.Value && g.IsDeleted == false);
                if (newGame == null)
                    return NotFound("Game is not found.");

                if (newGame.AvailableStock <= 0)
                    return BadRequest($"'{newGame.Title}' is currently out of stock.");

                // Return previous game back to stock
                if (targetSession.GameId.HasValue)
                {
                    var oldGame = _db.Games.FirstOrDefault(g => g.Id == targetSession.GameId.Value);
                    if (oldGame != null)
                    {
                        oldGame.AvailableStock++;
                    }
                }

                // Take new game from stock
                newGame.AvailableStock--;
                targetSession.GameId = newGame.Id;
                hasChanges = true;
            }

            // Update start and end time or schedule
            if (updateRequest.StartTime.HasValue || updateRequest.EndTime.HasValue)
            {
                DateTime newStartTime = updateRequest.StartTime ?? targetSession.StartTime;
                DateTime newEndTime = updateRequest.EndTime ?? targetSession.EndTime;

                if (newStartTime >= newEndTime)
                    return BadRequest("EndTime should not come first.");

                // Find overlap booking for this console
                bool isBookingExist = _db.Bookings
                    .Any(booking =>
                        booking.ConsoleId == targetSession.ConsoleId &&
                        booking.StartTime < newEndTime &&
                        booking.EndTime > newStartTime &&
                        booking.BookingStatus.ToLower() != "completed" &&
                        booking.BookingStatus.ToLower() != "cancelled" &&
                        booking.IsDeleted == false
                    );

                // Find overlap session for this console (excluding itself)
                bool isSessionExist = _db.GameSessions
                    .Any(session =>
                        session.Id != targetSession.Id &&
                        session.ConsoleId == targetSession.ConsoleId &&
                        session.StartTime < newEndTime &&
                        session.EndTime > newStartTime &&
                        session.IsDeleted == false
                    );

                if (isBookingExist || isSessionExist)
                    return Conflict($"Cannot update session time.Conflict with sessions and bookings.");

                targetSession.StartTime = newStartTime;
                targetSession.EndTime = newEndTime;

                // Recalculate duration, totalHours, and totalAmount
                TimeSpan duration = newEndTime - newStartTime;
                decimal totalHours = (decimal)duration.TotalHours;

                decimal? hourlyRate = _db.GameConsoles
                    .Include(c => c.Category)
                    .FirstOrDefault(c => c.Id == targetSession.ConsoleId)?.Category?.HourlyRate ?? 0;

                if (!hourlyRate.HasValue)
                    return NotFound("Invalid console category");

                targetSession.TotalHours = totalHours;
                targetSession.TotalAmount = totalHours * hourlyRate.Value;
                hasChanges = true;
            }

            // Update PaidAmount
            if (updateRequest.PaidAmount.HasValue)
            {
                if (updateRequest.PaidAmount.Value < 0)
                    return BadRequest("Paid amount cannot be negative.");

                targetSession.PaidAmount = updateRequest.PaidAmount.Value;
                hasChanges = true;
            }

            if (!hasChanges)
                return BadRequest("No changes provided.");

            targetSession.UpdatedAt = DateTime.Now;

            int saveStatus = _db.SaveChanges();
            transaction.Commit();
            return saveStatus > 0 ? Ok(_modelConverter.ConvertGameSessionResponse(targetSession)) : BadRequest("Unable to update game session.");
        }

        //delete game session with soft delete support
        [HttpDelete("{id}")]
        public IActionResult DeleteGameSession(int id, [FromQuery] bool isHardDelete = false)
        {
            using var transaction = _db.BeginSchedulingTransaction();
            var targetSession = _db.GameSessions.FirstOrDefault(s => s.Id == id);
            if (targetSession == null)
                return NotFound($"Game session with Id {id} doesn't exist");

            if (!targetSession.IsDeleted)
            {
                if (targetSession.GameId.HasValue)
                {
                    var game = _db.Games.FirstOrDefault(g => g.Id == targetSession.GameId.Value);
                    if (game != null)
                        game.AvailableStock++;
                }
                var console = _db.GameConsoles.FirstOrDefault(c => c.Id == targetSession.ConsoleId);
                if (console != null && console.CurrentStatus.Equals("Taken", StringComparison.OrdinalIgnoreCase) && !_db.GameSessions.Any(s => s.Id != id && s.ConsoleId == targetSession.ConsoleId && !s.IsDeleted))
                    console.CurrentStatus = "Available";
            }

            if (isHardDelete)
            {
                _db.GameSessions.Remove(targetSession);
            }
            else
            {
                targetSession.IsDeleted = true;
                targetSession.UpdatedAt = DateTime.Now;
            }

            int status = _db.SaveChanges();
            transaction.Commit();
            return status > 0 ? Ok("Successfully deleted") : BadRequest("Failed to delete game session");
        }

    }
}
