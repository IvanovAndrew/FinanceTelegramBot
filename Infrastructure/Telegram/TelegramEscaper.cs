namespace TelegramBot.Services
{
    public static class TelegramEscaper
    {
        // List is taken from here https://doc.botboom.ru/nyuansy/telegram-markdown
        private static readonly string[] TelegramEscapeSymbols = {"_", "*", "[", "]", "(", ")", "~", /*"`",*/ ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
    
        public static string EscapeString(string s)
        {
            var result = s;
            foreach (var symbol in TelegramEscapeSymbols)
            {
                result = result.Replace(symbol, $@"\{symbol}");
            }

            return result;
        }

        public static  string Decode(string s)
        {
            var result = s;
            foreach (var symbol in TelegramEscapeSymbols)
            {
                result = result.Replace($@"\{symbol}", $"{symbol}");
            }

            return result;
        }
    }
}