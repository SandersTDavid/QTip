using Microsoft.EntityFrameworkCore;
using QTip.Domain.Entities;

namespace QTip.Infrastructure.Persistence;

public sealed class QTipDbContext : DbContext
{
    public QTipDbContext(DbContextOptions<QTipDbContext> options) : base(options) { }

    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<ClassifiedItem> ClassifiedItems => Set<ClassifiedItem>();
    public DbSet<SubmissionClassification> SubmissionClassifications => Set<SubmissionClassification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Submission>(b =>
        {
            b.ToTable("submissions");
            b.HasKey(x => x.Id);

            b.Property(x => x.TokenizedText)
                .IsRequired();

            b.Property(x => x.CreatedAtUtc)
                .IsRequired();

            b.HasMany(x => x.Classifications)
                .WithOne(x => x.Submission)
                .HasForeignKey(x => x.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClassifiedItem>(b =>
        {
            b.ToTable("classified_items");
            b.HasKey(x => x.Id);

            b.Property(x => x.Tag).IsRequired();
            b.Property(x => x.Token).IsRequired();
            b.Property(x => x.NormalizedValue).IsRequired();
            b.Property(x => x.RawValue).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            // One token per unique (Tag + NormalizedValue)
            b.HasIndex(x => new { x.Tag, x.NormalizedValue })
                .IsUnique();

            // Also ensure tokens are unique
            b.HasIndex(x => x.Token)
                .IsUnique();

            b.HasMany(x => x.Occurrences)
                .WithOne(x => x.ClassifiedItem)
                .HasForeignKey(x => x.ClassifiedItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SubmissionClassification>(b =>
        {
            b.ToTable("submission_classifications");
            b.HasKey(x => x.Id);

            b.Property(x => x.StartIndex).IsRequired();
            b.Property(x => x.Length).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasIndex(x => x.SubmissionId);
            b.HasIndex(x => x.ClassifiedItemId);
        });
    }
}
