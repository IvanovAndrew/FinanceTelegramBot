namespace Application;

public class CheckRequisite
{
    public DateTime DateTime;
    public decimal TotalPrice;
    public string FiscalNumber;
    public string FiscalDocumentNumber;
    public string FiscalDocumentSign;

    public const int n = 1;

    public string ToQueryString()
    {
        return $"t={DateTime.ToString("yyyyMMdd'T'HHmm")}&" +
               $"s={TotalPrice}&" +
               $"fn={FiscalNumber}&" +
               $"i={FiscalDocumentNumber}&" +
               $"fp={FiscalDocumentSign}&" +
               "n=1";
    }
}