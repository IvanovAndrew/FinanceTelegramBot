using MediatR;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceSaveCurrencyCommand : IRequest
{
    public long SessionId { get; init; }
    public string Currency { get; init; }
}