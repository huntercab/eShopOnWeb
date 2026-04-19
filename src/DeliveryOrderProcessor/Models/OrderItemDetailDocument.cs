
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace DeliveryOrderProcessor.Models;

public class OrderItemDetailDocument
{
    public CatalogItemOrdered ItemOrdered { get; set; }
    public int Units { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
