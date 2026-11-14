using Microsoft.EntityFrameworkCore;

namespace Sep2026.MagicNumberTrick.Database.Models;

public class AppDbContext : DbContext
{
    private static readonly int[] KeyValues = [1, 2, 4, 8, 15, 28, 56];

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MagicNumberCard> MagicNumberCards { get; set; }
    public virtual DbSet<MagicNumberCardItem> MagicNumberCardItems { get; set; }

     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)        => optionsBuilder.UseSqlServer("Server=.;Database=sep_2026; ?? ;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MagicNumberCard>(entity =>
        {
            entity.HasKey(e => e.CardId);
            entity.ToTable("MagicNumberCard", "dbo");
            entity.HasIndex(e => e.CardNo).IsUnique();
            entity.Property(e => e.CardName).HasMaxLength(100);
        });

        modelBuilder.Entity<MagicNumberCardItem>(entity =>
        {
            entity.HasKey(e => e.CardItemId);
            entity.ToTable("MagicNumberCardItem", "dbo");
            entity.HasIndex(e => new { e.CardId, e.Number }).IsUnique();
            entity.HasOne(e => e.Card)
                .WithMany(e => e.CardItems)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MagicNumberCard>().HasData(
            KeyValues.Select((keyValue, index) => new MagicNumberCard
            {
                CardId = index + 1,
                CardNo = index + 1,
                KeyValue = keyValue,
                CardName = $"Card {index + 1}"
            }));

        modelBuilder.Entity<MagicNumberCardItem>().HasData(BuildCardItems());
    }

    private IEnumerable<MagicNumberCardItem> BuildCardItems()
    {
        var itemId = 1;

        for (var number = 1; number <= 100; number++)
        {
            var remaining = number;
            var selectedCardIds = new List<int>();

            for (var index = KeyValues.Length - 1; index >= 0; index--)
            {
                if (KeyValues[index] > remaining)
                {
                    continue;
                }

                remaining -= KeyValues[index];
                selectedCardIds.Add(index + 1);
            }

            foreach (var cardId in selectedCardIds)
            {
                yield return new MagicNumberCardItem
                {
                    CardItemId = itemId++,
                    CardId = cardId,
                    Number = number
                };
            }
        }
    }
}
