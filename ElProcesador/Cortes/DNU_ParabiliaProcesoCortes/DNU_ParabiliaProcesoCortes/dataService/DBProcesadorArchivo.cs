using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.dataService
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

        public static SqlConnection BDLecturaArchivo
        {
            get
            {
                //return new SqlConnection(strBDLectura);
                SqlConnection unaConexion = new SqlConnection(strBDLecturaArchivo);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static SqlConnection BDEscrituraArchivo
        {
            get
            {
                //return new SqlConnection(strBDEscritura);
                SqlConnection unaConexion = new SqlConnection(strBDEscrituraArchivo);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static SqlConnection BDEscrituraAutorizadorExterno
        {
            get
            {
                //return new SqlConnection(strBDEscritura);
                SqlConnection unaConexion = new SqlConnection(strBDEscrituraAutorizadorExerno);
                unaConexion.Open();
                return unaConexion;
            }
        }



        public static String strBDLecturaAutorizador
        {
            get
            {
                return PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizador");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }

        public static String strBDEscrituraAutorizador
        {
            get
            {
                return PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizador");// ;
            }
        }

        public static String strBDLecturaArchivo
        {
            get
            {
                return PNConfig.Get("PROCESAEDOCUENTA", "BDWriteProcesadorArchivosCacao");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }

        public static String strBDEscrituraArchivo
        {
            get
            {
                return PNConfig.Get("PROCESAEDOCUENTA", "BDWriteProcesadorArchivosCacao");// ;
            }
        }

        public static String strBDEscrituraAutorizadorExerno
        {
            get
            {
                return PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizador");// ;
            }
        }
        //
    }
}
