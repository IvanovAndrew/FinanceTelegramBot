using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterCheckRequisitesState : StateWithChainsBase
{
    private readonly ILogger _logger;
    private readonly CheckRequisite _checkRequisite;
    
    public EnterCheckRequisitesState(DateTime now, ILogger logger)
    {
        _logger = logger;
        _checkRequisite = new CheckRequisite();
        StateChain = new StateChain(
            new EnterCheckDateTimeState(_checkRequisite, now), 
            new EnterCheckAmountState(_checkRequisite), 
            new EnterFiscalNumberState(_checkRequisite), 
            new EnterFiscalDocumentNumberState(_checkRequisite), 
            new EnterFiscalDocumentSignState(_checkRequisite));
    }

    protected override IExpenseInfoState PreviousState(IStateFactory stateFactory)
    {
        return stateFactory.CreateCheckInfoState(); 
    }

    protected override IExpenseInfoState NextState(IStateFactory stateFactory)
    {
        return stateFactory.CreateRequestFnsDataState(_checkRequisite.ToQueryString());
    }
}