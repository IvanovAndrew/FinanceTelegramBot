namespace Domain
{
    public class Currency
    {
        private readonly string Name;
        private readonly string Symbol;

        public static Currency Rur = new("RUR", "₽");
        public static Currency Amd = new("AMD", "֏");
    
        private Currency(string s, string symbol)
        {
            Name = s;
            Symbol = symbol;
        }

        public override string ToString() => Symbol;
    }
}