using System;
using System.Collections.Generic;

namespace QTip.Domain.Entities;

/// <summary>
/// Vault record: one row per unique sensitive value per tag.
/// This is where the real email (sensitive) lives.
/// </summary>
public sealed class ClassifiedItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // e.g. "pii.email"
    public string Tag { get; set; } = "pii.email";

    // e.g. "{{TKN-abc123}}"
    public string Token { get; set; } = string.Empty;

    // Normalised for matching uniqueness (lowercase email, trimmed)
    public string NormalizedValue { get; set; } = string.Empty;

    // Original sensitive value (store as-is)
    public string RawValue { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<SubmissionClassification> Occurrences { get; set; } =
        new List<SubmissionClassification>();
}