using System.Net.Mime;
using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class RequestJsonState : IExpenseInfoState
{
    public bool UserAnswerIsRequired => true;
    private readonly ILogger _logger;
        
    internal RequestJsonState(ILogger logger)
    {
        _logger = logger;
    }
        
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Paste json file",
            cancellationToken: cancellationToken);
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return stateFactory.WayOfEnteringExpenseState();
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        string info = "";
        if (message.FileInfo == null)
        {
            info = "File is not attached.";
        }
        else if (message.FileInfo.MimeType != MediaTypeNames.Application.Json)
        {
            info = $"json file is expected but {message.FileInfo.MimeType} was";
        }

        if (!string.IsNullOrEmpty(info))
        {
            _logger.LogInformation(info);
            return stateFactory.CreateErrorWithRetryState(info, this);
        }

        return stateFactory.CreateHandleJsonFileState(message.FileInfo!);
    }
}