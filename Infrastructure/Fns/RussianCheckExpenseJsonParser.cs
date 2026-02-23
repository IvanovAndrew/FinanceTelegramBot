using Application;
using Domain;
using Newtonsoft.Json;

namespace Infrastructure.Fns;

public class RussianCheckExpenseJsonParser : IExpenseJsonParser
{
    public Currency Currency => Currency.RUR;

    public bool CanParse(string json)
    {
        return json.Contains("fnsurl", StringComparison.InvariantCultureIgnoreCase);
    }

    public IReadOnlyList<Outcome> ParseOutcomes(string json, Category defaultCategory)
    {
        var result = new List<Outcome>();
        dynamic order = JsonConvert.DeserializeObject(json)!;

        var date = DateOnly.FromDateTime(DateTime.Parse(order["dateTime"].ToString()));

        foreach (var jsonExpense in order["items"])
        {
            var amount = decimal.Parse(jsonExpense["sum"].ToString()) / 100;
            var description = jsonExpense["name"].ToString();

            var expense = new Outcome()
            {
                Amount = new Money { Amount = amount, Currency = Currency },
                Description = description,
                Date = date,
                Category = defaultCategory,
            };
            
            result.Add(expense);
        }

        return result;
    }
}