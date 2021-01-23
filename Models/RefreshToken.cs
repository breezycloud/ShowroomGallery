using System;
using System.Collections.Generic;

#nullable disable

namespace ShowroomAPI.Models
{
    public partial class RefreshToken
    {
        public Guid TokenId { get; set; }
        public int? StaffId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }

        public virtual staff Staff { get; set; }
    }
}
