using Domain;

namespace Infrastructure
{
    public class ExpenseBuilder
    {
        public DateOnly? Date { get; set; }
        public Category? Category { get; set; }
        public SubCategory? SubCategory { get; set; }

        public string? Description { get; set; }
        public Money? Sum { get; set; }

        public IExpense Build()
        {
            return new Expense
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