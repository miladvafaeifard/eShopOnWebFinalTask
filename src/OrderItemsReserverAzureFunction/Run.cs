using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OrderItemsReserverAzureFunction;

public static class Run
{
    [FunctionName(nameof(RunPost))]
    public static async Task<IActionResult> RunPost(
        [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)] HttpRequest req,
        [Blob("{BLOB_NAME}/{sys.utcnow}.json", FileAccess.Write)] Stream outputStreamBlob,
        ILogger log)
    {
        log.LogInformation("HTTP trigger function processed new item uploaded to the blob. done");
        
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        await using (StreamWriter writer = new StreamWriter(outputStreamBlob))
        {
            await writer.WriteAsync(requestBody);
        }

        return new OkObjectResult("OK");
    }
}
