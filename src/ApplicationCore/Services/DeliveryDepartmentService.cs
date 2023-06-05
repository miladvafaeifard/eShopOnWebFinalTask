using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class DeliveryDepartmentService: IDeliveryDepartmentService
{ 
    private readonly HttpClient _httpClient;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IFunctionAppConfiguration _functionAppConfiguration;

    public DeliveryDepartmentService(HttpClient httpClient, IRepository<Basket> basketRepository, IFunctionAppConfiguration functionAppConfiguration)
    {
        _httpClient = httpClient;
        _basketRepository = basketRepository;
        _functionAppConfiguration = functionAppConfiguration;
    }
    
    public async Task CreateDeliveryOrderAsync(int basketId, Address shippingAddress)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId); 
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

        Guard.Against.Null(basket, nameof(basket));
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var finalPrice = Math.Round(basket.Items.Sum(item => item.UnitPrice * item.Quantity), 2);
        
        var items = basket.Items.Select(item => 
            new
            {
                id = item.Id,
                quantity = item.Quantity,
                unitPrice = item.UnitPrice,
                basketId = item.BasketId
            }
            ).ToList();
        var orderDelivery = new { id = System.Guid.NewGuid().ToString(), customerId = basket.BuyerId, finalPrice , shippingAddress, items };

        var orderDeliveryContent = new StringContent(JsonSerializer.Serialize(orderDelivery), Encoding.UTF8, "application/json");
        try
        {
            var request = new HttpRequestMessage {
                RequestUri = new Uri($"{_functionAppConfiguration.ApiBase}/DeliveryOrderProcessor"),
                Method = HttpMethod.Post,
                Headers = {
                    { "x-functions-key", _functionAppConfiguration.Code }
                },
                Content = orderDeliveryContent
            };
            var responseMessage = await _httpClient.SendAsync(request);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

    }
}
