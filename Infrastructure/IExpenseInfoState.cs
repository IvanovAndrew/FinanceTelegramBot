namespace Infrastructure
{
    public interface IExpenseInfoState
    {
        bool UserAnswerIsRequired { get; }
        
        Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default);
        Task Handle(IMessage message, CancellationToken cancellationToken);

        IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory);
        IExpenseInfoState MoveToNextState(IMessage message, IStateFactory stateFactory, CancellationToken cancellationToken)
        {
            if (TelegramCommand.TryGetCommand(message.Text, out var command))
            {
                return command.Execute(this, stateFactory);
            }

            return ToNextState(message, stateFactory, cancellationToken);
        }

        protected IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken);
    }
}