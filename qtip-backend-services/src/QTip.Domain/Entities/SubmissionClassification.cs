namespace QTip.Domain.Entities;

public class SubmissionClassification
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public int ClassifiedItemId { get; set; }
    public ClassifiedItem ClassifiedItem { get; set; } = null!;
    
    public int StartIndex { get; set; }
    public int Length { get; set; }
}