using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryOrderProcessor.Models;

public class OrderDetailDocument
{
    public string id { get; set; }
    public string ShippingAddress { get; set; } = default!;
    public List<OrderItemDetailDocument> Items { get; set; } = new();
    public decimal OrderTotal { get; set; }
}
