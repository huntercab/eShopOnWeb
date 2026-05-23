
using DeliveryOrderProcessor.Models;
using Microsoft.Azure.Cosmos;

namespace DeliveryOrderProcessor.Respository;

internal class CosmosOrderRepository: IOrderRepository
{
    private readonly Container _container;

    public CosmosOrderRepository(CosmosClient cosmosClient)
    {
        var db = cosmosClient
            .CreateDatabaseIfNotExistsAsync("OrdersDb")
            .GetAwaiter()
            .GetResult();

        var container = db.Database
            .CreateContainerIfNotExistsAsync(new ContainerProperties("OrderDetails", "/id"))
            .GetAwaiter()
            .GetResult();

        _container = container.Container;
    }

    public async Task CreateAsync(OrderDetailDocument order)
    {
        await _container.CreateItemAsync(order, new PartitionKey(order.id));
    }
}
