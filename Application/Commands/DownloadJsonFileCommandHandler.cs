using System.Net.Mime;
using Application.Events;
using MediatR;

namespace Application.Commands;

public class DownloadJsonFileCommandHandler(IUserSessionService userSessionService, IMessageService messageService, IMediator mediator) : IRequestHandler<DownloadJsonFileCommand>
{
    public async Task Handle(DownloadJsonFileCommand notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        var fileInfo = notification.FileInfo;

        if (session != null)
        {
            INotification domainEvent = null;
            if (fileInfo.MimeType == MediaTypeNames.Application.Json)
            {
                var file = await messageService.GetFileAsync(fileInfo.FileId, cancellationToken);

                if (file != null)
                {
                    domainEvent = new JsonFileDownloadedEvent() { SessionId = session.Id, Json = file.Text };
                }
            }
            else
            {
                domainEvent = new WrongFileExtensionReceivedEvent() { SessionId = session.Id };
            }

            await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}