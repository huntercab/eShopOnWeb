using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderItemsReserver
{
    public class PurchaseOrderRequest
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
