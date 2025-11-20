using Application.Statistic.StatisticBalance;
using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using UnitTest;
using UnitTest.Stubs;
using Xunit;

namespace Application.Test.CommandTest;

public class SpendingUntilPaydayTest
{
    /// <summary>
    /// today is the first day of month and it's a holiday
    /// </summary>
    [Fact]
    public async Task Handle_ReturnsCorrectMoneyLeft_TodayIsTheFirstDayOfMonthAndHoliday()
    {
        // Arrange
        var today = new DateOnly(2025, 6, 1);

        var balance = new Money { Amount = 10_000, Currency = Currency.AMD };

        var dateTimeServiceStub = new DateTimeServiceStub(today.ToDateTime(new TimeOnly()));

        var loggerMock = new LoggerStub<SpendingUntilPaydayCommandHandler>();

        var financeRepositoryStub = new FinanceRepositoryStub();
        var moneyTransferBuilder = new MoneyTransferBuilder(true)
        {
            Date = today,
            Category = "Other".AsCategory(),
            Sum = balance
        };
        await financeRepositoryStub.SaveIncome(moneyTransferBuilder.Build(), CancellationToken.None);

        var handler = new SpendingUntilPaydayCommandHandler(
            dateTimeServiceStub,
            financeRepositoryStub,
            loggerMock
        );

        var request = new SpendingUntilPaydayCommand
        {
            Balance = balance,
            Currency = Currency.AMD
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(10_000, result.MoneyPerDay.Amount);
        Assert.Equal(Currency.AMD, result.MoneyPerDay.Currency);
        Assert.Equal(new DateOnly(2025, 6, 2), result.Payday);
    }
}