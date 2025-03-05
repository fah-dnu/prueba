using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class CuentaAdicional : Cuentas
    {
        public List<string> Archivos { get; set; }
        public bool EstatusEnvioCorreo{ get; set; }
        public string IDMATarjetaAdicional { get; set; }
        public string ID_CuentaCompensacion { get; set; }
        public string RutaEdoCuenta { get; set; }
        public string ID_ColectivaAdicional { get; set; }
        // public bool idCuentaConsecutivo { get; set; }
    }
}
