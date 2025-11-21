using MediatR;

namespace Application.Statistic.StatisticBalance;

public record StatisticBalanceCommand : IRequest
{
    public long SessionId { get; init; }
}