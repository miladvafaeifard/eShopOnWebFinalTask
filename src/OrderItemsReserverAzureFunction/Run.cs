using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OrderItemsReserverAzureFunction;

public static class Run
{
    [FunctionName(nameof(RunPost))]
    public static async Task RunPost(
      [ServiceBusTrigger("orders", Connection = "ServiceBusConnectionString")] 
      string myQueueItem,
      Int32 deliveryCount,
      DateTime enqueuedTimeUtc,
      string messageId,
      ILogger log)
    {
      log.LogInformation("HTTP trigger function processed new item uploaded to the blob. done");
    }
}
