using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowroomAPI.Models
{
    public class OrderItems
    {
        public double Quantity { get; set; }
        public string ProductName { get; set; }
        public decimal ItemPrice { get; set; }
    }
}
