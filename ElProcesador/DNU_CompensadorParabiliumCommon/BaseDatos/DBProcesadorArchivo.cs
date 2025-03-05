using CommonProcesador;
using DNU_CompensadorParabiliumCommon.Constants;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    public class DBProcesadorArchivo
    {

        private static string etiquetaProcesoComp = Processes.PROCESA_COMPENSACION.ToString();

        public static String strBDLecturaAutorizador
        {
            get
            {
                return PNConfig.Get(etiquetaProcesoComp, "BDReadAutorizador");
            }
        }

        public static String strBDEscrituraAutorizador
        {
            get
            {
                return PNConfig.Get(etiquetaProcesoComp, "BDWriteAutorizador");
            }
        }

        public static String strBDLecturaArchivo
        {
            get
            {
                return PNConfig.Get(etiquetaProcesoComp, "BDReadCompensador");
            }
        }

        public static String strBDEscrituraArchivo
        {
            get
            {
                return PNConfig.Get(etiquetaProcesoComp, "BDWriteCompensador");
            }
        }

    }
}
