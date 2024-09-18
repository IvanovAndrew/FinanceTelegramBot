using Domain;

namespace Infrastructure
{
    public class MoneyTransferBuilder
    {
        private bool _isIncome;
        public MoneyTransferBuilder(bool isIncome)
        {
            _isIncome = isIncome;
        }
        
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
                    Category = Category!.Name,
                    Description = Description,
                    Amount = Sum!,
                };
            }
            
            return new Outcome
            {
                Date = Date!.Value,
                Category = Category!.Name,
                SubCategory = SubCategory?.Name,
                Description = Description,
                Amount = Sum!,
            };
        }
    }
}