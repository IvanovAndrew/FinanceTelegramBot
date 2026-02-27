using System.Net.Mime;
using Application.Events;
using MediatR;

namespace Application.Commands;

public class DownloadJsonFileCommandHandler(IMessageService messageService, IMediator mediator) : IRequestHandler<DownloadJsonFileCommand>
{
    public async Task Handle(DownloadJsonFileCommand notification, CancellationToken cancellationToken)
    {
        var fileInfo = notification.FileInfo;

        INotification domainEvent = null;
        if (fileInfo.MimeType == MediaTypeNames.Application.Json)
        {
            var file = await messageService.GetFileAsync(fileInfo.FileId, cancellationToken);

            if (file != null)
            {
                domainEvent = new JsonFileDownloadedEvent() { SessionId = notification.SessionId, Json = file.Text, FileName = fileInfo.FileName };
            }
        }
        else
        {
            domainEvent = new WrongFileExtensionReceivedEvent() { SessionId = notification.SessionId };
        }

        await mediator.Publish(domainEvent, cancellationToken);
    }
}