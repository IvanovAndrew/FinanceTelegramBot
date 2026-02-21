using Domain;

namespace Application
{
    public class MoneyTransferDraft
    {
        private bool _isIncome;
        private DateOnly? _date;
        private Money? _amount;
        
        public MoneyTransferDraft(bool isIncome)
        {
            _isIncome = isIncome;
        }

        public bool IsIncome => _isIncome;
        public DateOnly? Date => _date;
        public Category? Category { get; private set; }
        public SubCategory? SubCategory { get; private set; }

        public string? Description { get; private set; }
        public Money? Sum => _amount;
        
        public void SetDate(DateOnly date) => _date = date;
        public void SetAmount(Money amount) => _amount = amount;
        public void SetCategory(Category category) => Category = category;
        public void SetDescription(string description) => Description = description;
        public void SetSubCategory(SubCategory subCategory) => SubCategory = subCategory;
        
        public bool IsComplete { get; internal set; }
        public bool CustomDayMode { get; set; }

        public IMoneyTransfer ToEntity()
        {
            // if (!IsComplete)
            //     // TODO replace with custom exception
            //     throw new InvalidOperationException("Draft is incomplete");

            return _isIncome
                ? new Income() { Date = Date!.Value, Category = Category, Description = Description, Amount = Sum!, }
                : new Outcome()
                {
                    Date = Date!.Value, Category = Category, SubCategory = SubCategory, Description = Description,
                    Amount = Sum!
                };
        }
    }
}