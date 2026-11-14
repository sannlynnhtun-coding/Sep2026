using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }


    public virtual DbSet<PersonalityType> PersonalityTypes { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__PersonalityTestDb")
                ?? "Server=.;Database=Sept2026Db;Integrated Security=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(e => e.AnswerOptionId).HasName("PK__AnswerOp__CEB739B63071D19D");

            entity.Property(e => e.OptionText).HasMaxLength(300);

            entity.HasOne(d => d.PersonalityType).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.PersonalityTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AnswerOptions_PersonalityTypes");

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AnswerOptions_Questions");
        });


        modelBuilder.Entity<PersonalityType>(entity =>
        {
            entity.HasKey(e => e.PersonalityTypeId).HasName("PK__Personal__AEBA7F5766B8A4D2");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06FACCADBD794");

            entity.Property(e => e.QuestionText).HasMaxLength(500);
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__Test__8CC33160808FF376");

            entity.ToTable("Test");

            entity.HasOne(d => d.ResultPersonalityType).WithMany(p => p.Tests)
                .HasForeignKey(d => d.ResultPersonalityTypeId)
                .HasConstraintName("FK_Test_PersonalityTypes");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.UserAnswerId).HasName("PK__UserAnsw__47CE237F860E6CA9");

            entity.ToTable("UserAnswer");

            entity.HasIndex(e => new { e.TestId, e.QuestionId }, "UQ_UserAnswer_Test_Question").IsUnique();

            entity.HasOne(d => d.AnswerOption).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.AnswerOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswer_AnswerOption");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswer_Question");

            entity.HasOne(d => d.Test).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswer_Test");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
