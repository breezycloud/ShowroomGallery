using System;
using System.Collections.Generic;

#nullable disable

namespace ShowroomAPI.Models
{
    public partial class staff
    {
        public staff()
        {
            RefreshTokens = new HashSet<RefreshToken>();
            Transactions = new HashSet<Transaction>();
        }

        public int StaffId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ContactNo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
        public bool? Active { get; set; }

        public virtual UserRole UserRole { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
