namespace StateMachine;

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