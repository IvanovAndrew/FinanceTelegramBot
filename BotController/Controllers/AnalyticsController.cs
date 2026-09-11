using Application.Api;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelegramBot.Contracts;
using TelegramBot.Mappers;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public AnalyticsController(IMediator mediator, ILogger<AnalyticsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpGet("summary")]
    public async Task<BalanceDTO> GetSummary([FromQuery] string currency, [FromQuery] DateOnly start, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching summary for {Currency} starting from {Start}", currency, start);
        
        var summary = await _mediator.Send(new GetBalanceStatisticApiCommand()
        {
            Currency = Currency.Parse(currency),
            StartDate = YearMonth.From(start)
        }, cancellationToken);

        return new BalanceDTO()
        {
            Currency = summary.Currency.Name,
            DailyBudgetLimit = summary.DailyBudget.Amount,
            DaysUntilPayday = summary.PeriodLeft.DaysRemaining,
            FutureExpenses = summary.FutureExpenses.Select(FutureExpenseMapper.ToDto).ToList(),
            FutureExpensesTotal = summary.FutureExpensesSum.Amount,
            Payday = summary.PeriodLeft.End.ToDateTime(new TimeOnly(0, 0)),
            StartPeriod = (summary.PeriodLeft.IncludeStart? summary.PeriodLeft.Start : summary.PeriodLeft.Start.AddDays(1)).ToDateTime(new TimeOnly(0, 0)),
            
            TotalIncome = summary.MonthBalances.Sum(x => x.Balance.Income.Amount),
            TotalOutcome = summary.MonthBalances.Sum(x => x.Balance.Outcome.Amount),
            TotalBalance = summary.MonthBalances.Sum(x => x.Balance.Saldo.Amount),
            
            RealFreeMoney = summary.Saldo.Amount - summary.FutureExpensesSum.Amount,
        };
    }
    
    [HttpGet("history/monthly")]
    public async Task<SpendingHistoryDTO> GetMonthlyAnalytics([FromQuery] string currency, [FromQuery] DateOnly start, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching spending history from {Start} in {Currency}", start, currency);

        var result = await _mediator.Send(new GetSpendingHistoryApiCommand
        {
            StartMonth = YearMonth.From(start),
            Currency = Currency.Parse(currency)
        }, cancellationToken);

        return new SpendingHistoryDTO
        {
            Currency = result.Currency.Name,
            Months = result.Months.Select(m => new MonthSpendingDTO
            {
                Month = m.Month.ToDateOnly(1),
                Total = m.Total.Amount,
                TotalOutcome = m.OutcomeTotal.Amount,
                RealOutcomeTotal = m.RealOutcomeTotal.Amount,
                TotalIncome = m.IncomeTotal.Amount,
                OutcomeCategories = (m.OutcomeCategories ?? []).Select(c => new CategorySpendingDTO
                {
                    Category = c.Category?.Code ?? Categories.Outcome.Other.Code,
                    Total = c.Total.Amount,
                    SubCategories = (c.SubCategories ?? []).Select(s => new SubCategorySpendingDTO
                    {
                        SubCategory = s.SubCategory?.Code ?? string.Empty,
                        Total = s.Total.Amount
                    }).ToList()
                }).ToList(),
                IncomeCategories = (m.IncomeCategories ?? []).Select(c => new CategorySpendingDTO
                {
                    Category = c.Category?.Code ?? Categories.Income.Others.Code,
                    Total = c.Total.Amount,
                    SubCategories = (c.SubCategories ?? []).Select(s => new SubCategorySpendingDTO
                    {
                        SubCategory = s.SubCategory?.Code ?? string.Empty,
                        Total = s.Total.Amount
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }
    
    [HttpGet("history/daily")]
    public async Task<SpendingDayHistoryDTO> GetDailyAnalytics([FromQuery] string currency, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching spending history from {Start} to {End} in {Currency}", startDate, endDate, currency);

        var result = await _mediator.Send(new GetSpendingDailyHistoryApiCommand
        {
            From = startDate,
            To = endDate,
            Currency = Currency.Parse(currency)
        }, cancellationToken);

        return new SpendingDayHistoryDTO
        {
            Currency = result.Currency.Name,
            Days = result.Days.Select(d => new DaySpendingDTO
            {
                Day = d.Day,
                Total = d.Total.Amount,
                Shops = d.ShopChecks.Select(c => new ShopSpendingDTO()
                {
                    Name = c.Shop?.Name,
                    Categories = c.Categories.Select(x => new CategorySpendingDTO()
                        {
                            Category = x.Category.Code,
                            Total = x.Total.Amount,
                            SubCategories = x.SubCategories.Select(s => new SubCategorySpendingDTO
                            {
                                SubCategory = s.SubCategory?.Code,
                                Total = s.Total.Amount
                            }).ToList(),
                        }).ToList()
                }).ToList()
            }).ToList()
        };
    }
    
    [HttpGet("daily")]
    public async Task<List<MoneyTransferDTO>> GetDayExpenses([FromQuery] string currency, [FromQuery] DateOnly start, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching daily expenses for {Date} in {Currency}", start, currency);
        
        var outcomes = await _mediator.Send(new GetDayOutcomesRequestCommand(){
            Date = start,
            Currency = currency
        }, cancellationToken);
        
        return outcomes.Select(o => new MoneyTransferDTO()
        {
            IsOutcome = true,
            Date = o.Date,
            Category = o.Category.Code,
            SubCategory = o.SubCategory?.Code,
            Shop = o.Shop?.Name,
            Description = o.Description,
            Amount = o.Amount.Amount,
            Currency = o.Amount.Currency.Name
        }).ToList();
    }
}