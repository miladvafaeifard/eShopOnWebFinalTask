using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OrderItemsReserverAzureFunction;

public static class Run
{
    [FunctionName(nameof(RunPost))]
    public static async Task RunPost(
      [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnectionString")] 
      string orderItem,
      Int32 deliveryCount,
      DateTime enqueuedTimeUtc,
      string messageId,
      ILogger log)
    {
      var maxRetryCount = 3;
      if (deliveryCount > 2)
      {
        log.LogInformation("Dead-letter message received.");
      }
      else
      {
        var attempts = 0;
        var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("BLOB_NAME"));
        while (attempts < maxRetryCount)
        {
          try
          {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(orderItem);
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            BlobClient blobClient = containerClient.GetBlobClient($"{unixTime}.json");
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
              await blobClient.UploadAsync(stream);
            }
            break;
          }
          catch (Exception exception)
          {
            attempts++;
            if (attempts == 3)
            {
              var httpClient = new HttpClient();
              var request = new HttpRequestMessage {
                RequestUri = new Uri(Environment.GetEnvironmentVariable("LogicAppUrl")!),
                Method = HttpMethod.Post,
                Content = JsonContent.Create(new { messageId, order = orderItem })
              };
              var responseMessage = await httpClient.SendAsync(request);
            }
            else
            {
              log.LogInformation($"Failed upload ${exception.Message}" );
            }
          }

        }
      }
      
      

      
    }
}
