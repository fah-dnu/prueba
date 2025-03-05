using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TELEVIP_ImportarTipoCuenta
{
    public class CuentaTeleVIA
    {
        public Int64 ID_Registro { get; set; }
        public String CuentaOrigen { get; set; }
        public String CuentaDestino { get; set; }

        public DateTime FechaBaja { get; set; }

        public DateTime FechaMigracion { get; set; }

        public Int32 NuevoTipoCuenta { get; set; }



                
    }
}
