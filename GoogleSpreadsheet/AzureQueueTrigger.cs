using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoogleSpreadsheet;

public class AzureQueueTrigger
{
    private readonly ILogger<AzureQueueTrigger> _logger;

    public AzureQueueTrigger(ILogger<AzureQueueTrigger> logger)
    {
        _logger = logger;
    }

    [Function(nameof(AzureQueueTrigger))]
    public void Run([QueueTrigger("save-expense")] QueueMessage message)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
    }
}