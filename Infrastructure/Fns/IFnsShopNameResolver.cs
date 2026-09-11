using Application.Contracts.FNS;
using Domain;

namespace Infrastructure.Fns;

public interface IFnsShopNameResolver
{
    Shop? Resolve(FnsCheckInfo checkInfo);
}

public class FnsShopNameResolver : IFnsShopNameResolver
{
    private static Dictionary<string, string> _shopNameReplacements = new Dictionary<string, string>
    {
        { "ООО", string.Empty },
        { "ОБЩЕСТВО С ОГРАНИЧЕННОЙ ОТВЕТСТВЕННОСТЬЮ", string.Empty },
        { "Общество с ограниченной ответственностью", string.Empty },
        { "АКЦИОНЕРНОЕ ОБЩЕСТВО", string.Empty },
        { "\"ТОРГОВЫЙ ДОМ \"", string.Empty },
        {"Агроторг", "Пятёрочка"},
        {"БЭСТ ПРАЙС", "Fix Price"},
        {"Интернет Решения", "Озон"},
        {"ИНТЕРНЕТ РЕШЕНИЯ", "Озон"},
        {"РВБ", "Wildberries"},
        {"ТК Прогресс", "Семишагофф"},
        {"ПЕРЕКРЕСТОК", "Перекрёсток"},
        {"Калуга Лариса Владимировна", "handywatercolor"},
        {"Садовникова Лариса Михайловна", "Paris Nail"},
        {"Дорин Григорий Григорьевич", "Магазин художественных товаров"},
    };

    public Shop? Resolve(FnsCheckInfo checkInfo)
    {
        string mainString = checkInfo.User;

        foreach (KeyValuePair<string, string> nameReplacement in _shopNameReplacements)
        {
            if (checkInfo.RetailPlace.Contains(nameReplacement.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                mainString = checkInfo.RetailPlace;
                break;
            }
        }        

        return Shop.Create(CleanName(mainString));
    }

    private string? CleanName(string s)
    {
        if (string.IsNullOrEmpty(s)) return null;

        string shopName = s;
        foreach (var (key, value) in _shopNameReplacements)
        {
            shopName = shopName.Replace(key, value).Trim().Trim('\"');
        }

        return shopName.Trim().Trim('\"');
    }
}