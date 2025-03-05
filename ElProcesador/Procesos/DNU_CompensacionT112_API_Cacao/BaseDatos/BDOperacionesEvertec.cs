using CommonProcesador;
using System;
using System.Data.SqlClient;

namespace DNU_CompensacionT112_API_Cacao.BaseDatos
{
    class BDOperacionesEvertec
    {
        //public static SqlConnection BDLectura
        //{
        //    get
        //    {
        //        SqlConnection unaConexion = new SqlConnection(strBDLecturaArchivo);
        //        unaConexion.Open();
        //        return unaConexion;
        //    }
        //}

        //public static SqlConnection BDEscritura
        //{
        //    get
        //    {
        //        SqlConnection unaConexion = new SqlConnection(strBDEscrituraArchivo);
        //        unaConexion.Open();
        //        return unaConexion;
        //    }
        //}

        public static String strBDLecturaArchivo
        {
            get
            {
                return PNConfig.Get("COMPT112APICACAO", "BDReadOperacionesEvertec");
            }
        }

        public static String strBDEscrituraArchivo
        {
            get
            {
                return PNConfig.Get("COMPT112APICACAO", "BDWriteOperacionesEvertec");
            }
        }
    }
}
