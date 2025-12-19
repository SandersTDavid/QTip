using MediatR;

namespace QTip.Application.Submissions.CreateSubmission;

public sealed record CreateSubmissionCommand(string Text) : IRequest<CreateSubmissionResponse>;

public sealed record CreateSubmissionResponse(
    int SubmissionId,
    string TokenizedText,
    int PiiEmailCount
);