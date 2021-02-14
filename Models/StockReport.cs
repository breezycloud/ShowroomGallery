using System;

namespace ShowroomAPI.Models
{
    public class StockReport
    {
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal SubTotal { get; set; }
    }
}