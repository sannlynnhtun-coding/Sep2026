using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.ReaderClub.Database.AppDbContextModels;
using Sep2026.ReaderClub.WebApi.Models;

namespace Sep2026.ReaderClub.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewerController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReviewerController()
        {
            _db = new AppDbContext();
        }

        //get
        [HttpGet]
        public IActionResult GetReviewers()
        {
            var lst = _db.Reviewers.ToList();
            return Ok(lst);
        }

        //get by id
        [HttpGet("{id}")]
        public IActionResult GetReviewerById(int id)
        {
            var item = _db.Reviewers.FirstOrDefault(x => x.ReviewerId == id);
            if (item is null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        //create
        [HttpPost]
        public IActionResult CreateReviewer(ReviewerCreateRequestModel requestModel)
        {
            using var transaction = _db.Database.BeginTransaction();
            // Serialize allocation because this table does not generate IDs.
            _db.Database.ExecuteSqlRaw("SELECT 1 FROM Reviewer WITH (TABLOCKX, HOLDLOCK);");
            int maxId = _db.Reviewers
                .Select(x => x.ReviewerId)
                .ToList()
                .DefaultIfEmpty(0)
                .Max();

            _db.Reviewers.Add(new Reviewer
            {
                ReviewerId = maxId + 1,
                ReviewerName = requestModel.ReviewerName!.Trim(),
                ProfileDescription = requestModel.ProfileDescription!.Trim(),
            });

            int result = _db.SaveChanges();
            transaction.Commit();

            return Ok(result > 0 ? "Saving Successful." : "Saving Failed.");
        }


        //patch
        [HttpPatch("{id}")]
        public IActionResult UpdateReviewer(int id, ReviewerUpdateRequestModel requestModel)
        {
            var item = _db.Reviewers.FirstOrDefault(x => x.ReviewerId == id);
            if (item is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(requestModel.ReviewerName))
            {
                item.ReviewerName = requestModel.ReviewerName.Trim();
            }

            if (!string.IsNullOrEmpty(requestModel.ProfileDescription))
            {
                item.ProfileDescription = requestModel.ProfileDescription.Trim();
            }

            int result = _db.SaveChanges();
            string message = result > 0 ? "Updating Successful." : "Updating Failed.";

            return Ok(message);
        }


        //delete
        [HttpDelete("{id}")]
        public IActionResult DeleteReviewer(int id)
        {
            // Check whether reviewer exists
            var reviewer = _db.Reviewers
                .FirstOrDefault(x => x.ReviewerId == id);

            if (reviewer == null)
            {
                return NotFound("Reviewer not found.");
            }

            // Get associated Review records
            var reviews = _db.Reviews
                .Where(x => x.ReviewerId == id)
                .ToList();

            // Get associated BookReviewIds
            var bookReviewIds = reviews
                .Select(x => x.BookReviewId)
                .ToList();

            // Get associated BookReview records
            var bookReviews = _db.BookReviews
                .Where(x => bookReviewIds.Contains(x.BookReviewId))
                .ToList();

            // Delete Review records
            _db.Reviews.RemoveRange(reviews);

            // Delete BookReview records
            _db.BookReviews.RemoveRange(bookReviews);

            // Delete Reviewer
            _db.Reviewers.Remove(reviewer);

            int result = _db.SaveChanges();

            return Ok(result > 0
                ? "Deleting Successful."
                : "Deleting Failed.");
        }


    }

}

