using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    public class BDAutorizador
    {
        public static String strBDLectura
        {
            get
            {
                return PNConfig.Get("PROCESA_COMPENSACION", "BDReadAutorizador");
            }
        }

        public static String strBDEscritura
        {
            get
            {
                return PNConfig.Get("PROCESA_COMPENSACION", "BDWriteAutorizador");
            }
        }
    }
}
