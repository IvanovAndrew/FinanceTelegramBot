using Application.Core;

namespace Application.Test.Stubs;

public class ExpenseJsonParserStub : IExpenseJsonParser
{
    internal Dictionary<string, Check> JsonToCheck = new();
    
    public bool CanParse(string json) => JsonToCheck.ContainsKey(json);
    

    public Check ParseCheck(string json)
    {
        return JsonToCheck[json];
    }
}