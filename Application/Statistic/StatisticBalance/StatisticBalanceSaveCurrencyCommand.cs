using MediatR;

namespace Application.Statistic.StatisticBalance;

public record StatisticBalanceSaveCurrencyCommand : IRequest
{
    public long SessionId { get; init; }
    public string Currency { get; init; }
}