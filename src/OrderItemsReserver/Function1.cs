using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OrderItemsReserver.Interfaces;
using OrderItemsReserver.Entities;

namespace OrderItemsReserver;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOrderRequestBlobUploader _requestBlobUploader;

    public Function1(ILogger<Function1> logger, IConfiguration configuration, IOrderRequestBlobUploader requestBlobUploader)
    {
        _logger = logger;
        _configuration = configuration;
        _requestBlobUploader = requestBlobUploader;
    }

    [Function("OrderItemsReserver")]
    public async Task<IActionResult> RunAsync([ServiceBusTrigger("sb-orderrequest", Connection = "ServiceBusConnectionString")]
        ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = message.Body.ToString();
            OrderItemRequest? request = null;

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("The body request is empty");
            }

            var order = JsonSerializer.Deserialize<OrderRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (order == null)
            {
                return new BadRequestObjectResult("Invalid or Empty Json");
            }

            await _requestBlobUploader.UploadAsync(order, cancellationToken);
            
            return new OkObjectResult($"Order {order.OrderId} Stored in blob storage container");
        }
        catch (Exception ex) {
            //return new BadRequestObjectResult($"There was an issue storing the order: {ex.Message}");
            _logger.LogError(ex, $"There was an issue storing the order. DeliveryCount: {message.DeliveryCount} MessageId: {message.MessageId}, Body: {message.Body}");
            throw;//important to be moved to dead letter queue after the max delivery attempts is reached
        }
        
    }
}
