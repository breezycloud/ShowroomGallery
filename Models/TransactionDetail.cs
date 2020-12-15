using System;
using System.Collections.Generic;

#nullable disable

namespace ShowroomAPI.Models
{
    public partial class TransactionDetail
    {
        public int TdetailsNo { get; set; }
        public string InvoiceNo { get; set; }
        public Guid ProductNo { get; set; }
        public decimal? ItemPrice { get; set; }
        public double? Quantity { get; set; }

        public virtual Transaction InvoiceNoNavigation { get; set; }
        public virtual Product ProductNoNavigation { get; set; }
    }
}
