using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Request
{
    public class CreateUserRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int RoleID { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? TypeLogin { get; set; } = "normal";
    }

}
