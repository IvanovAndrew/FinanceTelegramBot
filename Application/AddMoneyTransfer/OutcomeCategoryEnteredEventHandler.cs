using Application.Events;
using MediatR;

namespace Application.AddMoneyTransfer;

public class OutcomeCategoryEnteredEventHandler(IMessageService messageService, IUserSessionService userSessionService)
    : INotificationHandler<OutcomeCategoryEnteredEvent>
{
    public async Task Handle(OutcomeCategoryEnteredEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var category = session.MoneyTransferBuilder.Category;

            if (category.Subcategories.Any())
            {
                await messageService.EditSentTextMessageAsync(new Message()
                {
                    ChatId = notification.SessionId,
                    Id = session.LastSentMessageId,
                    Text = "Enter the subcategory",
                    Options = MessageOptions.FromList(category.Subcategories.Select(c => c.Name).ToList()),
                }, cancellationToken: cancellationToken);
            }
            else
            {
                session.QuestionnaireService.Next();
                await messageService.EditSentTextMessageAsync(new Message()
                {
                    ChatId = notification.SessionId,
                    Id = notification.LastSentMessageId,
                    Text = "Enter the description",
                }, cancellationToken: cancellationToken);
            }
        }
    }
}