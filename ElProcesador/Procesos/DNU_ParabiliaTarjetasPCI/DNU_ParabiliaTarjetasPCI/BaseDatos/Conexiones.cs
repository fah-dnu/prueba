// -----------------------------------------------------------------------
// <copyright file="Conexiones.cs" company="DNU">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DNU_ParabiliaTarjetasPCI.BaseDatos
{
    using CommonProcesador;
    using System;
    using System.Data.SqlClient;

    /// <summary>
    /// Clase de obtiene parametro de conexion
    /// </summary>
    public  class Conexiones
    {

        /// <summary>
        /// Retorna cadena de conexion obtenida por el common Procesador
        /// </summary>
        public static String strBDEscrituraAutoCacao
        {
            get
            {
                return PNConfig.Get("PRPROCESARTARPCI", "BDWriteAutoCacao");
            }
        }

        /// <summary>
        /// Retorna Cadena de Conexion obtenida por el common Procesador
        /// </summary>
        public static String strBDLecturaAutoCacao
        {
            get
            {
                return PNConfig.Get("PRPROCESARTARPCI", "BDReadAutoCacao");
            }
        }

        public static string strBDEscrituraConsultaClientes 
        {
            get 
            {
                return PNConfig.Get("PRPROCESARTARPCI", "BDWriteConsultaClientes");
            }
        }
        public static string strBDLecturaConsultaClientes 
        
        { 
            get 
            {
                return PNConfig.Get("PRPROCESARTARPCI", "BDReadConsultaClientes");
            }
        }

        /// <summary>
        /// abre conexion de lectura auto_cacao
        /// </summary>
        public static SqlConnection BDLecturaAutoCacao
        {
            get
            {
                
                SqlConnection unaConexion = new SqlConnection(strBDLecturaAutoCacao);
                unaConexion.Open();
                return unaConexion;
            }
        }


        public static SqlConnection ConsultaFechaClientesLectura
        {
            get
            {

                SqlConnection unaConexion = new SqlConnection(strBDLecturaConsultaClientes);
                unaConexion.Open();
                return unaConexion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static SqlConnection ConsultaFechaClientesEscritura        {
            get
            {

                SqlConnection unaConexion = new SqlConnection(strBDEscrituraConsultaClientes);
                unaConexion.Open();
                return unaConexion;
            }
        }
        /// <summary>
        /// Abre conexion de escrituta auto_cacao
        /// </summary>
        public static SqlConnection BDEscrituraAutoCacao
        {
            get
            {

                SqlConnection unaConexion = new SqlConnection(strBDLecturaAutoCacao);
                unaConexion.Open();
                return unaConexion;
            }
        }
        public static String strBDLecturaArchivo
        {
            get
            {
                return PNConfig.Get("PRPROCESARTARPCI", "BDReadProcesadorArchivos");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }

    }
}
