using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QTip.Domain.Entities;
using QTip.Infrastructure.Persistence;

namespace QTip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private static readonly Regex EmailRegex = new(
        @"(?<![A-Za-z0-9._%+\-])([A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,})(?![A-Za-z0-9._%+\-])",
        RegexOptions.Compiled);

    private readonly QTipDbContext _db;

    public SubmissionsController(QTipDbContext db)
    {
        _db = db;
    }

    public sealed class CreateSubmissionRequest
    {
        public string Text { get; set; } = string.Empty;
    }

    public sealed class CreateSubmissionResponse
    {
        public int SubmissionId { get; set; }
        public string TokenizedText { get; set; } = string.Empty;
        public int EmailsDetected { get; set; }
        public int UniqueEmailsDetected { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult<CreateSubmissionResponse>> Create(
        [FromBody] CreateSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Text is required.");

        var originalText = request.Text;

        var matches = EmailRegex.Matches(originalText).Cast<Match>().ToList();
        if (matches.Count == 0)
        {
            var emptySubmission = new Submission
            {
                TokenizedText = originalText,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Submissions.Add(emptySubmission);
            await _db.SaveChangesAsync(cancellationToken);

            return Ok(new CreateSubmissionResponse
            {
                SubmissionId = emptySubmission.Id,
                TokenizedText = emptySubmission.TokenizedText,
                EmailsDetected = 0,
                UniqueEmailsDetected = 0
            });
        }
        
        var uniqueEmails = matches
            .Select(m => CanonicaliseEmail(m.Value))
            .Distinct()
            .ToList();

        var emailHashes = uniqueEmails
            .Select(ComputeSha256Hex)
            .ToList();
        
        var existingItems = await _db.ClassifiedItems
            .Where(x => x.Tag == "pii.email" && emailHashes.Contains(x.ValueHash))
            .ToListAsync(cancellationToken);

        var itemsByHash = existingItems.ToDictionary(x => x.ValueHash, x => x);
        
        foreach (var email in uniqueEmails)
        {
            var hash = ComputeSha256Hex(email);

            if (itemsByHash.ContainsKey(hash))
                continue;

            var newItem = new ClassifiedItem
            {
                Tag = "pii.email",
                SensitiveValue = email,
                ValueHash = hash,
                Token = $"{{{{TKN-email-{Guid.NewGuid():N}}}}}",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.ClassifiedItems.Add(newItem);
            itemsByHash[hash] = newItem;
        }
        
        var tokenByEmail = uniqueEmails.ToDictionary(
            e => e,
            e => itemsByHash[ComputeSha256Hex(e)].Token);

        var tokenizedText = EmailRegex.Replace(originalText, m =>
        {
            var email = CanonicaliseEmail(m.Value);
            return tokenByEmail[email];
        });

        var submission = new Submission
        {
            TokenizedText = tokenizedText,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        foreach (var match in matches)
        {
            var email = CanonicaliseEmail(match.Value);
            var hash = ComputeSha256Hex(email);
            var item = itemsByHash[hash];

            submission.Classifications.Add(new SubmissionClassification
            {
                ClassifiedItem = item,
                StartIndex = match.Index,
                Length = match.Length
            });
        }

        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new CreateSubmissionResponse
        {
            SubmissionId = submission.Id,
            TokenizedText = submission.TokenizedText,
            EmailsDetected = matches.Count,
            UniqueEmailsDetected = uniqueEmails.Count
        });
    }

    private static string CanonicaliseEmail(string value) =>
        value.Trim().ToLowerInvariant();

    private static string ComputeSha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
