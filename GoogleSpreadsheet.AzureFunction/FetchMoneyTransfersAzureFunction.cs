using System;
using GoogleSheetWriter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoogleSpreadsheet;

// public class FetchMoneyTransfersAzureFunction(Fetcher fetcher, ILoggerFactory loggerFactory)
// {
//     private readonly ILogger _logger = loggerFactory.CreateLogger<FetchMoneyTransfersAzureFunction>();
//
//     //[Function(nameof(FetchMonthMoneyTransfers))]
//     public async Task FetchMonthMoneyTransfers([TimerTrigger("0 0 0 1 * *")] TimerInfo myTimer)
//     {
//         var now = DateTime.Now;
//         _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
//
//         var searchOptions = new MoneyTransferSearchOption();
//         if (now.Day == 1)
//         {
//             searchOptions.DateFrom = now.AddMonths(-1);
//             searchOptions.DateTo = now.AddDays(-1);
//         }
//         else
//         {
//             var startRange = new DateTime(now.Year, now.Month, 1);
//             searchOptions.DateFrom = startRange;
//             searchOptions.DateTo = startRange.AddMonths(1).AddDays(-1);
//         }
//
//         await fetcher.Fetch(searchOptions, new CancellationToken());
//         
//         if (myTimer.ScheduleStatus is not null)
//         {
//             _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
//             
//         }
//     }
//
//     [Function(nameof(FetchWeekMoneyTransfers))]
//     public async Task FetchWeekMoneyTransfers([TimerTrigger("0 0 0 */7 * *")] TimerInfo myTimer)
//     {
//         var now = DateTime.Now;
//         _logger.LogInformation($"C# Timer trigger function executed at: {now}");
//         
//         var searchOptions = new MoneyTransferSearchOption
//         {
//             DateFrom = now.AddDays(-7),
//             DateTo = now
//         };
//
//         await fetcher.Fetch(searchOptions, new CancellationToken());
//
//         if (myTimer.ScheduleStatus is not null)
//         {
//             _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
//         }
//     }
//     
//     [Function(nameof(FetchDayMoneyTransfers))]
//     public void FetchDayMoneyTransfers([TimerTrigger("0 0 0 */7 * *")] TimerInfo myTimer)
//     {
//         _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
//
//         if (myTimer.ScheduleStatus is not null)
//         {
//             _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
//         }
//     }
// }
//
