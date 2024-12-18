using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Response
{
    public class CheckPointResponse
    {
        public int UserId { get; set; }
        public int BonusPoint { get; set; } // Số điểm thưởng còn lại
    }
}
