using Microsoft.EntityFrameworkCore;
using QTip.Domain.Entities;

namespace QTip.Application.Abstractions;

public interface IQTipDbContext
{
    DbSet<Submission> Submissions { get; }
    DbSet<ClassifiedItem> ClassifiedItems { get; }
    DbSet<SubmissionClassification> SubmissionClassifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}