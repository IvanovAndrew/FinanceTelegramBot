namespace Infrastructure.Telegram;

public class CommandAttribute : Attribute
{
    public string Text { get; init; } = "";
    public string Command { get; init; } = "";
    public int Order { get; init; }
}