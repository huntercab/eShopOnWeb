using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace DeliveryOrderProcessor.Models;

internal class FuncOrder
{
    public int Id { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public Address ShipToAddress { get; set; }
    public List<OrderItem> OrderItems { get; set; }

    public decimal Total()
    {
        var total = 0m;
        foreach (var item in OrderItems)
        {
            total += item.UnitPrice * item.Units;
        }
        return total;
    }

    public bool IsShipToAddressValid()
    {
        return !string.IsNullOrWhiteSpace(ShipToAddress.Country) &&
                !string.IsNullOrWhiteSpace(ShipToAddress.City) &&
                !string.IsNullOrWhiteSpace(ShipToAddress.State) &&
                !string.IsNullOrWhiteSpace(ShipToAddress.Street);
    }

    public string GetCompleteShipToAddress()
    {
        return string.Join(", ", ShipToAddress.Country, ShipToAddress.State, ShipToAddress.City, ShipToAddress.Street);
    }
}
