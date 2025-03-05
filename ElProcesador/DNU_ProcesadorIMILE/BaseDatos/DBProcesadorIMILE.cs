using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorIMILE.BaseDatos
{
    public class DBProcesadorIMILE
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
                return PNConfig.Get("DNU_ProcesadorIMILE", "BDReadIMILE");
            }
        }

        public static String strBDEscritura
        {
            get
            {
                return PNConfig.Get("DNU_ProcesadorIMILE", "BDWriteIMILE");
            }
        }
    }
}
