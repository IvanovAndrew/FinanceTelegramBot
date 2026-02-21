using System.Globalization;
using Domain.Check;

namespace Application.Contracts;

public record CheckRequisite
{
    public DateTime DateTime;
    public decimal TotalPrice;
    public FiscalNumber FiscalNumber;
    public FiscalDocumentNumber FiscalDocumentNumber;
    public FiscalDocumentSign FiscalDocumentSign;

    public const int n = 1;

    public string ToQueryString()
    {
        return $"t={DateTime:yyyyMMdd'T'HHmm}&" +
               $"s={TotalPrice}&" +
               $"fn={FiscalNumber}&" +
               $"i={FiscalDocumentNumber}&" +
               $"fp={FiscalDocumentSign}&" +
               "n=1";
    }

    public static CheckRequisite FromUrlLink(string s)
    {
        var parts = s.Split('&', StringSplitOptions.RemoveEmptyEntries);

        var checkRequisite = new CheckRequisite();

        foreach (var part in parts)
        {
            var keyValue = part.Split('=', StringSplitOptions.RemoveEmptyEntries);
            var key = keyValue[0];
            var value = keyValue[1];

            switch(key)
            {
                case "t":
                    checkRequisite.DateTime = DateTime.TryParseExact(value, "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                        ? d
                        : DateTime.TryParseExact(value, "yyyyMMdd'T'HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
                            ? d
                            : new DateTime(2000, 1, 1);

                    break;
                case "s":
                    checkRequisite.TotalPrice = decimal.Parse(value);
                    break;
                case "fn":
                    checkRequisite.FiscalNumber = FiscalNumber.Create(value).Value;
                    break;
                case "i": 
                    checkRequisite.FiscalDocumentNumber = FiscalDocumentNumber.Create(value).Value;
                    break;
                case "fp":
                    checkRequisite.FiscalDocumentSign = FiscalDocumentSign.Create(value).Value;
                    break;
                default:
                    break;
            };
        }

        return checkRequisite;
    }
}