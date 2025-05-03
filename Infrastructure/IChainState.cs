using Application;
using Infrastructure.Telegram;

namespace Infrastructure;

public interface IChainState
{
    Task<IMessage> Request(IMessageService botClient, long chatId, CancellationToken cancellationToken = default);
    ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken);
}