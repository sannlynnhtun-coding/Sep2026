using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sep2026.ReaderClub.Database.AppDbContextModels;
using Sep2026.ReaderClub.WebApi.Controllers;
using Sep2026.ReaderClub.WebApi.Models;

internal static class SqlRegression
{
    public static async Task RunAsync()
    {
        string database = "ReaderClubRegression_" + Guid.NewGuid().ToString("N");
        const string master = "Server=.;Database=master;Integrated Security=True;TrustServerCertificate=True;";
        var connection = new SqlConnectionStringBuilder(master) { InitialCatalog = database };
        string? original = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        bool created = false;
        try
        {
            await using (var admin = new SqlConnection(master))
            {
                await admin.OpenAsync();
                await new SqlCommand($"CREATE DATABASE [{database}]", admin).ExecuteNonQueryAsync();
                created = true;
            }

            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connection.ConnectionString);
            await using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();

            // Exercise the actual controller transactions, including simultaneous ID allocation.
            using var startReviewers = new ManualResetEventSlim();
            var reviewers = Enumerable.Range(0, 12).Select(i => Task.Run(() =>
            {
                startReviewers.Wait();
                var result = new ReviewerController().CreateReviewer(new ReviewerCreateRequestModel
                {
                    ReviewerName = $"Reviewer {i}", ProfileDescription = "Regression"
                });
                if (result is not OkObjectResult) throw new Exception("Reviewer create failed.");
            })).ToArray();
            startReviewers.Set();
            await Task.WhenAll(reviewers);
            var reviewerIds = await db.Reviewers.Select(x => x.ReviewerId).ToListAsync();
            if (reviewerIds.Count != 12 || reviewerIds.Distinct().Count() != 12)
                throw new Exception("Concurrent reviewer IDs were not unique.");

            BookReviewCreateRequestModel Request() => new()
            {
                ReviewerId = reviewerIds[0], BookTitle = "Book", Author = "Author",
                ReviewTitle = "Review", ReviewContent = "Content", ReviewDate = DateOnly.FromDateTime(DateTime.Today)
            };
            using var startReviews = new ManualResetEventSlim();
            var reviews = Enumerable.Range(0, 12).Select(_ => Task.Run(() =>
            {
                startReviews.Wait();
                if (new BookReviewController().CreateBookReview(Request()) is not OkObjectResult)
                    throw new Exception("Book review create failed.");
            })).ToArray();
            startReviews.Set();
            await Task.WhenAll(reviews);
            var bookIds = await db.BookReviews.Select(x => x.BookReviewId).ToListAsync();
            var reviewIds = await db.Reviews.Select(x => x.ReviewId).ToListAsync();
            if (bookIds.Count != 12 || bookIds.Distinct().Count() != 12 ||
                reviewIds.Count != 12 || reviewIds.Distinct().Count() != 12)
                throw new Exception("Concurrent book/review IDs were not unique.");

            await db.Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER RejectRegressionReview ON Review AFTER INSERT AS
                BEGIN
                    THROW 50001, 'Regression: reject relationship insert', 1;
                END
                """);
            bool rejected = false;
            try { new BookReviewController().CreateBookReview(Request()); }
            catch (DbUpdateException) { rejected = true; }
            if (!rejected) throw new Exception("Failure injection did not reject the relationship insert.");
            if (await db.BookReviews.CountAsync() != 12 || await db.Reviews.CountAsync() != 12)
                throw new Exception("Failed relationship insert left an orphan or changed committed rows.");
            Console.WriteLine("SQL regression passed: concurrent IDs unique; failed insert rolled back without orphans.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", original);
            SqlConnection.ClearAllPools();
            if (created)
            {
                await using var admin = new SqlConnection(master);
                await admin.OpenAsync();
                await new SqlCommand($"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]", admin).ExecuteNonQueryAsync();
            }
        }
    }
}
