using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class CreateAdminManagerRequestDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }      
        public int RoleID { get; set; }       
    }
}
