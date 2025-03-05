using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_MovimientoManualMasivo.BaseDatos
{
    public class DBProcesadorArchivo
    {
        public static String strBDLecturaAutorizador
        {
            get
            {
                return PNConfig.Get("MOVIMIENTO_MASIVO", "BDReadAutorizador");
            }
        }

        public static String strBDEscrituraAutorizador
        {
            get
            {
                return PNConfig.Get("MOVIMIENTO_MASIVO", "BDWriteAutorizador");
            }
        }

        public static String strBDEscrituraBatch
        {
            get
            {
                return PNConfig.Get("MOVIMIENTO_MASIVO", "BDWriteBatch");
            }
        }
    }
}
