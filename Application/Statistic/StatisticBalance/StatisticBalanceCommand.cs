using MediatR;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceCommand : IRequest
{
    public long SessionId { get; init; }
}