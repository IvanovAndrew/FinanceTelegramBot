using Domain;

namespace Application
{
    public class MoneyTransferBuilder
    {
        private bool _isIncome;
        public MoneyTransferBuilder(bool isIncome)
        {
            _isIncome = isIncome;
        }

        public bool IsIncome => _isIncome;
        public DateOnly? Date { get; set; }
        public Category? Category { get; set; }
        public SubCategory? SubCategory { get; set; }

        public string? Description { get; set; }
        public Money? Sum { get; set; }

        public IMoneyTransfer Build()
        {
            if (_isIncome)
            {
                return new Income()
                {
                    Date = Date!.Value,
                    Category = Category,
                    Description = Description,
                    Amount = Sum!,
                };
            }
            
            return new Outcome
            {
                Date = Date!.Value,
                Category = Category,
                SubCategory = SubCategory,
                Description = Description,
                Amount = Sum!,
            };
        }
    }
}