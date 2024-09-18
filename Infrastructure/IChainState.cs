using Infrastructure.Telegram;

namespace Infrastructure;

public interface IChainState
{
    Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default);
    ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken);
}