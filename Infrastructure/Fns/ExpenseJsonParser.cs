using Application;
using Domain;
using Newtonsoft.Json;

namespace Infrastructure.Fns;

public class ExpenseJsonParser : IExpenseJsonParser
{
    public IReadOnlyList<IMoneyTransfer> Parse(string text, Category category, Currency currency)
    {
        var result = new List<IMoneyTransfer>();
        dynamic json = JsonConvert.DeserializeObject(text)!;

        var date = DateOnly.FromDateTime(DateTime.Parse(json["dateTime"].ToString()));

        foreach (var jsonExpense in json["items"])
        {
            var amount = decimal.Parse(jsonExpense["sum"].ToString()) / 100;
            var description = jsonExpense["name"].ToString();

            var expense = new Outcome()
            {
                Amount = new Money { Amount = amount, Currency = currency },
                Description = description,
                Date = date,
                Category = category,
            };
            
            result.Add(expense);
        }

        return result;
    }
}