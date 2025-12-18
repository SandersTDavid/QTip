using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QTip.Infrastructure.Persistence;

namespace QTip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly QTipDbContext _db;

    public StatisticsController(QTipDbContext db)
    {
        _db = db;
    }

    public sealed class StatisticsResponse
    {
        public int TotalPiiEmailsSubmitted { get; set; }
    }

    [HttpGet]
    public async Task<ActionResult<StatisticsResponse>> Get(CancellationToken cancellationToken)
    {
        var total = await _db.SubmissionClassifications
            .CountAsync(x => x.ClassifiedItem.Tag == "pii.email", cancellationToken);

        return Ok(new StatisticsResponse
        {
            TotalPiiEmailsSubmitted = total
        });
    }
    
    [HttpGet("pii-email-count")]
    public async Task<ActionResult<int>> GetPiiEmailCount(CancellationToken cancellationToken)
    {
        var total = await _db.SubmissionClassifications
            .CountAsync(x => x.ClassifiedItem.Tag == "pii.email", cancellationToken);

        return Ok(total);
    }

}