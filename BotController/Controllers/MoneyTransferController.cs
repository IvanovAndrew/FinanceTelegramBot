using Application.Api;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelegramBot.Contracts;
using TelegramBot.Mappers;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api/moneytransfer")]
public class MoneyTransferController(IMediator mediator, ILogger<MoneyTransferController> logger)
    : ControllerBase
{
    [HttpGet("outcomes")]
    public async Task<Dictionary<DateOnly, List<ShopExpensesDto>>> GetOutcomes([FromQuery] string currency, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Getting outcomes for {currency} for period from {startDate} to {endDate}");

        var data = await mediator.Send(new GetOutcomeApiCommand
            {
                StartDate = startDate,
                EndDate = endDate,
                Currency = Currency.Parse(currency)
            }, cancellationToken);
            
        return data.ToDictionary(
            x => x.Key, 
            x => x.Value.Select(ShopExpensesMapper.ToDto).ToList()
        );
    }
    
    [HttpGet("incomes")]
    public async Task<IEnumerable<MoneyTransferDTO>> GetIncomes([FromQuery] string currency, [FromQuery] DateOnly day, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Getting incomes for {currency} on {day}");
        
        var data = await mediator.Send(new GetIncomeApiCommand
        {
            Day = day,
            Currency = Currency.Parse(currency)
        }, cancellationToken);
        
        return data.Select(MoneyTransferMapper.ToDto);
    }
}