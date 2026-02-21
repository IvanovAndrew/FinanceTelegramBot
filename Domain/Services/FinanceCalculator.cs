namespace Domain.Services;

public static class FinanceCalculator
{
    public static Money Sum(IEnumerable<IMoneyTransfer> transfers, Currency currency, MonthRange range)
    {
        Money sum = new Money() { Amount = 0, Currency = currency?? transfers.First().Amount.Currency };

        foreach (var transfer in transfers)
        {
            if (transfer.Amount.Currency != currency)
                continue;
            
            if (!range.IsInRange(transfer.Date))
                continue;
            
            sum += transfer.Amount;
        }

        return sum;
    }
}