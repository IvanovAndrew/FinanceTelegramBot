using Application;
using Infrastructure.Telegram;

namespace Infrastructure;

public interface ILongTermOperation
{
    Task<IMessage> Handle(IMessageService botClient, IMessage message, CancellationToken cancellationToken);
    Task Cancel();
}