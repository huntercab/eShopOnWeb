using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace OrderItemsReserver;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly IConfiguration _configuration;

    public Function1(ILogger<Function1> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function("OrderItemsReserver")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("The body request is empty");
            }

            var order = JsonSerializer.Deserialize<PurchaseOrderRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            if (order == null)
            {
                return new BadRequestObjectResult("Invalid or Empty Json");
            }

            string connectionString = _configuration["Values:AzureWebJobsStorage"] ?? throw new InvalidOperationException("AzureWebJobsStorage not defined");
            string containerName = _configuration["Values.BlobContainerName"] ?? "orders";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();

            var filename = $"order-{order.ItemId}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
            var blobClient = containerClient.GetBlobClient(filename);

            var jsonToStore = JsonSerializer.Serialize(order, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonToStore));

            await blobClient.UploadAsync(stream, overwrite: true);

            return new OkObjectResult($"Order Stored in blob storage container {containerName}");
        }
        catch (Exception ex) {
            return new BadRequestObjectResult($"There was an issue storing the order: {ex.Message}");
        }
        
    }
}