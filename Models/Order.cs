using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowroomAPI.Models
{
    public class Order
    {
        public Order()
        {
            OrderItems = new List<OrderItems>();
        }
        public string InvoiceNo { get; set; }
        public string OrderDate { get; set; }
        public string Cashier { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }

        public List<OrderItems> OrderItems { get; set; }
    }
}
