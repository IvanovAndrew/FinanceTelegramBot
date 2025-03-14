﻿using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine.FnsCheck;

internal class SaveExpensesFromJsonState : IExpenseInfoState, ILongTermOperation
{
    private readonly List<IMoneyTransfer> _expenses;
    private readonly IFinanceRepository _financeRepository;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    
    internal SaveExpensesFromJsonState(List<IMoneyTransfer> expenses, IFinanceRepository financeRepository, ILogger logger)
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
        // TODO move calculation logic to here
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        throw new NotImplementedException();
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        return stateFactory.CreateGreetingState();
    }

    public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
    {
        if (TelegramCommand.TryGetCommand(message.Text, out _))
        {
            Cancel();
        }
        else
        {
            var savingMessage =
                await botClient.SendTextMessageAsync(new EditableMessageToSend(){ChatId = message.ChatId, Text = "Saving... It can take some time."}, cancellationToken: cancellationToken);

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
            catch (Exception e)
            {
                result = SaveBatchExpensesResult.Failed(_expenses, e.Message);
            }
            finally
            {
                _cancellationTokenSource = null;
            }

            var sum = new Money() { Amount = 0, Currency = _expenses[0].Amount.Currency };
            foreach (var expense in _expenses)
            {
                sum += expense.Amount;
            }

            return await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ChatId = message.ChatId, Text = result.GetMessage()}, cancellationToken: cancellationToken);
        }

        return await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ChatId = message.ChatId, Text = "Canceled"}, cancellationToken: cancellationToken);
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