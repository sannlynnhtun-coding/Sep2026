
using Microsoft.AspNetCore.Mvc;
using Sep2026.Panyooneain_Movie_Watchlist_System.Data;
using Sep2026.Panyooneain_Movie_Watchlist_System;

namespace Sep2026.Panyooneain_Movie_Watchlist_System.Controllers
{
    // api/movies
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly MovieDbContext _db;

        public MoviesController(MovieDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetMovies()
        {
            var lst = _db.Movies.ToList();
            return Ok(lst);
        }

        [HttpGet("Edit/{id}")]
        public IActionResult GetMovie(int id)
        {
            var item = _db.Movies.FirstOrDefault(x => x.MovieId == id);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public IActionResult CreateMovie(Movie requestModel)
        {
            _db.Movies.Add(new Movie
            {
                Title = requestModel.Title.Trim(),
                Genre = requestModel.Genre.Trim(),
                ReleaseYear = requestModel.ReleaseYear,
                Rating = requestModel.Rating,
                Status = requestModel.Status.Trim()
            });

            int result = _db.SaveChanges();

            string message = result > 0
                ? "Saving Successful."
                : "Saving Failed.";

            return Ok(message);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMovie(int id, Movie requestModel)
        {
            var item = _db.Movies.FirstOrDefault(x => x.MovieId == id);

            if (item is null)
            {
                return NotFound();
            }

            item.Title = requestModel.Title.Trim();
            item.Genre = requestModel.Genre.Trim();
            item.ReleaseYear = requestModel.ReleaseYear;
            item.Rating = requestModel.Rating;
            item.Status = requestModel.Status.Trim();

            int result = _db.SaveChanges();

            string message = result > 0
                ? "Updating Successful."
                : "Updating Failed.";

            return Ok(message);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMovie(int id)
        {
            var item = _db.Movies.FirstOrDefault(x => x.MovieId == id);

            if (item is null)
            {
                return NotFound();
            }

            _db.Movies.Remove(item);

            int result = _db.SaveChanges();

            string message = result > 0
                ? "Deleting Successful."
                : "Deleting Failed.";

            return Ok(message);
        }
    }
}
