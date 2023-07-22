using System.Net.Mime;
using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class RequestJsonState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }
    private readonly ILogger _logger;
        
    internal RequestJsonState(StateFactory factory, IExpenseInfoState previousState, ILogger logger)
    {
        _factory = factory;
        _logger = logger;
        PreviousState = previousState;
    }
        
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Paste json file",
            cancellationToken: cancellationToken);
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Run(() => { });
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
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
            return _factory.CreateErrorWithRetryState(info, this);
        }

        return _factory.CreateHandleJsonFileState(this, message.FileInfo!);
    }
}