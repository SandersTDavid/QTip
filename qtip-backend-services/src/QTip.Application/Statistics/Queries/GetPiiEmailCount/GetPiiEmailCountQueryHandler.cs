using MediatR;
using Microsoft.EntityFrameworkCore;
using QTip.Application.Abstractions;

namespace QTip.Application.Stats.GetPiiEmailCount;

public sealed class GetPiiEmailCountQueryHandler : IRequestHandler<GetPiiEmailCountQuery, int>
{
    private readonly IQTipDbContext _db;

    public GetPiiEmailCountQueryHandler(IQTipDbContext db)
    {
        _db = db;
    }

    public Task<int> Handle(GetPiiEmailCountQuery request, CancellationToken cancellationToken)
    {
        return _db.SubmissionClassifications.CountAsync(
            x => x.ClassifiedItem.Tag == "pii.email",
            cancellationToken);
    }
}