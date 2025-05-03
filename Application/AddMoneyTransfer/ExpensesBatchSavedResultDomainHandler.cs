using MediatR;

namespace Application.Events;

public class ExpensesBatchSavedResultEvent : INotification
{
    public long SessionId { get; init; }
    public SaveBatchExpensesResult Result { get; init; }
}

public class ExpensesBatchSavedResultEventHandler(IMessageService messageService) : INotificationHandler<ExpensesBatchSavedResultEvent>
{
    public async Task Handle(ExpensesBatchSavedResultEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Text = notification.Result.GetMessage(),
            }, cancellationToken);
    }
}