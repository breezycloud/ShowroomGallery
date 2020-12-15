using System;
using System.Collections.Generic;

#nullable disable

namespace ShowroomAPI.Models
{
    public partial class StockIn
    {
        public int StockInNo { get; set; }
        public Guid? ProductNo { get; set; }
        public double? Quantity { get; set; }
        public DateTime? DateIn { get; set; }

        public virtual Product ProductNoNavigation { get; set; }
    }
}
