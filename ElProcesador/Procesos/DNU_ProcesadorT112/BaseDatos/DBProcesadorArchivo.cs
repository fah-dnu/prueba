using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace DNU_ProcesadorT112.BaseDatos
{
    class DBProcesadorArchivo
    {
        //public static SqlConnection BDLecturaAutorizador
        //{
        //    get
        //    {
        //        //return new SqlConnection(strBDLectura);
        //        SqlConnection unaConexion = new SqlConnection(strBDLecturaAutorizador);
        //        unaConexion.Open();
        //        return unaConexion;
        //    }
        //}

        //public static SqlConnection BDEscrituraAutorizador
        //{
        //    get
        //    {
        //        //return new SqlConnection(strBDEscritura);
        //        SqlConnection unaConexion = new SqlConnection(strBDEscrituraAutorizador);
        //        unaConexion.Open();
        //        return unaConexion;
        //    }
        //}

        //public static SqlConnection BDLecturaArchivo
        //{
        //    get
        //    {
        //        //return new SqlConnection(strBDLectura);
        //        SqlConnection unaConexion = new SqlConnection(strBDLecturaArchivo);
        //        unaConexion.Open();
        //        return unaConexion;
        //    }
        //}

        //public static SqlConnection BDEscrituraArchivo
        //{
        //    get
        //    {
        //        //return new SqlConnection(strBDEscritura);
        //        SqlConnection unaConexion = new SqlConnection(strBDEscrituraArchivo);
        //        unaConexion.Open();
        //        return unaConexion;
        //    }
        //}

        public static String strBDLecturaAutorizador
        {
            get
            {
                return PNConfig.Get("PROCESAT112", "BDReadAutorizador");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }

        public static String strBDEscrituraAutorizador
        {
            get
            {
                return PNConfig.Get("PROCESAT112", "BDWriteAutorizador");// ;
            }
        }

        public static String strBDLecturaArchivo
        {
            get
            {
                return PNConfig.Get("PROCESAT112", "BDReadArchivos");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }

        public static String strBDEscrituraArchivo
        {
            get
            {
                return PNConfig.Get("PROCESAT112", "BDWriteArchivos");// ;
            }
        }

    }
}
