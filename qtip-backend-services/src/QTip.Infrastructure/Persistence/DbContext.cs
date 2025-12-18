using Microsoft.EntityFrameworkCore;
using QTip.Domain.Entities;

namespace QTip.Infrastructure.Persistence;

public class QTipDbContext : DbContext
{
    public QTipDbContext(DbContextOptions<QTipDbContext> options) : base(options) { }

    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<ClassifiedItem> ClassifiedItems => Set<ClassifiedItem>();
    public DbSet<SubmissionClassification> SubmissionClassifications => Set<SubmissionClassification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Submission>(b =>
        {
            b.ToTable("Submissions");
            b.HasKey(x => x.Id);
            b.Property(x => x.TokenizedText).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<ClassifiedItem>(b =>
        {
            b.ToTable("ClassifiedItems");
            b.HasKey(x => x.Id);

            b.Property(x => x.Tag).IsRequired();
            b.Property(x => x.Token).IsRequired();
            b.Property(x => x.SensitiveValue).IsRequired();
            b.Property(x => x.ValueHash).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasIndex(x => x.Token).IsUnique();
            b.HasIndex(x => new { x.Tag, x.ValueHash }).IsUnique();
        });

        modelBuilder.Entity<SubmissionClassification>(b =>
        {
            b.ToTable("SubmissionClassifications");
            b.HasKey(x => x.Id);

            b.Property(x => x.StartIndex).IsRequired();
            b.Property(x => x.Length).IsRequired();

            b.HasOne(x => x.Submission)
                .WithMany(x => x.Classifications)
                .HasForeignKey(x => x.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ClassifiedItem)
                .WithMany(x => x.SubmissionOccurrences)
                .HasForeignKey(x => x.ClassifiedItemId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.SubmissionId);
            b.HasIndex(x => x.ClassifiedItemId);
        });
    }
}
