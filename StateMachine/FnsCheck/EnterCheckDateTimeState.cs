using System.Globalization;
using System.Text.RegularExpressions;
using Infrastructure;

namespace StateMachine;

internal class EnterCheckDateTimeState : IChainState
{
    private readonly DateTime _now;
    private const string DateTimeFormat = "dd/MM/yyyy hh:mm";
    private readonly CheckRequisite _requisite;

    public EnterCheckDateTimeState(CheckRequisite requisite, DateTime now)
    {
        _now = now;
        _requisite = requisite;
    }

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Enter date and time of the check. For example, {_now.ToString(DateTimeFormat)}",
            cancellationToken: cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        DateTime date;
        if (DateTime.TryParse(message.Text, new CultureInfo("ru-Ru"), DateTimeStyles.None, out date))
        {
            _requisite.DateTime = date;
            return ChainStatus.Success();
        }

        return ChainStatus.Retry(this);
    }
}

internal class EnterCheckAmountState : IChainState
{
    private readonly CheckRequisite _requisite;

    public EnterCheckAmountState(CheckRequisite requisite)
    {
        _requisite = requisite;
    }
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Enter the amount in rubles.",
            cancellationToken: cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        if (decimal.TryParse(message.Text, out var amount))
        {
            _requisite.Amount = amount;
            return ChainStatus.Success();
        }

        return ChainStatus.Retry(this);
    }
}

internal class EnterFiscalNumberState : IChainState
{
    private readonly CheckRequisite _requisite;

    public EnterFiscalNumberState(CheckRequisite requisite)
    {
        _requisite = requisite;
    }
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Enter the fiscal number. It should contain 16 digits",
            cancellationToken: cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        if (Regex.IsMatch(message.Text, "\\d{16}"))
        {
            _requisite.FiscalNumber = message.Text;
            return ChainStatus.Success();
        }

        return ChainStatus.Retry(this);
    }
}

internal class EnterFiscalDocumentNumberState : IChainState
{
    private readonly CheckRequisite _requisite;

    public EnterFiscalDocumentNumberState(CheckRequisite requisite)
    {
        _requisite = requisite;
    }
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Enter the fiscal document number. It should contain only digits",
            cancellationToken: cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        if (decimal.TryParse(message.Text, out _))
        {
            _requisite.FiscalDocumentNumber = message.Text;
            return ChainStatus.Success();
        }

        return ChainStatus.Retry(this);
    }
}

internal class EnterFiscalDocumentSignState : IChainState
{
    private readonly CheckRequisite _requisite;

    public EnterFiscalDocumentSignState(CheckRequisite requisite)
    {
        _requisite = requisite;
    }
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Enter the fiscal document sign. It should contain only digits",
            cancellationToken: cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        if (decimal.TryParse(message.Text, out _))
        {
            _requisite.FiscalDocumentSign = message.Text;
            return ChainStatus.Success();
        }

        return ChainStatus.Retry(this);
    }
}