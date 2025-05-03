using Application.Events;
using Domain;
using MediatR;

namespace Application.Commands.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryMonthSaveDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string DateFromText { get; init; }
}

public class StatisticBySubcategoryMonthSaveDateCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator) : IRequestHandler<StatisticBySubcategoryMonthSaveDateCommand>
{
    public async Task Handle(StatisticBySubcategoryMonthSaveDateCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (dateTimeService.TryParseDate(request.DateFromText, out var date))
            {
                session.StatisticsOptions.DateFrom = date;
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticBySubcategoryByMonthDateSavedEvent() { SessionId = session.Id, LastSentMessageId = session.LastSentMessageId}, cancellationToken);
            }
            else
            {
                session.LastSentMessageId = null;
                await mediator.Publish(new CustomDateRequestedEvent() { SessionId = session.Id }, cancellationToken);
            }
        }
    }
}

public class StatisticBySubcategoryByMonthDateSavedEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class StatisticBySubcategoryByMonthDateSavedEventHandler(IMessageService messageService) : INotificationHandler<StatisticBySubcategoryByMonthDateSavedEvent>
{
    public async Task Handle(StatisticBySubcategoryByMonthDateSavedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "Enter the currency",
                Options = MessageOptions.FromListAndLastSingleLine(Currency.GetAvailableCurrencies().Select(c => c.Name).ToList(), "All")
            }, cancellationToken);
    }
}