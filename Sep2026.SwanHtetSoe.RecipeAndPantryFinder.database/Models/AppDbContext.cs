using Microsoft.EntityFrameworkCore;

namespace Sep2026.ReceipeDb.Models;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PantryItem> PantryItems { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                Environment.GetEnvironmentVariable("ConnectionStrings__Sep2026Db")
                ?? "Server=.;Database=Sep2026Db;Integrated Security=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PantryItem>(entity =>
        {
            entity.HasKey(e => e.PantryItemId).HasName("PK_PantryItems");

            entity.Property(e => e.ItemName).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Unit).HasMaxLength(50);
        });
    }
}
