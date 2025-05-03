using Domain;

namespace Application;

public interface IExpenseJsonParser
{
    IReadOnlyList<IMoneyTransfer> Parse(string text, string category, Currency currency);
}