using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using CommonProcesador;

namespace QUANTUM_ModificarLineasCredito.BaseDatos
{
    public class BDModificarLC
    {
        public static SqlConnection BDLectura
        {
            get
            {
                //return new SqlConnection(strBDLectura);
                SqlConnection unaConexion = new SqlConnection(strBDLectura);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static SqlConnection BDEscritura
        {
            get
            {
                //return new SqlConnection(strBDEscritura);
                SqlConnection unaConexion = new SqlConnection(strBDEscritura);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static String strBDLectura
        {
            get
            {
                return PNConfig.Get("MODIFICAR", "BDReadAutorizador");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }

        public static String strBDEscritura
        {
            get
            {
                return PNConfig.Get("MODIFICAR", "BDWriteAutorizador");// ;
            }
        }

    }
}
