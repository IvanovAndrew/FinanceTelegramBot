using Application.Bot.Flows;
using Application.Contracts;
using Domain.Check;
using MediatR;

namespace Application.Bot;

public class UserSession
{
    public long Id { get; init; }

    public UserFlow? ActiveFlow { get; set; }
}

public abstract class UserFlow
{
    public FlowStep CurrentStep { get; protected set; }
    public abstract void ComputeStep();
    public abstract Task HandleInput(FlowStep step, string text, CancellationToken ct);
    public abstract Task Complete(long sessionId, IMediator mediator, CancellationToken ct);

    internal abstract ExtraData GetExtraData();
}

public enum FlowStep
{
    AskDay,
    AskCustomDay,
    AskDateTime,
    AskMonth,
    AskCustomMonth,
    AskOutcomeCategory,
    AskIncomeCategory,
    AskSubCategory,
    AskDescription,
    AskAmount,
    Confirm,
    Completed,
    AskFiscalNumber,
    AskFiscalDocumentNumber,
    AskFiscalDocumentSign,
    AskCurrency,
    AskStatisticMode,
    AskOrderId,
    AskUrlLink
}

public class FnsCheckRequisiteDraft
{
    private DateTime _dateTime;
    private decimal _price;
    private FiscalNumber _fiscalNumber;
    private FiscalDocumentNumber _fiscalDocumentNumber;
    private FiscalDocumentSign _fiscalDocumentSign;

    internal DateTime DateTime => _dateTime;
    internal decimal Price => _price;
    internal FiscalNumber FiscalNumber => _fiscalNumber;
    internal FiscalDocumentNumber FiscalDocumentNumber => _fiscalDocumentNumber;
    internal FiscalDocumentSign FiscalDocumentSign => _fiscalDocumentSign;

    public CheckRequisite ToEntity()
    {
        return new CheckRequisite()
        {
            DateTime = _dateTime,
            TotalPrice = _price,
            FiscalNumber = _fiscalNumber,
            FiscalDocumentNumber = _fiscalDocumentNumber,
            FiscalDocumentSign = _fiscalDocumentSign,
        };
    }

    public void SetDate(DateTime dateTime)
    {
        _dateTime = dateTime;
    }

    public void SetDocumentNumber(FiscalDocumentNumber documentNumber)
    {
        _fiscalDocumentNumber = documentNumber;
    }

    public void SetDocumentSign(FiscalDocumentSign documentSign)
    {
        _fiscalDocumentSign = documentSign;
    }

    public void SetPrice(decimal money)
    {
        _price = money;
    }

    public void SetFiscalNumber(FiscalNumber fiscalNumber)
    {
        _fiscalNumber = fiscalNumber;
    }
}

public class YerevanCityCheckRequisiteDraft
{
    public DateOnly? Date { get; private set; }
    public bool IsCustomDate { get; private set; }
    public string? CheckId { get; private set; }

    public void SetDate(DateOnly date)
    {
        Date = date;
    }
    
    public void SetCustomDate()
    {
        IsCustomDate = true;
    }

    public void SetCheckId(string checkId)
    {
        CheckId = checkId;
    }

    public YerevanCityCheckRequisite ToEntity()
    {
        return new YerevanCityCheckRequisite() { Date = Date.Value, OrderId = CheckId! };
    }

    
}