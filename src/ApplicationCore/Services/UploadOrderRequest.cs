using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class UploadOrderRequest: IUploadOrderRequest
{
  private readonly IConfiguration _configuration;
  private readonly IRepository<Basket> _basketRepository;
  public UploadOrderRequest(
        HttpClient httpClient,
        IConfiguration configuration,
        IRepository<Basket> basketRepository, IFunctionAppConfiguration functionAppConfiguration)
    {
        Guard.Against.Null(functionAppConfiguration.ApiBase, null);
        _configuration = configuration;
        _basketRepository = basketRepository;
    }

    public async Task Save(int basketId)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId); 
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

        Guard.Against.Null(basket, nameof(basket));
        Guard.Against.EmptyBasketOnCheckout(basket.Items);
        
        //lazy loading
        var items = basket.Items.Select(basketItem =>
            new
            {
                id = basketItem.BasketId,
                quantity = basketItem.Quantity
            }
        ).ToList();
        
        await using var client = new ServiceBusClient(_configuration.GetConnectionString("ServiceBusConnectionString"));
        await using var sender = client.CreateSender(_configuration.GetConnectionString("QueueName"));

        var itemsContent = new StringContent(JsonSerializer.Serialize(items), Encoding.UTF8, "application/json");
        try
        {
            string messageBody = $"$10,000 order for bicycle parts from retailer Adventure Works.";
            var message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
        }
        catch (Exception e)
        {
            await using var receiver = client.CreateReceiver(_configuration.GetConnectionString("QueueName"));
            var message = await receiver.ReceiveMessageAsync();
            await receiver.DeadLetterMessageAsync(message, $"reason: { message }", "unable to uploaded");
            Console.WriteLine(e.Message);
        }

    }
}
