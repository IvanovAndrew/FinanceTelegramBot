namespace Infrastructure;

public class TelegramKeyboard
{
    public IReadOnlyList<TelegramButton[]> Buttons { get; }
    private TelegramKeyboard(List<TelegramButton[]> buttons)
    {
        Buttons = buttons;
    }


    public static TelegramKeyboard FromButtons(IEnumerable<TelegramButton> buttons, int chunkSize = 4)
    {
        return new TelegramKeyboard(buttons.Chunk(chunkSize).ToList());
    }
}