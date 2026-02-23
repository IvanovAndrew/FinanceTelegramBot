using Application.Contracts;
using Domain.Check;

namespace Application;

public class UserSession
{
    public long Id { get; init; }
    public int? LastSentMessageId { get; set; }

    public UserFlow? ActiveFlow { get; set; }
}

public abstract class UserFlow
{
    public FlowStep CurrentStep { get; protected set; }
    public abstract void ComputeStep();
    public abstract Task HandleInput(FlowStep step, string text, CancellationToken ct);

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
    AskStatisticMode
}

public class CheckRequisiteDraft
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