using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.ReaderClub.Database.AppDbContextModels;
using Sep2026.ReaderClub.WebApi.Models;

namespace Sep2026.ReaderClub.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookReviewController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BookReviewController()
        {
            _db = new AppDbContext();
        }

        //get
        [HttpGet]
        public IActionResult GetBookReviews()
        {
            var lst = _db.BookReviews.ToList();
            return Ok(lst);
        }
        [HttpGet("{id}")]
        public IActionResult GetBookReviewById(int id)
        {
            var item = _db.BookReviews.FirstOrDefault(x => x.BookReviewId == id);
            if (item is null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        //create
        [HttpPost]
        public IActionResult CreateBookReview(BookReviewCreateRequestModel requestModel)
        {
            using var transaction = _db.Database.BeginTransaction();
            // Serialize manual ID allocation and keep both inserts atomic.
            _db.Database.ExecuteSqlRaw("SELECT 1 FROM BookReview WITH (TABLOCKX, HOLDLOCK); SELECT 1 FROM Review WITH (TABLOCKX, HOLDLOCK);");

            // Check whether ReviewerId exists
            bool reviewerExists = _db.Reviewers
                .Any(x => x.ReviewerId == requestModel.ReviewerId);

            if (!reviewerExists)
            {
                return BadRequest("ReviewerId does not exist.");
            }

            // Get next IDs
            int maxBookReviewId = _db.BookReviews
                .Select(x => x.BookReviewId)
                .ToList()
                .DefaultIfEmpty(0)
                .Max();

            int maxReviewId = _db.Reviews
                .Select(x => x.ReviewId)
                .ToList()
                .DefaultIfEmpty(0)
                .Max();

            // Insert BookReview
            _db.BookReviews.Add(new BookReview
            {
                BookReviewId = maxBookReviewId + 1,
                BookTitle = requestModel.BookTitle!.Trim(),
                Author = requestModel.Author!.Trim(),
                ReviewTitle = requestModel.ReviewTitle!.Trim(),
                ReviewContent = requestModel.ReviewContent!.Trim(),
                ReviewDate = requestModel.ReviewDate
            });

            _db.SaveChanges();

            // Insert relationship into Review
            _db.Reviews.Add(new Review
            {
                ReviewId = maxReviewId + 1,
                ReviewerId = requestModel.ReviewerId,
                BookReviewId = maxBookReviewId + 1
            });

            int result = _db.SaveChanges();
            transaction.Commit();

            string message = result > 0
                ? "Saving Successful."
                : "Saving Failed.";

            return Ok(message);
        }

        //patch
        [HttpPatch("{id}")]
        public IActionResult UpdateBookReview(int id, BookReviewUpdateRequestModel requestModel)
        {
            var item = _db.BookReviews.FirstOrDefault(x => x.BookReviewId == id);
            if (item is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(requestModel.BookTitle))
            {
                item.BookTitle = requestModel.BookTitle.Trim();
            }

            if (!string.IsNullOrEmpty(requestModel.Author))
            {
                item.Author = requestModel.Author.Trim();
            }

            if (!string.IsNullOrEmpty(requestModel.ReviewTitle))
            {
                item.ReviewTitle = requestModel.ReviewTitle.Trim();
            }

            if (!string.IsNullOrEmpty(requestModel.ReviewContent))
            {
                item.ReviewContent = requestModel.ReviewContent.Trim();
            }

            if (requestModel.ReviewDate.HasValue)
            {
                item.ReviewDate = requestModel.ReviewDate.Value;
            }

            int result = _db.SaveChanges();
            string message = result > 0 ? "Updating Successful." : "Updating Failed.";

            return Ok(message);
        }

        // Delete
        [HttpDelete("{id}")]
        public IActionResult DeleteBookReview(int id)
        {
            var item = _db.BookReviews.FirstOrDefault(x => x.BookReviewId == id);

            if (item is null)
            {
                return NotFound();
            }

            // Find associated Review records
            var reviews = _db.Reviews
                .Where(x => x.BookReviewId == id)
                .ToList();

            // Delete associated Review records first
            _db.Reviews.RemoveRange(reviews);

            // Delete BookReview
            _db.BookReviews.Remove(item);

            int result = _db.SaveChanges();

            string message = result > 0 ? "Deleting Successful." : "Deleting Failed.";

            return Ok(message);
        }
    }
}
