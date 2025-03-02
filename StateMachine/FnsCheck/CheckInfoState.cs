using Infrastructure;
using Infrastructure.Telegram;

namespace StateMachine;

public class CheckInfoState : IExpenseInfoState
{
    public bool UserAnswerIsRequired => true;
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var keyboard = TelegramKeyboard.FromButtons(new[]
        {
            new TelegramButton{Text = "json", CallbackData = "json"},
            new TelegramButton{Text = "QR", CallbackData = "rawqr"},
            new TelegramButton{Text = "By requisites", CallbackData = "requisites"},
        });
        
        return await botClient.SendTextMessageAsync(
            new EditableMessageToSend(){ChatId = chatId, Text = "Enter the check", Keyboard = keyboard},
            cancellationToken: cancellationToken);
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return stateFactory.WayOfEnteringExpenseState();
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory, CancellationToken cancellationToken)
    {
        if (message.Text == "json") return stateFactory.CreateRequestPasteJsonState();
        if (message.Text == "rawqr") return stateFactory.CreateEnterRawQrState();
        if (message.Text == "requisites") return stateFactory.CreateCheckByRequisitesState();

        throw new BotStateException(new []{"json", "rawqr"}, message.Text);
    }
}