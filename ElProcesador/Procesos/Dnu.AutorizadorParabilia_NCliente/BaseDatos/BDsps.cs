using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using CommonProcesador;
using Dnu.AutorizadorParabilia_NCliente.Entidades;
using Dnu.AutorizadorParabilia_NCliente.LogicaNegocio;
using Dnu.AutorizadorParabilia_NCliente.Utilities;
using Npgsql;

namespace Dnu.AutorizadorParabilia_NCliente.BaseDatos
{
    public class BDsps
    {
        public static DataSet EjecutarSP(string procedimiento, Hashtable parametros, string pConexion)
        {
            DataSet retorno = new DataSet();
            SqlConnection connection = new SqlConnection(pConexion);
            try
            {

                connection.Open();
                SqlCommand query = new SqlCommand(procedimiento, connection);
                query.CommandType = CommandType.StoredProcedure;
                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                    }

                }

                SqlDataAdapter sda = new SqlDataAdapter(query);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional : null;
            }
            catch (Exception e)
            {
                Logueo.Error("[EjecutarSP] [Error al ejecutar sp: " + procedimiento + "] [Conexion: "+connection+"] [Mensaje: " + e.Message + " TRACE: " + e.StackTrace + "]");
                return null;
            }
            finally
            {
                connection.Close();
            }

            return retorno;
        }

        public static DataSet EjecutarSP_Postgres(string procedimiento, Hashtable parametros, string pConexion)
        {
            DataSet retorno = new DataSet();
            NpgsqlConnection connection = new NpgsqlConnection(pConexion);
            try
            {

                connection.Open();
                NpgsqlCommand query = new NpgsqlCommand(procedimiento, connection);
                query.CommandType = CommandType.StoredProcedure;
                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                    }

                }

                NpgsqlDataAdapter sda = new NpgsqlDataAdapter(query);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional : null;
            }
            catch (Exception e)
            {
                Logueo.Error("[EjecutarSP] [Error al ejecutar sp: " + procedimiento + "] [Mensaje: " + e.Message + "]");
                return null;
            }
            finally
            {
                connection.Close();
            }

            return retorno;
        }
        public static void insertarRespuestaEvertecAltas(XmlDocument pResponse, SqlConnection pConn, string pIdDetalle, string pNombre)
        {
            SqlCommand query = new SqlCommand("procnoc_travel_InsertaRespuestaEvertecAltas", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@codRespuesta", pResponse.GetElementsByTagName("Codigo")[0].InnerText);
            query.Parameters.AddWithValue("@cuenta", SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Cuenta")[0].InnerText));
            query.Parameters.AddWithValue("@descRespuesta", pResponse.GetElementsByTagName("Descripcion")[0].InnerText);
            query.Parameters.AddWithValue("@fecha", pResponse.GetElementsByTagName("Fecha")[0].InnerText);
            query.Parameters.AddWithValue("@hora", pResponse.GetElementsByTagName("Hora")[0].InnerText);
            query.Parameters.AddWithValue("@identificacion", SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Identificacion")[0].InnerText));
            query.Parameters.AddWithValue("@nombre", pNombre);
            query.Parameters.AddWithValue("@tarjeta", SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Tarjeta")[0].InnerText));
            query.Parameters.AddWithValue("@idArchivoDetalle", pIdDetalle);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
        }

        public static void insertarRespuestaEvertecAltasReproceso(SqlConnection pConn, string pIdDetalle, string claveEmpleadora
                                                            , string regEmpleado)
        {
            SqlCommand query = new SqlCommand("procnoc_travel_InsertaRespuestaEvertecAltasReproceso", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@idArchivoDetalle", pIdDetalle);
            query.Parameters.AddWithValue("@claveEmpleadora", claveEmpleadora);
            query.Parameters.AddWithValue("@regEmpleado", regEmpleado);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
        }

        public static void insertarRespuestaEvertecBajas(XmlDocument pResponse, SqlConnection pConn, string pIdDetalle)
        {
            SqlCommand query = new SqlCommand("procnoc_travel_InsertaRespuestaEvertecBajas", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@numeroTarjeta", SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("numeroTarjeta")[0].InnerText));
            query.Parameters.AddWithValue("@estadoDeseado", pResponse.GetElementsByTagName("estadoDeseado")[0].InnerText);
            query.Parameters.AddWithValue("@motivoCancelacion", pResponse.GetElementsByTagName("motivoCancelacion")[0].InnerText);
            query.Parameters.AddWithValue("@codigoRespuesta", pResponse.GetElementsByTagName("codigoRespuesta")[0].InnerText);
            query.Parameters.AddWithValue("@descripcionRespuesta", pResponse.GetElementsByTagName("descripcionRespuesta")[0].InnerText);
            query.Parameters.AddWithValue("@idArchivoDetalle", pIdDetalle);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
        }

        public static void insertarRespuestaEvertecBajasReproceso(SqlConnection pConn, string pIdDetalle, string claveEmpleadora
                                                            , string regEmpleado)
        {
            SqlCommand query = new SqlCommand("procnoc_travel_InsertaRespuestaEvertecBajasReproceso", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@idArchivoDetalle", pIdDetalle);
            query.Parameters.AddWithValue("@claveEmpleadora", claveEmpleadora);
            query.Parameters.AddWithValue("@regEmpleado", regEmpleado);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
        }


        public static DataSet insertarDatosParabiliaAltas(ParamParabiliaAltas pParametros, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("Procnoc_Travel_AltaParabilia", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@claveColectiva", pParametros.ClaveColectiva);
            query.Parameters.AddWithValue("@nombre", pParametros.Nombre);
            query.Parameters.AddWithValue("@tarjeta", SeguridadCifrado.descifrar(pParametros.Tarjeta));
            query.Parameters.AddWithValue("@idArchivoDetalleAltas", pParametros.IdArchivoDetalle);
            query.Parameters.AddWithValue("@cuenta", SeguridadCifrado.descifrar(pParametros.NumeroCuenta));
            query.Parameters.AddWithValue("@idEmpPadre", pParametros.IdEmpleadoraPadre);
            query.Parameters.AddWithValue("@fechaVencimiento", pParametros.FechaVencimientoTarjeta);
            query.Parameters.AddWithValue("@correoCFDI", pParametros.correoCFDI);
            query.Parameters.AddWithValue("@correo", pParametros.correo);
            query.Parameters.AddWithValue("@telefono", pParametros.telefono);


            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }

        public static DataSet insertarDatosParabiliaBajas(ParamParabiliaBajas pParametros, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("Procnoc_Travel_BajaParabilia", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@claveColectiva", pParametros.ClaveColectiva);
            query.Parameters.AddWithValue("@numTarjeta", SeguridadCifrado.descifrar(pParametros.NumeroTarjeta));
            query.Parameters.AddWithValue("@idArchivoDetalleBajas", pParametros.ArchivoDetalleBajas);
            query.Parameters.AddWithValue("@claveEmpleadora", pParametros.ClaveEmpleadora);
            query.Parameters.AddWithValue("@claveTipoEmpleadora", pParametros.ClaveTipoEmpleadora);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }

        public static DataSet ObtenerDatosCuenta(string pCuenta, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("procnoc_travel_ObtieneDatosCuenta", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@numeroCuenta", SeguridadCifrado.descifrar(pCuenta));

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }
    }
}
