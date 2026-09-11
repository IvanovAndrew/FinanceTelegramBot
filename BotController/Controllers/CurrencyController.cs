using Microsoft.AspNetCore.Mvc;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api")]
public class CurrencyController : ControllerBase
{
    [HttpGet("currencies")]
    public List<Contracts.CurrencyDTO> GetCurrencies()
    {
        return
        [
            .. Domain.Currency.GetAvailableCurrencies().Select(c => new Contracts.CurrencyDTO()
            {
                Name = c.Name,
                Symbol = c.Symbol,
                Format = c.Format,
                IsPopular = c.IsPopular
            })
        ];
    }
}