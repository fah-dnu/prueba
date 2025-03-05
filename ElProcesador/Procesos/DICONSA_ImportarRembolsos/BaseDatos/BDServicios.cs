using CommonProcesador;
using System;
using System.Data.SqlClient;

namespace DICONSA_ImportarRembolsos.BaseDatos
{
    class BDServicios
    {
        public static SqlConnection BDLectura
        {
            get
            {
                SqlConnection unaConexion = new SqlConnection(strBDLectura);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static SqlConnection BDEscritura
        {
            get
            {
                SqlConnection unaConexion = new SqlConnection(strBDEscritura);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static String strBDLectura
        {
            get
            {
                return PNConfig.Get("IMPORTATX", "BDReadServicios");
            }
        }

        public static String strBDEscritura
        {
            get
            {
                return PNConfig.Get("IMPORTATX", "BDWriteServicios");
            }
        }
    }
}
