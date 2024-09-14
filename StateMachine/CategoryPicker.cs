using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CategoryPicker : IChainState
    {
        private readonly IEnumerable<Category> _categories;
        private readonly Action<Category> _fillCategory;
        private ILogger _logger;

        public CategoryPicker(Action<Category> fillCategory, IEnumerable<Category> categories, ILogger logger)
        {
            _fillCategory = fillCategory;
            _categories = categories;
            _logger = logger;
        }

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            string infoMessage = "Enter the category";

            var keyboard = TelegramKeyboard.FromButtons(
                _categories.Select(c => new TelegramButton() { Text = c.Name, CallbackData = c.Name })
            );

            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: infoMessage,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            var categoryDomain = _categories.FirstOrDefault(c => c.Name == message.Text);
            if (categoryDomain != null)
            {
                _fillCategory(categoryDomain);
                
                return ChainStatus.Success();
            }

            _logger.LogWarning($"The category with name {message.Text} has not been found");
            return ChainStatus.Retry(this);
        }
    }

    internal class CategorySubcategoryPicker : IChainState
    {
        private readonly List<Category> _categories;
        private readonly Action<Category> _fillCategory;
        private readonly Action<SubCategory> _fillSubcategory;
        private Category? _chosenCategory;
        private readonly ILogger _logger;

        public CategorySubcategoryPicker(Action<Category> fillCategory, Action<SubCategory> fillSubcategory, IEnumerable<Category> categories, ILogger logger)
        {
            _fillCategory = fillCategory;
            _fillSubcategory = fillSubcategory;
            _categories = categories.ToList();
            _logger = logger;
        }
        
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
        {
            bool isCategoryChosen = _chosenCategory != null;

            string infoMessage;
            TelegramKeyboard keyboard;
            
            if (!isCategoryChosen)
            {
                infoMessage = "Enter the category";
                keyboard = TelegramKeyboard.FromButtons(
                    _categories.Select(c => new TelegramButton() { Text = c.Name, CallbackData = c.Name })
                );
            }
            else
            {
                infoMessage = "Enter the subcategory";
                keyboard = TelegramKeyboard.FromButtons(
                    _chosenCategory!.Subcategories.Select(c => new TelegramButton() { Text = c.Name, CallbackData = c.Name })
                );
            }
            
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: infoMessage,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            if (_chosenCategory == null)
            {
                var categoryDomain = _categories.FirstOrDefault(c => c.Name == message.Text);
                if (categoryDomain != null)
                {
                    _chosenCategory = categoryDomain;
                    _fillCategory(categoryDomain);

                    if (!categoryDomain.Subcategories.Any())
                    {
                        return ChainStatus.Success();
                    }
                }
            }
            else
            {
                var subcategory = _chosenCategory.Subcategories.FirstOrDefault(c => c.Name == message.Text);
                if (subcategory != null)
                {
                    _fillSubcategory(subcategory);
                    return ChainStatus.Success();
                }
            }
            
            return ChainStatus.Retry(this);
        }
    }
}