using Application.Api;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelegramBot.Contracts;
using TelegramBot.Mappers;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionController(IMediator mediator, ILogger<TransactionController> logger) : ControllerBase
{
    [HttpPost("save")]
    public async Task<SaveResult> SaveTransaction([FromBody] MoneyTransferDTO moneyTransferDto, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SaveMoneyTransferApiCommand()
        {
            IsOutcome = moneyTransferDto.IsOutcome,
            Date = moneyTransferDto.Date,
            Category = moneyTransferDto.Category,
            SubCategory = moneyTransferDto.SubCategory,
            Shop = moneyTransferDto.Shop,
            Description = moneyTransferDto.Description,
            Amount = moneyTransferDto.Amount, 
            Currency = moneyTransferDto.Currency,
        }, cancellationToken);
        
        return result;
    }
    
    [HttpPost("yerevancity/save")]
    public async Task<SaveCheckDto> SaveTransaction([FromBody] YerevanCityCheckDTO yerevanCityCheckDto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new SaveYerevanCityReceiptCommand()
            {
                Date = yerevanCityCheckDto.Date,
                Barcode = yerevanCityCheckDto.Barcode
            }, cancellationToken);
        
            return SaveCheckMapper.ToDto(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error saving Yerevan City receipt");
            return new SaveCheckDto()
            {
                Success = false,
                Error = e.Message,
            };
        }
    }

    

    [HttpPost("fns/url/save")]
    public async Task<SaveCheckDto> SaveTransaction([FromBody] FnsURLDTO fnsUrldto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new SaveFnsCheckCommand() { Url = fnsUrldto.Url }, cancellationToken);
        
            return SaveCheckMapper.ToDto(result);
        }
        catch (Exception e)
        {
            return new SaveCheckDto()
            {
                Success = false,
                Error = e.Message,
            };
        }
    }
    
    [HttpPost("fns/requisites/save")]
    public async Task<SaveCheckDto> SaveTransaction([FromBody] FnsRequisiteDTO fnsRequisiteDto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new SaveFnsCheckCommand()
            {
                DateTime = fnsRequisiteDto.DateTime,
                FiscalDocumentNumber = fnsRequisiteDto.FiscalDocumentNumber,
                FiscalDocumentSign = fnsRequisiteDto.FiscalDocumentSign,
                FiscalNumber = fnsRequisiteDto.FiscalNumber,
                TotalPrice = fnsRequisiteDto.TotalPrice
            }, cancellationToken);

            return SaveCheckMapper.ToDto(result);
        }
        catch (Exception e)
        {
            return new SaveCheckDto()
            {
                Success = false,
                Error = e.Message,
            };
        }
    }
}