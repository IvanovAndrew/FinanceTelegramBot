using Domain;
using MediatR;

namespace Application.Statistic.StatisticBalance;

public class SpendingPerDayCalculatedEvent : INotification
{
    public long ChatId { get; set; }
    public Money MoneyPerDay { get; set; }
    public DateOnly ExpectedSalaryDay { get; set; }
}