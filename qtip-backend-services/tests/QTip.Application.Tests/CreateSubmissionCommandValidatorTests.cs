using QTip.Application.Submissions.CreateSubmission;
using Xunit;

namespace QTip.Application.Tests.Submissions;

public class CreateSubmissionCommandValidatorTests
{
    [Fact]
    public void Text_longer_than_MaxChars_is_invalid()
    {
        // Arrange
        var validator = new CreateSubmissionCommandValidator();
        var tooLongText = new string('a', CreateSubmissionCommandValidator.MaxChars + 1);
        var command = new CreateSubmissionCommand(tooLongText);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateSubmissionCommand.Text));
    }

    [Fact]
    public void Text_exactly_MaxChars_is_valid()
    {
        // Arrange
        var validator = new CreateSubmissionCommandValidator();
        var maxText = new string('a', CreateSubmissionCommandValidator.MaxChars);
        var command = new CreateSubmissionCommand(maxText);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Empty_text_is_invalid()
    {
        // Arrange
        var validator = new CreateSubmissionCommandValidator();
        var command = new CreateSubmissionCommand(string.Empty);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateSubmissionCommand.Text));
    }
}