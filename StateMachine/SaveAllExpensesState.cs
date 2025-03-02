using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine;

public class SaveAllExpensesState : IExpenseInfoState, ILongTermOperation
{
    private readonly List<IMoneyTransfer> _expenses;
    private readonly IFinanceRepository _financeRepository;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    internal SaveAllExpensesState(List<IMoneyTransfer> expenses,
        IFinanceRepository financeRepository, ILogger logger)
    {
        _expenses = expenses;
        _financeRepository = financeRepository;
        _logger = logger;
    }

    public bool UserAnswerIsRequired => false;

    public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        if (TelegramCommand.TryGetCommand(message.Text, out _))
        {
            Cancel();
        }
        
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        Cancel();
        throw new NotImplementedException();
    }

    public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
    {
        var savingMessage = await botClient.SendTextMessageAsync(new EditableMessageToSend(){ChatId = message.ChatId, Text = "Saving... It can take some time."}, cancellationToken: cancellationToken);

        SaveBatchExpensesResult result;

        try
        {
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                await _financeRepository.SaveAllOutcomes(_expenses, _cancellationTokenSource.Token);
            }

            result = SaveBatchExpensesResult.Saved(_expenses);
        }
        catch (OperationCanceledException)
        {
            result = SaveBatchExpensesResult.Canceled(_expenses);
            _logger.LogInformation("Operation is canceled by user");
        }
        catch (Exception exception)
        {
            result = SaveBatchExpensesResult.Failed(_expenses, exception.Message);
        }
        finally
        {
            _cancellationTokenSource = null;
        }

        return await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ChatId = message.ChatId, Text = result.GetMessage()}, cancellationToken: cancellationToken);
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
    }

    public Task Cancel()
    {
        return Task.Run(() =>
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        });
    }
}