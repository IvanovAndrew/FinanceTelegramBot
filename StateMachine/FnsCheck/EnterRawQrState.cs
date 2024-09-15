using Domain;
using Infrastructure;
using Infrastructure.Fns;
using Infrastructure.Fns.DataContract;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine;

public class EnterRawQrState : IExpenseInfoState
{
    private readonly IFnsService _fnsService;
    private FnsResponse? _check;
    private readonly ILogger _logger;
    
    public EnterRawQrState(IFnsService fnsService, ILogger logger)
    {
        _logger = logger;
        _fnsService = fnsService;
    }

    public bool UserAnswerIsRequired => true;
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(chatId, "Enter the string you get after QR reading", cancellationToken: cancellationToken);
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) =>
        stateFactory.CreateCheckInfoState();

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        return stateFactory.CreateRequestFnsDataState(message.Text);
    }
}