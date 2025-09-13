using System.Globalization;

namespace Application.Contracts;

public record CheckRequisite
{
    public DateTime DateTime;
    public decimal TotalPrice;
    public string FiscalNumber;
    public string FiscalDocumentNumber;
    public string FiscalDocumentSign;

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
                    checkRequisite.FiscalNumber = value;
                    break;
                case "i": 
                    checkRequisite.FiscalDocumentNumber = value;
                    break;
                case "fp":
                    checkRequisite.FiscalDocumentSign = value;
                    break;
                default:
                    break;
            };
        }

        return checkRequisite;
    }
}