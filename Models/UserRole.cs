using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#nullable disable

namespace ShowroomAPI.Models
{
    public partial class UserRole
    {
        public int RoleId { get; set; }
        public string RoleDesc { get; set; }

        [JsonIgnore]
        public virtual staff staff { get; set; }
    }
}
