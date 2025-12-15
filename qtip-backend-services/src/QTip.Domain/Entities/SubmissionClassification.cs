using System;

namespace QTip.Domain.Entities;

/// <summary>
/// Occurrence record: one row per detection in a submission.
/// This is what you count for "total PII emails submitted".
/// </summary>
public sealed class SubmissionClassification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public Guid ClassifiedItemId { get; set; }
    public ClassifiedItem ClassifiedItem { get; set; } = null!;

    // Optional but useful for future UI / debugging
    public int StartIndex { get; set; }
    public int Length { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}