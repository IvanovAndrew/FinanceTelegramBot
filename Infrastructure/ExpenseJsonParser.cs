using Domain;
using Newtonsoft.Json;

namespace Infrastructure;

public class ExpenseJsonParser
{
    public List<IExpense> Parse(string text, string category, Currency currency)
    {
        var result = new List<IExpense>();
        dynamic json = JsonConvert.DeserializeObject(text)!;

        var date = DateOnly.FromDateTime(DateTime.Parse(json["dateTime"].ToString()));

        foreach (var jsonExpense in json["items"])
        {
            var amount = decimal.Parse(jsonExpense["sum"].ToString()) / 100;
            var description = jsonExpense["name"].ToString();

            var expense = new Expense()
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