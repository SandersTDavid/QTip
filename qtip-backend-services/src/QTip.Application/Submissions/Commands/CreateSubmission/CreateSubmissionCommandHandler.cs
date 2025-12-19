using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QTip.Application.Abstractions;
using QTip.Domain.Entities;

namespace QTip.Application.Submissions.CreateSubmission;

public sealed class CreateSubmissionCommandHandler : IRequestHandler<CreateSubmissionCommand, CreateSubmissionResponse>
{
    private const string EmailTag = "pii.email";

    private static readonly Regex EmailRegex = new(
        @"(?<![\w.+-])([A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,})(?![\w.+-])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly IQTipDbContext _db;

    public CreateSubmissionCommandHandler(IQTipDbContext db)
    {
        _db = db;
    }

    public async Task<CreateSubmissionResponse> Handle(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var original = request.Text;

        var matches = EmailRegex.Matches(original);
        if (matches.Count == 0)
        {
            var emptySubmission = new Submission
            {
                TokenizedText = original
            };

            _db.Submissions.Add(emptySubmission);
            await _db.SaveChangesAsync(cancellationToken);

            return new CreateSubmissionResponse(emptySubmission.Id, emptySubmission.TokenizedText, 0);
        }
        
        var tokenized = new StringBuilder(original);
        var createdAt = DateTimeOffset.UtcNow;

        var submission = new Submission
        {
            TokenizedText = original,
            CreatedAt = createdAt
        };

        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync(cancellationToken);

        var piiCount = 0;

        foreach (Match match in matches.Cast<Match>().OrderByDescending(m => m.Index))
        {
            var email = match.Value;
            var valueHash = ComputeSha256Hex(Normalise(email));

            var existing = await _db.ClassifiedItems
                .FirstOrDefaultAsync(x => x.Tag == EmailTag && x.ValueHash == valueHash, cancellationToken);

            ClassifiedItem item;
            if (existing is not null)
            {
                item = existing;
            }
            else
            {
                item = new ClassifiedItem
                {
                    Tag = EmailTag,
                    SensitiveValue = email,
                    ValueHash = valueHash,
                    Token = $"{{{{TKN-{Guid.NewGuid():N}}}}}",
                    CreatedAt = createdAt
                };

                _db.ClassifiedItems.Add(item);
                await _db.SaveChangesAsync(cancellationToken);
            }
            
            tokenized.Remove(match.Index, match.Length);
            tokenized.Insert(match.Index, item.Token);

            _db.SubmissionClassifications.Add(new SubmissionClassification
            {
                SubmissionId = submission.Id,
                ClassifiedItemId = item.Id,
                StartIndex = match.Index,
                Length = match.Length
            });

            piiCount++;
        }

        submission.TokenizedText = tokenized.ToString();

        await _db.SaveChangesAsync(cancellationToken);

        return new CreateSubmissionResponse(submission.Id, submission.TokenizedText, piiCount);
    }

    private static string Normalise(string value) => value.Trim().ToLowerInvariant();

    private static string ComputeSha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
