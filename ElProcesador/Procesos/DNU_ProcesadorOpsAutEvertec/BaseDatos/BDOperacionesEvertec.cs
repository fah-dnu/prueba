using CommonProcesador;
using System;
using System.Data.SqlClient;

namespace DNU_ProcesadorOpsAutEvertec.BaseDatos
{
    class BDOperacionesEvertec
    {
        public static SqlConnection BDLectura
        {
            get
            {
                SqlConnection unaConexion = new SqlConnection(strBDLecturaArchivo);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static SqlConnection BDEscritura
        {
            get
            {
                SqlConnection unaConexion = new SqlConnection(strBDEscrituraArchivo);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static String strBDLecturaArchivo
        {
            get
            {
                return PNConfig.Get("OPAUTEVERTEC", "BDReadArchivos");
            }
        }

        public static String strBDEscrituraArchivo
        {
            get
            {
                return PNConfig.Get("OPAUTEVERTEC", "BDWriteArchivos");
            }
        }

    }
}
