using MediatR;

namespace QTip.Application.Stats.GetPiiEmailCount;

public sealed record GetPiiEmailCountQuery() : IRequest<int>;