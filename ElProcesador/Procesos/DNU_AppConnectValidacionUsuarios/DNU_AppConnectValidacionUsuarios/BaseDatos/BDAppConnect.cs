using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_AppConnectValidacionUsuarios.BaseDatos
{
    class BDAppConnect
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
                return PNConfig.Get("LimpiarUsuarios", "BDReadAppConnect");
            }
        }

        public static String strBDEscritura
        {
            get
            {
                return PNConfig.Get("LimpiarUsuarios", "BDWriteAppConnect");
            }
        }
    }
}
