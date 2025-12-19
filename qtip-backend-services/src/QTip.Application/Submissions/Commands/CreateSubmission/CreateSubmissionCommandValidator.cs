using FluentValidation;

namespace QTip.Application.Submissions.CreateSubmission;

public sealed class CreateSubmissionCommandValidator : AbstractValidator<CreateSubmissionCommand>
{
    public const int MaxChars = 50_000;

    public CreateSubmissionCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(MaxChars);
    }
}