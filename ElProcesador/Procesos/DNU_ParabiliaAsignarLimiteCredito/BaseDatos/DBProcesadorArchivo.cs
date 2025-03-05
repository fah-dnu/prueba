using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace DNU_ParabiliaAsignarLimiteCredito.BaseDatos
{
    class DBProcesadorArchivo
    {
        public static SqlConnection BDLecturaAutorizador
        {
            get
            {
                //return new SqlConnection(strBDLectura);
                SqlConnection unaConexion = new SqlConnection(strBDLecturaAutorizador);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static SqlConnection BDEscrituraAutorizador
        {
            get
            {
                //return new SqlConnection(strBDEscritura);
                SqlConnection unaConexion = new SqlConnection(strBDEscrituraAutorizador);
                unaConexion.Open();
                return unaConexion;
            }
        }



        public static String strBDLecturaAutorizador
        {
            get
            {
                return PNConfig.Get("ASIGNALIMITECREDITO", "BDReadAuto");
            }
        }

        public static String strBDEscrituraAutorizador
        {
            get
            {
                return PNConfig.Get("ASIGNALIMITECREDITO", "BDWriteAuto");
            }
        }

        public static String strBDEscrituraBatch
        {
            get
            {
                return PNConfig.Get("ASIGNALIMITECREDITO", "BDWriteBatch");
            }
        }
    }
}
