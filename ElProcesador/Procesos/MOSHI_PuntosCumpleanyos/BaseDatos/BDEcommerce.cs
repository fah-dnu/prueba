using CommonProcesador;
using System;
using System.Data.SqlClient;

namespace MOSHI_PuntosCumpleanyos.BaseDatos
{
    class BDEcommerce
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
                return PNConfig.Get("ABONOPTS", "BDReadEcommerce");
            }
        }

        public static String strBDEscritura
        {
            get
            {
                return PNConfig.Get("ABONOPTS", "BDWriteEcommerce");
            }
        }
    }
}
