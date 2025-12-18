using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QTip.Domain.Entities;
using QTip.Infrastructure.Persistence;

namespace QTip.Api.Services;

public interface ISubmissionProcessor
{
    Task<SubmitTextResult> SubmitAsync(string text, CancellationToken ct);
    Task<int> GetTotalPiiEmailCountAsync(CancellationToken ct);
}

public sealed record SubmitTextResult(int SubmissionId, string TokenizedText, int TotalPiiEmailItemsSubmitted);

public class SubmissionProcessor : ISubmissionProcessor
{
    private const string EmailTag = "pii.email";

    private static readonly Regex EmailRegex = new(
        @"(?<![\w.+-])([A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,})(?![\w.+-])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private readonly QTipDbContext _db;

    public SubmissionProcessor(QTipDbContext db)
    {
        _db = db;
    }

    public async Task<SubmitTextResult> SubmitAsync(string text, CancellationToken ct)
    {
        text ??= string.Empty;

        var matches = EmailRegex.Matches(text).Cast<Match>().ToList();
        
        if (matches.Count == 0)
        {
            var submissionNoPii = new Submission { TokenizedText = text };
            _db.Submissions.Add(submissionNoPii);
            await _db.SaveChangesAsync(ct);

            var total = await GetTotalPiiEmailCountAsync(ct);
            return new SubmitTextResult(submissionNoPii.Id, submissionNoPii.TokenizedText, total);
        }
        
        var uniqueEmails = matches
            .Select(m => NormaliseEmail(m.Value))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var hashes = uniqueEmails.Select(Hash).ToList();
        
        var existing = await _db.ClassifiedItems
            .Where(x => x.Tag == EmailTag && hashes.Contains(x.ValueHash))
            .ToListAsync(ct);

        var byHash = existing.ToDictionary(x => x.ValueHash, StringComparer.Ordinal);
        
        foreach (var email in uniqueEmails)
        {
            var hash = Hash(email);
            if (byHash.ContainsKey(hash)) continue;

            var token = $"TKN_{NewTokenPart()}";

            var item = new ClassifiedItem
            {
                Tag = EmailTag,
                Token = token,
                SensitiveValue = email,
                ValueHash = hash
            };

            _db.ClassifiedItems.Add(item);
            byHash[hash] = item;
        }
        
        var tokenizedText = EmailRegex.Replace(text, m =>
        {
            var normalised = NormaliseEmail(m.Value);
            var hash = Hash(normalised);
            var item = byHash[hash];
            return $"{{{{{item.Token}}}}}";
        });
        
        var submission = new Submission
        {
            TokenizedText = tokenizedText
        };

        _db.Submissions.Add(submission);

        foreach (var match in matches)
        {
            var normalised = NormaliseEmail(match.Value);
            var hash = Hash(normalised);
            var item = byHash[hash];

            _db.SubmissionClassifications.Add(new SubmissionClassification
            {
                Submission = submission,
                ClassifiedItem = item,
                StartIndex = match.Index,
                Length = match.Length
            });
        }

        await _db.SaveChangesAsync(ct);

        var totalCount = await GetTotalPiiEmailCountAsync(ct);
        return new SubmitTextResult(submission.Id, submission.TokenizedText, totalCount);
    }

    public Task<int> GetTotalPiiEmailCountAsync(CancellationToken ct)
    {
        return _db.SubmissionClassifications.CountAsync(ct);
    }

    private static string NormaliseEmail(string email) => email.Trim().ToLowerInvariant();

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private static string NewTokenPart()
    {
        var bytes = RandomNumberGenerator.GetBytes(9);
        return Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");
    }
}
