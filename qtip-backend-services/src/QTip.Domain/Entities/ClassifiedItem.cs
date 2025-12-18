namespace QTip.Domain.Entities;

public class ClassifiedItem
{
    public int Id { get; set; }

    public string Tag { get; set; } = "pii.email";
    
    public string Token { get; set; } = string.Empty;
    
    public string SensitiveValue { get; set; } = string.Empty;
    
    public string ValueHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<SubmissionClassification> SubmissionOccurrences { get; set; } = new List<SubmissionClassification>();
}