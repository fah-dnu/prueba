using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class RequestTraducirPinBlock
    {
        public string keySource { get; set; }
        public string keyTarget { get; set; }
        public string pinBlockSource { get; set; }
    }
}
