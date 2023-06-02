using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeliveryOrderProcessorFunctionApp;

public static class Run
{
  [FunctionName(nameof(RunPost))]
  public static async Task<IActionResult> RunPost(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
    HttpRequest req,
    [CosmosDB(
      databaseName: "%DATABASE_NAME%",
      containerName: "%CONTAINER_NAME%",
      Connection = "CosmosDBConnection")] IAsyncCollector<dynamic> documentsOut,
    ILogger log)
  {
    log.LogInformation("HTTP trigger function processed new item uploaded to the cosmosDB. done");
        
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
        
    await documentsOut.AddAsync(data);
        
    return new OkObjectResult("OK");
  }
}
