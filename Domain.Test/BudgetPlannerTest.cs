namespace Domain.Test;

public class BudgetPlannerTest
{
    [Fact]
    public void Plan_WithPositiveBalanceAndNoCompulsoryOutcomes_ReturnsEqualDailyAmount()
    {
        var balance = new Money { Amount = 300m, Currency = Currency.Rur };
        var result = BudgetPlanner.Plan(balance, 30, []);
        
        Assert.Equal(new Money { Amount = 10m, Currency = Currency.RUR }, result);
    }

    [Fact]
    public void Plan_WithCompulsoryOutcomes_SubtractsBeforeDivision()
    {
        var balance = new Money { Amount = 300m, Currency = Currency.RUR };
        var compulsory = new List<Money>
        {
            new Money { Amount = 60m, Currency = Currency.RUR }
        };

        var result = BudgetPlanner.Plan(balance, 30, compulsory);

        Assert.Equal(new Money { Amount = 8m, Currency = Currency.RUR }, result); // (300 - 60) / 30
    }

    [Fact]
    public void Plan_WithCompulsoryGreaterThanBalance_ReturnsZero()
    {
        var balance = new Money { Amount = 50m, Currency = Currency.RUR };
        var compulsory = new List<Money>
        {
            new Money { Amount = 100m, Currency = Currency.RUR }
        };

        var result = BudgetPlanner.Plan(balance, 30, compulsory);

        Assert.Equal(Money.Zero(Currency.RUR), result);
    }

    [Fact]
    public void Plan_WithDifferentCurrency_ThrowsException()
    {
        var balance = new Money { Amount = 100m, Currency = Currency.RUR };
        var compulsory = new List<Money>
        {
            new Money { Amount = 10m, Currency = Currency.USD }
        };

        Assert.Throws<MoneyAdditionException>(() => BudgetPlanner.Plan(balance, 30, compulsory));
    }
}