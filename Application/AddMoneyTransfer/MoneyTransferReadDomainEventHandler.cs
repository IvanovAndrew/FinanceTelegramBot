using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

// public class MoneyTransferReadDomainEventHandler(IUserSessionService userSessionService, IMessageService messageService, ILogger<MoneyTransferReadDomainEventHandler> logger) : INotificationHandler<MoneyTransferReadDomainEvent>
// {
//     public async Task Handle(MoneyTransferReadDomainEvent notification, CancellationToken cancellationToken)
//     {
//         var session = userSessionService.GetUserSession(notification.SessionId);
//
//         if (session != null)
//         {
//             var statistic = notification.Statistic;
//             
//             var table = StatisticTableBuilder.BuildTable(notification, statistic);
//
//             await messageService.EditSentTextMessageAsync(
//                 new Message()
//                 {
//                     ChatId = session.Id,
//                     Id = session.LastSentMessageId,
//                     Table = table
//                 }, cancellationToken
//                 );
//         }
//     }
//
//     
// }

public record TableOptions : INotification
{
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }
    public string? Subtitle { get; init; }
    public string? FirstColumnName { get; init; }
}

internal static class StatisticTableBuilder
{
    internal static Table BuildTable(StatisticWrapper statistic, TableOptions tableOptions)
    {
        var table = new Table()
        {
            Title = "Statistic",
            Subtitle = tableOptions.Subtitle,
            FirstColumnName = tableOptions.FirstColumnName,
            Currencies = statistic.Currencies
        };
           
        int i = 0;

        foreach (var expenseInfo in statistic.Rows)
        {
            var currencyValues = new Dictionary<Currency, Money>();
            foreach (var currency in statistic.Currencies)
            {
                currencyValues[currency] = expenseInfo[currency];
            }
                
            table.AddRow(new Row(){FirstColumnValue = expenseInfo.FirstColumn.GetString(), CurrencyValues = currencyValues});
        }

        table.AddRow(new Row());
            
        var totalValues = new Dictionary<Currency, Money>();
        foreach (var currency in statistic.Currencies)
        {
            totalValues[currency] = statistic.Total[currency];
        }
        table.AddRow(new Row {FirstColumnValue = "Total", CurrencyValues = totalValues});
        return table;
    }
}