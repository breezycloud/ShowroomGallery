using System;
using System.Collections.Generic;

#nullable disable

namespace ShowroomAPI.Models
{
    public partial class Transaction
    {
        public Transaction()
        {
            TransactionDetails = new HashSet<TransactionDetail>();
        }

        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? Tdate { get; set; }
        public TimeSpan? Ttime { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? StaffId { get; set; }
        public decimal? Discount { get; set; }
        public decimal? SubTotal { get; set; }
        public byte[] Receipt { get; set; }

        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; }
    }
}
