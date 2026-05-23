using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.Shared;

public class OrderRequest
{
    public string OrderId { get; init; }
    public IReadOnlyCollection<OrderItemRequest> Items { get; init; }
}

public class OrderItemRequest
{
    public int ItemId { get; init; }
    public int Quantity { get; init; }
}
