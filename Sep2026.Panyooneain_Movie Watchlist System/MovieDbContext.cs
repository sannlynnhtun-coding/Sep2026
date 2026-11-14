using Microsoft.EntityFrameworkCore;

namespace Sep2026.Panyooneain_Movie_Watchlist_System.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
    }
}