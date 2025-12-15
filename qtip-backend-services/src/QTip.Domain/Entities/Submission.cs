using System;
using System.Collections.Generic;

namespace QTip.Domain.Entities;

public sealed class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Text after tokenisation (emails replaced by tokens)
    public string TokenizedText { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<SubmissionClassification> Classifications { get; set; } =
        new List<SubmissionClassification>();
}