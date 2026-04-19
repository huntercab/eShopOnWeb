using DeliveryOrderProcessor.Models;
using Microsoft.Azure.Cosmos;

namespace DeliveryOrderProcessor.Respository;

public interface IOrderRepository
{
    Task CreateAsync(OrderDetailDocument Order);
}
