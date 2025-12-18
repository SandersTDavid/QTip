namespace QTip.Domain.Entities;

public class Submission
{
    public int Id { get; set; }
    public string TokenizedText { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<SubmissionClassification> Classifications { get; set; } = new List<SubmissionClassification>();
}