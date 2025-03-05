// -----------------------------------------------------------------------
// <copyright file="Conexiones.cs" company="DNU">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DNU_ParabiliaTarjetasStock.BaseDeDatos
{
    using CommonProcesador;
    using System;
    using System.Data.SqlClient;

    /// <summary>
    /// Clase de obtiene parametro de conexion
    /// </summary>
    public class Conexion
    {

        /// <summary>
        /// Retorna cadena de conexion obtenida por el common Procesador
        /// </summary>
        public static String strBDEscrituraAutoCacao
        {
            get
            {
                return PNConfig.Get("PRPROCESARTARSTOCK", "BDWriteAutoCacao");
            }
        }

        /// <summary>
        /// Retorna Cadena de Conexion obtenida por el common Procesador
        /// </summary>
        public static String strBDLecturaAutoCacao
        {
            get
            {
                return PNConfig.Get("PRPROCESARTARSTOCK", "BDReadAutoCacao");
            }
        }

        public static string strBDEscrituraConsultaClientes
        {
            get
            {
                return PNConfig.Get("PRPROCESARTARSTOCK", "BDWriteConsultaClientes");
            }
        }
        public static string strBDLecturaConsultaClientes

        {
            get
            {
                return PNConfig.Get("PRPROCESARTARSTOCK", "BDReadConsultaClientes");
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
        public static SqlConnection ConsultaFechaClientesEscritura
        {
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
                return PNConfig.Get("PRPROCESARTARSTOCK", "BDReadProcesadorArchivos");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }

    }
}
