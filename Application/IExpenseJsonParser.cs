using Domain;

namespace Application;

public interface IExpenseJsonParser
{
    IReadOnlyList<IMoneyTransfer> Parse(string text, Category category, Currency currency);
}