using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalesOrder_Paramount.Connection;
using SalesOrder_Paramount.Models;
using SalesOrder_Paramount.Models.DTO;
using SalesOrder_Paramount.Models.Setting;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SalesOrder_Paramount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SalesOrderController
    {
        private readonly ILogger _logger;
        private readonly SAP_Connection connection;
        private Setting _setting;

        public SalesOrderController(IOptions<Setting> setting, ILogger<HomeController> logger)
        {
            this.connection = new SAP_Connection(setting.Value);
            _logger = logger;
            _setting = setting.Value;

        }

        [HttpGet]
        public async Task<string> GetAsync()
        {

            var rawInvoice = new List<DataModel>();


            if (connection.Connect() == 0)
            {

                SAPbobsCOM.Recordset recordSet = null;
                recordSet = connection.GetCompany().GetBusinessObject(BoObjectTypes.BoRecordset);
                recordSet.DoQuery($"SELECT  T0.\"ID\", T0.\"OrderNo\", T0.\"CustomerCode\", T0.\"CustomerName\", T0.\"ItemCode\", T0.\"ItemName\", T0.\"ItemQuantity\", T0.\"Price\", T0.\"GrandTotal\", T0.\"SaleOrderDate\",T0.\"Warehouse\", T0.\"Remarks\", T0.\"DeliveryDate\" FROM \"@SaleOrders\" T0 WHERE TO_VARCHAR(\"CreatedDate\",'YYYY-MM-DD HH:mm:SS') >= '2022-08-01 11:00:00' AND TO_VARCHAR(\"CreatedDate\",'YYYY-MM-DD HH:mm:SS') <= '2022-08-01 12:00:00' AND T0.\"IsPostedToSAP\"= 0");//{DateTime.Now.ToShortDateString()}


                var init = 0;
                var total = recordSet.RecordCount;

                while (init < total) {

                    var Id= recordSet.Fields.Item(0).Value.ToString();
                    var OrderNo = recordSet.Fields.Item(1).Value.ToString();
                    var CustomerCode = recordSet.Fields.Item(2).Value.ToString();
                    var CustomerName = recordSet.Fields.Item(3).Value.ToString();
                    var ItemCode = recordSet.Fields.Item(4).Value.ToString();
                    var ItemName = recordSet.Fields.Item(5).Value.ToString();
                    var Quantity = recordSet.Fields.Item(6).Value.ToString();
                    var Price = recordSet.Fields.Item(7).Value.ToString();
                    var GrandTotal = recordSet.Fields.Item(8).Value.ToString();
                    string SaleOrderDate = recordSet.Fields.Item(9).Value.ToString();
                    SaleOrderDate = SaleOrderDate.Split(" ")[0];
                    var WareHouse = recordSet.Fields.Item(10).Value.ToString();
                    var Remarks = recordSet.Fields.Item(11).Value.ToString();
                    string DeliveryDate = recordSet.Fields.Item(12).Value.ToString();
                    DeliveryDate = DeliveryDate.Split(" ")[0];

                    rawInvoice.Add(new DataModel() {  Id=int.Parse(Id), CustomerCode=CustomerCode, CustomerName=CustomerName, GrandTotal=float.Parse(GrandTotal), ItemCode=ItemCode, ItemName=ItemName, OrderNo=OrderNo, Price=float.Parse(Price), Quantity=int.Parse(Quantity), Remarks = Remarks, SaleOrderDate =DateTime.Parse(SaleOrderDate), WareHouse = WareHouse , DeliveryDate = DateTime.Parse(DeliveryDate)});
                    init += 1;
                    recordSet.MoveNext();
                }


                Documents oSO = null;
                var processData = await InvoiceMapper(rawInvoice);
                foreach (var singleInvoice in processData)
                {
                    var userResponse = await CheckOrderExist(singleInvoice.OrderNo);
                    if (!userResponse)
                    {
                        _logger.LogError("Sale Order already exists");
                        return "SAP B1 Background service";
                    }

                    oSO = connection.GetCompany().GetBusinessObject(BoObjectTypes.oOrders);

                    oSO.NumAtCard =singleInvoice.OrderNo;
                    oSO.DocDate = singleInvoice.OrderDate;
                    oSO.DocDueDate = singleInvoice.DeliveryDate;
                    oSO.CardCode = singleInvoice.CustomerCode;
                    oSO.Comments = "Posted By SAP-Exd SaleOrder Utitlity ";
                    foreach (var OrderItem in singleInvoice.orderDetails)
                    {
                        oSO.Lines.ItemCode = OrderItem.ItemCode.ToString();
                        oSO.Lines.Quantity = Convert.ToDouble(OrderItem.Quantity);
                        oSO.Lines.UnitPrice = Convert.ToDouble(OrderItem.UnitPrice);
                        oSO.Lines.WarehouseCode =singleInvoice.WareHouse;
                        
           
                        oSO.Lines.Add();
                    }

                    recordSet.DoQuery($"Select \"BPLId\" from \"OBPL\" WHERE \"DflWhs\"='{singleInvoice.WareHouse}'");
                    if (recordSet.RecordCount > 0) {
                        var Id = recordSet.Fields.Item(0).Value.ToString();
                        oSO.BPL_IDAssignedToInvoice = int.Parse(Id);
                    }
                   
                    if (oSO.Add() == 0)
                    {
                        Console.WriteLine("Success:Record added successfully");
                    }
                    else {

                        var errCode = connection.GetCompany().GetLastErrorCode();
                        var response = connection.GetCompany().GetLastErrorDescription();
                        _logger.LogError($"{errCode}:{response}");
                    
                    }
                  
                }
                //if (processData.Count > 0)
                //{

                //    recordSet.DoQuery($"UPDATE \"@SaleOrders\" SET T0.\"IsPostedToSAP\" = 1 T0 WHERE  TO_VARCHAR(\"CreatedDate\",'YYYY-MM-DD HH:mm:SS') >= '2022-08-01 11:00:00' AND TO_VARCHAR(\"CreatedDate\",'YYYY-MM-DD HH:mm:SS') <= '2022-08-01 12:00:00'  AND T0.\"IsPostedToSAP\"= 0");//{DateTime.Now.ToShortDateString()}

                //}

            }

            else
            {
                Console.WriteLine("Error " + connection.GetErrorCode() + ": " + connection.GetErrorMessage());
            }

            connection.GetCompany().Disconnect();
            return "SAP B1 Background service";
        }

        private async Task<bool> CheckOrderExist(string OrderCode)
        {
            bool output = false;
            Recordset recordSet = connection.GetCompany().GetBusinessObject(BoObjectTypes.BoRecordset);
            Documents oSO = connection.GetCompany().GetBusinessObject(BoObjectTypes.oOrders);
            recordSet.DoQuery($"SELECT * FROM \"ORDR\" WHERE \"NumAtCard\"='{OrderCode}'");
            if (recordSet.RecordCount == 0)
            {
                output = false;
            }
            else
            {
                output = true;
            }

            return output;
        }

        private async Task<List<Orders>> InvoiceMapper(List<DataModel> data)
        {

            List<Orders> SaleOrders = new List<Orders>();
            List<DataModel> resp = data.Select(x => new { x.CustomerCode, x.OrderNo}).Distinct()
                .Select(x => data.FirstOrDefault(r => r.CustomerCode == x.CustomerCode && r.OrderNo == x.OrderNo)).Distinct().ToList();

            foreach (var item in resp)
            {
                var orderItems = data.Where(x => x.OrderNo == item.OrderNo && x.CustomerCode == item.CustomerCode)
                    .Select(x => new OrderDetail { ItemCode = x.ItemCode, Quantity = x.Quantity, UnitPrice=x.Price }).Distinct().ToList();
                SaleOrders.Add(new Orders() { WareHouse=item.WareHouse,Remarks= item.Remarks, CustomerCode = item.CustomerCode, OrderNo = item.OrderNo, OrderDate = item.SaleOrderDate, DeliveryDate = item.DeliveryDate , orderDetails = orderItems });
            }

            return SaleOrders;
        }
    }
}
