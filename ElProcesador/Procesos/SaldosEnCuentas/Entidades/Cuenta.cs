using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitoreaCuentas.Entidades
{
    public class Cuenta
    {
        public Int64 ID_Cuenta { get; set; }
        public String nombreCuenta { get; set; }
        public String CuentaHabiente { get; set; }
        public Decimal Saldo { get; set; }
        public String ListaDistribucion { get; set; }

    }
}
