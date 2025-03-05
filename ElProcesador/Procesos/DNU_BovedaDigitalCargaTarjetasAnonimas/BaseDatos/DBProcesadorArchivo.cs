using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace DNU_BovedaDigitalCargaTarjetasAnonimas.BaseDatos
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

        public static SqlConnection BDEscrituraBovedaDigital
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
                return PNConfig.Get("CARGABOVEDADIGITAL", "BDReadAutoCacao");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }

        public static String strBDEscrituraAutorizador
        {
            get
            {
                return PNConfig.Get("CARGABOVEDADIGITAL", "BDWriteAutoCacao");// ;
            }
        }

        public static String strBDLecturaArchivo
        {
            get
            {
                return PNConfig.Get("CARGABOVEDADIGITAL", "BDReadProcesadorArchivos");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }

        public static String strBDEscrituraArchivo
        {
            get
            {
                return PNConfig.Get("CARGABOVEDADIGITAL", "BDWriteProcesadorArchivos");// ;
            }
        }

        public static String strBDEscrituraAutorizadorExerno
        {
            get
            {
                return PNConfig.Get("CARGABOVEDADIGITAL", "BDWriteAutoCacaoExterno");// ;
            }
        }

        public static String strBDEscrituraBovedaDigital
        {
            get
            {
                return PNConfig.Get("CARGABOVEDADIGITAL", "BDWriteBovedaDigital");// ;
            }
        }
        //
    }
}
