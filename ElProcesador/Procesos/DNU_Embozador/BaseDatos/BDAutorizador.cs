using CommonProcesador;
using DNU_Embozador.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.BaseDatos
{
    public class BDAutorizador
    {

        public static string ObtenerConeccion(string key)
        {
            return PNConfig.Get(Constants.PROC_NAME, key);
        }


        public static String strBDLectura
        {
            get
            {
                return PNConfig.Get(Constants.PROC_NAME, "BDReadAutorizador");
            }
        }

        public static String strBDEscritura
        {
            get
            {
                return PNConfig.Get(Constants.PROC_NAME, "BDWriteAutorizador");
            }
        }


    }
}
