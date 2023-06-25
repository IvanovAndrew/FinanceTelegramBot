using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;
using TelegramBot;

namespace StateMachine
{
    internal class EnterCategoryForStatisticState : IExpenseInfoState
    {
        private StateFactory _factory;
        private readonly IEnumerable<Category> _categories;
        private ILogger _logger;
        
        public EnterCategoryForStatisticState(StateFactory factory, IExpenseInfoState previousState, IEnumerable<Category> categories, ILogger logger)
        {
            _factory = factory;
            PreviousState = previousState;
            _logger = logger;
            _categories = categories;
        }

        public bool UserAnswerIsRequired { get; } = true;
        public IExpenseInfoState PreviousState { get; }
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            string infoMessage = "Enter the category";

            var  keyboard = TelegramKeyboard.FromButtons(
                _categories.Select(c => new TelegramButton(){Text = c.Name, CallbackData = c.Name})
            );
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: infoMessage,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            var categoryDomain = _categories.FirstOrDefault(c => c.Name == text);
            if (categoryDomain != null)
            {
                return _factory.GetEnterTypeOfCategoryStatistic(this, categoryDomain);
            }

            return this;
        }
    }

    internal class EnterTypeOfCategoryStatisticState : IExpenseInfoState
    {
        private StateFactory _factory;
        private readonly string _category;
        private ILogger _logger;
        public bool UserAnswerIsRequired => true;
        public IExpenseInfoState PreviousState { get; }
        
        public EnterTypeOfCategoryStatisticState(StateFactory factory, IExpenseInfoState previousState, string category, ILogger logger)
        {
            _factory = factory;
            _category = category;
            PreviousState = previousState;
            _logger = logger;
        }
        
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var keyboard = TelegramKeyboard.FromButtons(new[]
            {
                new TelegramButton()
                {
                    Text = "Subcategory", 
                    CallbackData = "subcategory"
                },
                new TelegramButton(){Text = "For last year", CallbackData = "lastyear"}, 
            });
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Choose",
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            if (text == "subcategory")
            {
                var firstDayOfMonth = DateOnly.FromDateTime(DateTime.Today).FirstDayOfMonth();
                
                var expenseAggregator = new ExpensesAggregator<string>(
                    e => e.SubCategory?? string.Empty, s => s, true, sortAsc:false);
                return _factory.GetExpensesState(this, 
                    d => d >= firstDayOfMonth, 
                    c => string.Equals(c, _category), 
                    expenseAggregator, 
                    new TableOptions()
                    {
                        Title = $"Category: {_category}. Expenses from {firstDayOfMonth.ToString("dd MMMM yyyy")}",
                        ColumnNames = new []{"Subcategory", "AMD", "RUR"}
                    });
            }

            if (text == "lastyear")
            {
                var expenseAggregator = new ExpensesAggregator<DateOnly>(
                    e => e.Date.LastDayOfMonth(), d => d.ToString("yyyy MMM"), false, sortAsc:true);
                return _factory.GetExpensesState(this, 
                    d => d >= DateOnly.FromDateTime(DateTime.Today.AddYears(-1)).FirstDayOfMonth(), 
                    c => string.Equals(c, _category), expenseAggregator,
                    new TableOptions()
                    {
                        Title = $"Category: {_category}",
                        ColumnNames = new []{"Month", "AMD", "RUR"}
                    });
            }

            throw new ArgumentOutOfRangeException(text);
        }
    }
}