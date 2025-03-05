using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class RequestGenerarPinBlock
    {
        public string cardPAN { get; set; }
        public string clearPIN { get; set; }
        public string keyPINBlock { get; set; }
    }
}
