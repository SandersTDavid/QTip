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

    public sealed class PiiEmailCountResponse
    {
        public int TotalPiiEmailCount { get; set; }
    }

    [HttpGet("pii-email-count")]
    public async Task<ActionResult<PiiEmailCountResponse>> GetPiiEmailCount(CancellationToken cancellationToken)
    {
        var total = await _db.SubmissionClassifications
            .CountAsync(x => x.ClassifiedItem.Tag == "pii.email", cancellationToken);

        return Ok(new PiiEmailCountResponse
        {
            TotalPiiEmailCount = total
        });
    }
}