using Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Services
{
    public interface IStaffService
    {
        Task<Staff?> GetStaffByUserId(Guid userId);
    }

}
