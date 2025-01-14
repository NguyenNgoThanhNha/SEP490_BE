using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Dtos
{
    public class GrossDTO
    {
        public bool HasGross { get; set; }
        public List<string> Grosses { get; set; }
    }
}
