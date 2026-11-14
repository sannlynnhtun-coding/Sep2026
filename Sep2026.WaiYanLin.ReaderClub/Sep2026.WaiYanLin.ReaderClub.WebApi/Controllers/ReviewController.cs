using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sep2026.ReaderClub.Database.AppDbContextModels;

namespace Sep2026.ReaderClub.WebApi.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReviewController()
        {
            _db = new AppDbContext();
        }

        [HttpGet]
        public IActionResult GetReviews()
        {
            var lst = _db.Reviews
                .Include(x => x.BookReview)
                .Include(x => x.Reviewer)
                .ToList();

            return Ok(lst);
        }


        [HttpGet("{id}")]
        public IActionResult GetReviewById(int id)
        {
            var item = _db.Reviews
                .Include(x => x.BookReview)
                .Include(x => x.Reviewer)
                .FirstOrDefault(x => x.ReviewId == id);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item);
        }
    }
}
