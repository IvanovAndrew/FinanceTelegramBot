using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public class MoneyTransferSavedEvent : INotification
{
    public long SessionId { get; init; }
    public IMoneyTransfer MoneyTransfer { get; init; }
}