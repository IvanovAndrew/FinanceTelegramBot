namespace Infrastructure.Telegram;

public class TelegramWebHookInfo
{
    public string Url { get; set; }
    public string IpAddress { get; init; }
    
    public int PendingUpdateCount { get; init; }
    public int? MaxConnections { get; init; }
    
    public DateTime? LastErrorDate { get; init; }
    public string LastErrorMessage { get; init; }
}