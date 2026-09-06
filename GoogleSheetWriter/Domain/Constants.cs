namespace GoogleSheetWriter;

public static class Constants
{
    public static IReadOnlyList<string> BaseCurrencies = [Currency.AMD, Currency.RUR];

    public static class Currency
    {
        public static string RUR = "RUR";
        public static string AMD = "AMD";
        public static string GEL = "GEL";
        public static string USD = "USD";
        public static string EUR = "EUR";
    }
}