using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Sep2026.ReaderClub.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookReview> BookReviews { get; set; }

    public virtual DbSet<Borrower> Borrowers { get; set; }

    public virtual DbSet<Borrowing> Borrowings { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Reviewer> Reviewers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Server=.;Database=Sep2026Db;Integrated Security=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Book__3DE0C2075F28D054");

            entity.ToTable("Book");

            entity.HasIndex(e => e.BookNo, "UQ__Book__3DE0BAA904E28DD0").IsUnique();

            entity.Property(e => e.Author).HasMaxLength(150);
            entity.Property(e => e.BookNo).HasMaxLength(50);
            entity.Property(e => e.BookTitle).HasMaxLength(200);
        });

        modelBuilder.Entity<BookReview>(entity =>
        {
            entity.HasKey(e => e.BookReviewId).HasName("PK__BookRevi__02F5EA559B8642A7");

            entity.ToTable("BookReview");

            entity.Property(e => e.BookReviewId).ValueGeneratedNever();
            entity.Property(e => e.Author).HasMaxLength(255);
            entity.Property(e => e.BookTitle).HasMaxLength(255);
            entity.Property(e => e.ReviewTitle).HasMaxLength(255);
        });

        modelBuilder.Entity<Borrower>(entity =>
        {
            entity.HasKey(e => e.BorrowerId).HasName("PK__Borrower__568EDB57EF6D0088");

            entity.ToTable("Borrower");

            entity.HasIndex(e => e.No, "UQ__Borrower__3214D4A91D99CDB4").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.No).HasMaxLength(50);
        });

        modelBuilder.Entity<Borrowing>(entity =>
        {
            entity.HasKey(e => e.BorrowingId).HasName("PK__Borrowin__6CD933D7E00942D5");

            entity.ToTable("Borrowing");

            entity.Property(e => e.BookNo).HasMaxLength(50);
            entity.Property(e => e.BorrowDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.BorrowerNo).HasMaxLength(50);

            entity.HasOne(d => d.BookNoNavigation).WithMany(p => p.Borrowings)
                .HasPrincipalKey(p => p.BookNo)
                .HasForeignKey(d => d.BookNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Borrowing_Book");

            entity.HasOne(d => d.BorrowerNoNavigation).WithMany(p => p.Borrowings)
                .HasPrincipalKey(p => p.No)
                .HasForeignKey(d => d.BorrowerNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Borrowing_Borrower");
        });


        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Review__74BC79CEF1EE4FDC");

            entity.ToTable("Review");

            entity.Property(e => e.ReviewId).ValueGeneratedNever();

            entity.HasOne(d => d.BookReview).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BookReviewId)
                .HasConstraintName("FK__Review__BookRevi__6754599E");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("FK__Review__Reviewer__66603565");
        });

        modelBuilder.Entity<Reviewer>(entity =>
        {
            entity.HasKey(e => e.ReviewerId).HasName("PK__Reviewer__1616CFDD70FBEDA8");

            entity.ToTable("Reviewer");

            entity.Property(e => e.ReviewerId).ValueGeneratedNever();
            entity.Property(e => e.ReviewerName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
