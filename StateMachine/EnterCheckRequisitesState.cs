using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

public class EnterCheckRequisitesState : IExpenseInfoState
{
    private readonly ILogger _logger;
    private readonly StateChain _stateChain;
    private readonly CheckRequisite _checkRequisite;
    public bool UserAnswerIsRequired => true;
    
    public EnterCheckRequisitesState(DateTime now, ILogger logger)
    {
        _logger = logger;
        _checkRequisite = new CheckRequisite();
        _stateChain = new StateChain(
            new EnterCheckDateTimeState(_checkRequisite, now), 
            new EnterCheckAmountState(_checkRequisite), 
            new EnterFiscalNumberState(_checkRequisite), 
            new EnterFiscalDocumentNumberState(_checkRequisite), 
            new EnterFiscalDocumentSignState(_checkRequisite));
    }
    
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await _stateChain.Request(botClient, chatId, cancellationToken);
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return _stateChain.MoveToPreviousState().IsOutOfChain ? stateFactory.CreateCheckInfoState() : this; 
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory, CancellationToken cancellationToken)
    {
        var moveStatus = _stateChain.ToNextState();

        if (moveStatus.IsOutOfChain)
        {
            return stateFactory.CreateRequestFnsDataState(_checkRequisite.ToQueryString());
        }
        
        return this;
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        _stateChain.Handle(message, cancellationToken);
        return Task.CompletedTask;
    }
}

internal class CheckRequisite
{
    public DateTime DateTime;
    public decimal Amount;
    public string FiscalNumber;
    public string FiscalDocumentNumber;
    public string FiscalDocumentSign;

    public const int n = 1;

    public string ToQueryString()
    {
        return $"t={DateTime.ToString("yyyyMMdd'T'HHmm")}&" +
               $"s={Amount}&" +
               $"fn={FiscalNumber}&" +
               $"i={FiscalDocumentNumber}&" +
               $"fp={FiscalDocumentSign}&" +
               "n=1";
    }
}