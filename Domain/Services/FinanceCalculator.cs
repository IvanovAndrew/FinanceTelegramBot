namespace Domain.Services;

public static class FinanceCalculator
{
    public static Money Sum(IEnumerable<IMoneyTransfer> transfers, Currency currency, DateOnly startDate, DateOnly? endDate = null)
    {
        Money sum = new Money() { Amount = 0, Currency = currency };

        foreach (var transfer in transfers)
        {
            if (transfer.Amount.Currency != currency)
                continue;
            
            if (transfer.Date < startDate)
                continue;
            
            if (endDate != null && transfer.Date > endDate)
                continue;
            
            sum += transfer.Amount;
        }

        return sum;
    }
}