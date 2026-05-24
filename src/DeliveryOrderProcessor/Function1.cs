using System.Text.Json;
using DeliveryOrderProcessor.Respository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using DeliveryOrderProcessor.Models;
using Microsoft.AspNetCore.Http.Features;

namespace DeliveryOrderProcessor;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly IOrderRepository _orderRepository;

    public Function1(ILogger<Function1> logger, IOrderRepository orderRepository)
    {
        _logger = logger;
        _orderRepository = orderRepository;
    }

    [Function("CreateOrder")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        try
        {
            var request = await JsonSerializer.DeserializeAsync<FuncOrder>(
                req.Body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }
                );

            if (request is null)
            {
                return new NotFoundObjectResult("Request Body Required");
            }

            if (!request.IsShipToAddressValid())
            {
                return new NotFoundObjectResult("ShippingAddress is required");
            }

            if (request.OrderItems is null || request.OrderItems.Count == 0)
            {
                return new NotFoundObjectResult("At least one Ordered Item is required");
            }

            var document = new OrderDetailDocument
            {
                id = Guid.NewGuid().ToString(),
                ShippingAddress = request.GetCompleteShipToAddress(),
                Items = request.OrderItems.Select(x => new OrderItemDetailDocument
                {
                    ItemOrdered = x.ItemOrdered,
                    UnitPrice = x.UnitPrice,
                    Units = x.Units
                }).ToList(),
                OrderTotal = request.Total()
            };

            await _orderRepository.CreateAsync( document );

            return new OkObjectResult($"Order created with id={document.id} and Total={document.OrderTotal}");
        }
        catch (Exception ex) 
        {
            return new BadRequestObjectResult(ex.Message);
        }
        
    }
}
