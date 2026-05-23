//using Microsoft.eShopWeb.ApplicationCore.Entities.Shared;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using OrderItemsReserver.Entities;
using Microsoft.Extensions.Configuration;
using OrderItemsReserver.Interfaces;

namespace OrderItemsReserver.Services;

public class OrderRequestBlobUploader : IOrderRequestBlobUploader
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public OrderRequestBlobUploader(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration["BlobContainerName"] ?? "orders";
    }
    public async Task UploadAsync(OrderRequest order, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var filename = $"order-{order.OrderId}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        var blobClient = containerClient.GetBlobClient(filename);

        var jsonToStore = JsonSerializer.Serialize(order, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonToStore));

        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken); 
    }
}
