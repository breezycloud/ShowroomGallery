namespace ShowroomAPI.Models
{
    public class UserWithToken : staff
    {

        public UserWithToken(staff staff)
        {
            this.StaffId = staff.StaffId;
            this.FirstName = staff.FirstName;
            this.LastName = staff.LastName;
            this.Address = staff.Address;
            this.City = staff.City;
            this.ContactNo = staff.ContactNo;
            this.Username = staff.Username;
            this.Password = staff.Password;
            this.UserRole = staff.UserRole;
        }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}