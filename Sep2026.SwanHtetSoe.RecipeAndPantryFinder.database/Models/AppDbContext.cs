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
    public virtual DbSet<Recipe> Recipes { get; set; } = null!;
    public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; } = null!;

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

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.RecipeId).HasName("PK_Recipes");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Instructions).HasColumnType("NVARCHAR(MAX)");
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_RecipeIngredients");
            entity.Property(e => e.IngredientName).HasMaxLength(100);
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Unit).HasMaxLength(50);

            entity.HasOne(e => e.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(e => e.RecipeId)
                .HasConstraintName("FK_RecipeIngredients_Recipes")
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
