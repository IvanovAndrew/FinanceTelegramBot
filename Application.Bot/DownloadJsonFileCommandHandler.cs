using System.Net.Mime;
using Application.Core;
using MediatR;

namespace Application.Bot;

public class DownloadJsonFileCommandHandler(IMessageService messageService, IExpenseJsonParser parser, IConversation conversation, IMediator mediator) : IRequestHandler<DownloadJsonFileCommand>
{
    public async Task Handle(DownloadJsonFileCommand notification, CancellationToken cancellationToken)
    {
        var fileInfo = notification.FileInfo;

        if (fileInfo.MimeType != MediaTypeNames.Application.Json)
        {
            await conversation.Update(notification.SessionId, Screens.Notify("Paste a json file"), cancellationToken);
            return;
        }

        var file = await messageService.GetFileAsync(fileInfo.FileId, cancellationToken);

        if (file != null)
        {
            var check = parser.ParseCheck(file.Text);

            if (!check.Outcomes.Any())
            {
                await conversation.Update(notification.SessionId, Screens.Notify($"There are no expenses in file {fileInfo.FileName}"), cancellationToken);
                return;
            }
    
            await mediator.Send(new SaveOutcomesBatchCommand()
                { SessionId = notification.SessionId, MoneyTransfers = check.Outcomes, FileName = fileInfo.FileName }, cancellationToken);
        }
    }
}