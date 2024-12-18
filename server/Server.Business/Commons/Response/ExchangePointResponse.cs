using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Commons.Response
{
    public class ExchangePointResponse
    {
        public string Message { get; set; }
        public int RemainingPoints { get; set; }
        public string PromotionName { get; set; }
    }
}
