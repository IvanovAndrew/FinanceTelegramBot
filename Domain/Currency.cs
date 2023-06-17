namespace Domain
{
    public class Currency
    {
        private readonly string Name;

        public static Currency Rur = new Currency("RUR");
        public static Currency Amd = new Currency("AMD");
    
        private Currency(string s)
        {
            Name = s;
        }

        public override string ToString() => Name;
    }
}