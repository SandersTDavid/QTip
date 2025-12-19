using MediatR;
using Microsoft.AspNetCore.Mvc;
using QTip.Application.Submissions.CreateSubmission;

namespace QTip.Api.Controllers;

[ApiController]
[Route("api/submissions")]
public sealed class SubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubmissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public sealed record CreateSubmissionRequest(string Text);

    [HttpPost]
    public async Task<ActionResult<CreateSubmissionResponse>> Create(
        [FromBody] CreateSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateSubmissionCommand(request.Text), cancellationToken);
        return Ok(result);
    }
}