using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.AutorizadorParabilia_NCliente.Entidades
{
    public class RWSMail
    {
        public int responseCode { get; set; }
        public string message { get; set; }
        public string stepFailed { get; set; }
    }
}
