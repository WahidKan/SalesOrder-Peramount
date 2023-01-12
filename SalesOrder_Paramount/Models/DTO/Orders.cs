using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesOrder_Paramount.Models.DTO
{
    public class Orders
    {
        public string OrderNo { get; set; }
        public string CustomerCode { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string WareHouse { get; set; }
        public string Remarks { get; set; }
        public List<OrderDetail> orderDetails { get; set; }
    }

    public class OrderDetail
    {
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
    }
}
