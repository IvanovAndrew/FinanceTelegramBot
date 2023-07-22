namespace Infrastructure;

public interface ILongTermOperation
{
    Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken);
    void Cancel();
}