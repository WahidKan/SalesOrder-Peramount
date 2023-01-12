using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesOrder_Paramount.Models
{
    public class DataModel
    {
        public int Id { get; set; }
        public string OrderNo { get; set; }
        public string UserName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
        public float GrandTotal { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string WareHouse { get; set; }
        public string Remarks { get; set; }
        public DateTime DeliveryDate { get; set; }
    }
}
