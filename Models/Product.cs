using System;
using System.Collections.Generic;

#nullable disable

namespace ShowroomAPI.Models
{
    public partial class Product
    {
        public Product()
        {
            StockIns = new HashSet<StockIn>();
            TransactionDetails = new HashSet<TransactionDetail>();
        }

        public Guid ProductNo { get; set; }
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public decimal UnitPrice { get; set; }
        public double StocksOnHand { get; set; }
        public int? ReorderLevel { get; set; }
        public Guid? CategoryNo { get; set; }
        public string ModelNo { get; set; }

        public virtual Category CategoryNoNavigation { get; set; }
        public virtual ICollection<StockIn> StockIns { get; set; }
        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; }
    }
}
