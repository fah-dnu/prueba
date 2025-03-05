using CommonProcesador;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DNU_CompensacionT112Evertec.BaseDatos
{
    public static class BDT112
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
#if DEBUG
                return ConfigurationManager.ConnectionStrings["BDAutorizadorRead"].ConnectionString;
#else
                return PNConfig.Get("COMPT112APICACAO", "BDReadT112").ToString();
#endif

            }
        }

        public static String strBDEscrituraArchivo
        {
            get
            {
#if DEBUG
                return ConfigurationManager.ConnectionStrings["BDAutorizadorWritte"].ConnectionString;
#else
                return PNConfig.Get("COMPT112APICACAO", "BDWriteT112").ToString();
#endif
            }
        }
        public static String strFechaProcesamientoLimite
        {
            get
            {
                return PNConfig.Get("COMPT112APICACAO", "FechaProcesamientoLimite").ToString();
            }
        }

    }
}
