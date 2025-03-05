using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class ClienteExterno
    {
        public string clientId { get; set; }
        public List<CuentaAhorroCLABE> cuentas { get; set; }
    }
}
