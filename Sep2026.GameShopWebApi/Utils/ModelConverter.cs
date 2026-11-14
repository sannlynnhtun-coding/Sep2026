using Sep2026.GameShopWebApi.Models;
using Sep2026.GameShopWebApi.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Utils
{
    public class ModelConverter
    {
        public GameConsoleModel.Response ConvertGameConsoleResponse(GameConsole gameConsole)
        {
            return new GameConsoleModel.Response { 
                Id = gameConsole.Id,
                ConsoleNumber = gameConsole.ConsoleNumber,
                CategoryId = gameConsole.CategoryId,
                CurrentStatus = gameConsole.CurrentStatus,
                IsDeleted = gameConsole.IsDeleted,
            };
        }

        public ConsoleCategoryModel.Response ConvertConsoleCategoryResponse(ConsoleCategory category)
        {
            return new ConsoleCategoryModel.Response
            {
                Id = category.Id,
                Name = category.Name,
                HourlyPrice = category.HourlyRate,
                IsDeleted = category.IsDeleted,
            };
        }

        public GamesModel.Response ConvertGameResponse(Game game)
        {
            return new GamesModel.Response
            {
                Id = game.Id,
                Title = game.Title,
                CategoryId = game.CategoryId,
                TotalStock = game.TotalStock,
                AvailableStock = game.AvailableStock,
                IsDeleted = game.IsDeleted,
            };
        }

        public BookingModel.Response ConvertBookingResponse(Booking booking)
        {
            return new BookingModel.Response
            {
                Id = booking.Id,
                ConsoleId = booking.ConsoleId,
                CustomerName = booking.CustomerName,
                CustomerPhone = booking.CustomerPhone,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                TotalHours = booking.TotalHours,
                TotalAmount = booking.TotalAmount,
                DepositAmount = booking.DepositAmount,
                BookingStatus = booking.BookingStatus,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                IsDeleted = booking.IsDeleted,
            };
        }

        public GameSessionModel.Response ConvertGameSessionResponse(GameSession session)
        {
            return new GameSessionModel.Response
            {
                Id = session.Id,
                BookingId = session.BookingId,
                ConsoleId = session.ConsoleId,
                GameId = session.GameId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                TotalHours = session.TotalHours,
                TotalAmount = session.TotalAmount,
                PaidAmount = session.PaidAmount,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                IsDeleted = session.IsDeleted,
            };
        }
    }
}
