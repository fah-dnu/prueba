using CommonProcesador;
using DNU_CompensacionT112_API_Cacao.BaseDatos;
using DNU_CompensacionT112_API_Cacao.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DNU_CompensacionT112Evertec.BaseDatos
{
    public class DAOT112
    {

        public static List<ArchivoT112> ObtieneArchivosT112PorCiclo(string fecha, SqlConnection dbConnections)
        {

            List<ArchivoT112> ficheros = new List<ArchivoT112>();
            int? idEstatus = null;
            using (SqlCommand cmd = new SqlCommand())
            {
                int? estatusDef = null;
                cmd.Parameters.Add(new SqlParameter("@fechaArchivo", fecha));

                cmd.Connection = dbConnections;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.CommandText = "ProcNoc_ObtieneArchivosT112PorCiclo";

                DataTable dt = new DataTable();

                dt.Load(cmd.ExecuteReader());

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ficheros.Add(new ArchivoT112
                        {
                            ID_Fichero = Convert.ToInt64(reader["ID_Fichero"].ToString()),
                            NombreFichero = reader["NombreFichero"].ToString(),
                            ID_EstatusFichero = reader["ID_EstatusFichero"] != null & !String.IsNullOrEmpty(reader["ID_EstatusFichero"].ToString()) ? Convert.ToInt32(reader["ID_EstatusFichero"].ToString()) : estatusDef
                        });
                    }
                }
            }

            return ficheros;
        }


        public static List<String> ObtenerFechasAProcesar(int timeOut)
        {

            List<String> fechas = new List<string>();
            using (SqlConnection con = new SqlConnection(BDT112.strBDLecturaArchivo))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "PROCNOC_ObtieneFechasAProcesar";
                    cmd.CommandTimeout = timeOut;

                    con.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fechas.Add(reader["FechaProceso"].ToString());
                        }

                    }
                }
            }

            return fechas;
        }


        public static bool VerificaRegistroRepetidoT112(Int64 idFicheroDetalle, SqlConnection dbConnections)
        {

            //using (SqlConnection con = new SqlConnection(BDT112.strBDLecturaArchivo))
            //{

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = dbConnections;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Proc_VerificaDuplicidadTransaccion";

                    SqlParameter idParam = new SqlParameter("@idFicheroDetalle", idFicheroDetalle);
                    cmd.Parameters.Add(idParam);

                    //con.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader["Duplicado"].ToString().Equals("1");
                        }

                        throw new Exception(String.Format("Ocurrio un error en la validación del caso duplicado en Base de datos "));
                    }
                }
            //}
        }



        /// <summary>
        /// Inserta el estatus que corresponde a cada fichero de la base de datos T112 del día
        /// </summary>
        /// <returns>TRUE si se insertan de forma exitosa</returns>
        public static bool InsertaEstatusFicheros(
            String prefix,
            string fecha,
            long? idFichero,
            int idEstatusFicheroApiCacao,
            int? idEstatusFichero,
            SqlConnection dbConnections)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = dbConnections;
                    cmd.CommandText = "ProcNoc_InsertaEstatusT112ApiCacao";
                    cmd.Parameters.Add(new SqlParameter("@Prefix", prefix));
                    cmd.Parameters.Add(new SqlParameter("@fechafichero", fecha));
                    cmd.Parameters.Add(new SqlParameter("@Id_estatusFicheroProcesoAPICacao", idEstatusFicheroApiCacao));

                    SqlParameter idFicheroParam = new SqlParameter("@Id_Fichero", SqlDbType.BigInt);
                    SqlParameter idEstatusFicheroParam = new SqlParameter("@id_estatusfichero", SqlDbType.Int);

                    if (idFichero == null)
                        idFicheroParam.Value = DBNull.Value;
                    else
                        idFicheroParam.Value = idFichero;


                    if (idEstatusFichero == null)
                        idEstatusFicheroParam.Value = DBNull.Value;
                    else
                        idEstatusFicheroParam.Value = idEstatusFichero;

                    cmd.Parameters.Add(idFicheroParam);
                    cmd.Parameters.Add(idEstatusFicheroParam);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();
                }


                return true;

            }

            catch (Exception ex)
            {
                Logueo.Error("InsertaEstatusFicheros() " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Consulta los registros de operaciones por compensar en la base de datos T112
        /// </summary>
        /// <returns>DataTable con los registros</returns>
        public static List<RegistroACompensar> ListaRegistrosPorCompensar(string strfechaTopeProcesamiento, SqlConnection dbConnections)
        {
            try
            {

                List<RegistroACompensar> hsetCompensar = new List<RegistroACompensar>();
                //using (SqlConnection con = new SqlConnection(BDT112.strBDLecturaArchivo))
                //{
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = dbConnections;
                        cmd.CommandText = "Proc_ObtieneRegistrosPorCompensar";
                        cmd.Parameters.Add(new SqlParameter("@fechaTopeProcesamiento", strfechaTopeProcesamiento));
                        cmd.CommandTimeout = 30;
                        cmd.CommandType = CommandType.StoredProcedure;
                        //con.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //Logueo.Evento(reader["IdFicheroDetalle"].ToString());



                                hsetCompensar.Add(new RegistroACompensar
                                {
                                    CodigoMonedaLocal = reader["CodigoMonedaLocal"] == null ? String.Empty : reader["CodigoMonedaLocal"].ToString(),
                                    CuotaIntercambio = obtieneDecimalValor(reader["CuotaIntercambio"]),
                                    //EsFicheroDuplicado = reader["EsFicheroDuplicado"] == null ? false :  Boolean.Parse(reader["EsFicheroDuplicado"].ToString()),
                                    IdFicheroDetalle = reader["IdFicheroDetalle"] == null ? 0 : Int64.Parse(reader["IdFicheroDetalle"].ToString()),
                                    //idProcesoT112ApiCacao = reader["IdProcesoT112ApiCacao"] == null ? 0 : int.Parse(reader["IdProcesoT112ApiCacao"].ToString()),
                                    ImporteCompensadoEnDolares = obtieneDecimalValor(reader["ImporteCompensadoDolar"]),
                                    ImporteCompensadoEnPesos = obtieneDecimalValor(reader["ImporteCompensadoPesos"]),
                                    ImporteCompensadoLocal = obtieneDecimalValor(reader["ImporteCompensadoLocal"]),
                                    //IVA = obtieneDecimalValor(reader["IVA"]),
                                    NumAutorizacion = reader["NumAutorizacion"] == null ? String.Empty : reader["NumAutorizacion"].ToString(),
                                    NumTarjeta = reader["NumTarjeta"] == null ? String.Empty : reader["NumTarjeta"].ToString(),
                                    T112_CodigoTx = reader["T112_CodigoTx"].ToString(),
                                    C040 = reader["C040"] == null ? string.Empty : reader["C040"].ToString().Trim(),
                                    IdFichero = Convert.ToInt64(reader["IdFichero"].ToString()),
                                    T112_Comercio = reader["T112_Comercio"] == null ? String.Empty : reader["T112_Comercio"].ToString(),
                                    T112_Ciudad = reader["T112_Ciudad"] == null ? String.Empty : reader["T112_Ciudad"].ToString(),
                                    T112_Pais = reader["T112_Pais"] == null ? String.Empty : reader["T112_Pais"].ToString(),
                                    T112_MCC = reader["T112_MCC"] == null ? String.Empty : reader["T112_MCC"].ToString(),
                                    T112_Moneda1 = reader["T112_Moneda1"] == null ? String.Empty : reader["T112_Moneda1"].ToString(),
                                    T112_Moneda2 = reader["T112_Moneda2"] == null ? String.Empty : reader["T112_Moneda2"].ToString(),
                                    T112_Referencia = reader["T112_Referencia"] == null ? String.Empty : reader["T112_Referencia"].ToString(),
                                    T112_FechaProc = reader["T112_FechaProc"] == null ? String.Empty : reader["T112_FechaProc"].ToString(),
                                    T112_FechaJuliana = reader["T112_FechaJuliana"] == null ? String.Empty : reader["T112_FechaJuliana"].ToString(),
                                    T112_FechaConsumo = reader["T112_FechaConsumo"] == null ? String.Empty : reader["T112_FechaConsumo"].ToString(),
                                    NombreFichero = reader["NombreFichero"] == null ? String.Empty : reader["NombreFichero"].ToString()
                                });
                            }
                        }
                    }
                //}

                return hsetCompensar;
            }

            catch (Exception ex)
            {
                Logueo.Error("ListaRegistrosPorCompensar() " + ex.Message);
                throw new Exception("Error al consultar las operaciones por compensar: " + ex);
            }
        }


        internal static bool ActualizaFicheroProcesado(long idFicheroActual, string claveEstatus, SqlConnection dbConnections)
        {
            try
            {
                //using (SqlConnection con = new SqlConnection(BDT112.strBDEscrituraArchivo))
                //{
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = dbConnections;
                        cmd.CommandText = "Proc_ActualizaEstatusFichero_Compensacion";
                        cmd.Parameters.Add(new SqlParameter("@IdFichero", idFicheroActual));
                        cmd.Parameters.Add(new SqlParameter("@Clave", claveEstatus));
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter paramcode = new SqlParameter("@Codigo",SqlDbType.VarChar, 5);
                        paramcode.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(paramcode);

                        SqlParameter paramMessage = new SqlParameter("@Mensaje", SqlDbType.VarChar, 5);
                        paramMessage.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(paramMessage);

                        cmd.CommandTimeout = 0;
                        //con.Open();
                        cmd.ExecuteNonQuery();

                        //Logueo.Evento(String.Format("Resultado actualizacion fichero T112 {0} {1}",cmd.Parameters["@Codigo"].Value, cmd.Parameters["@Mensaje"].Value));

                        return cmd.Parameters["@Codigo"].Value.ToString().Equals("00");

                    }


                //}
                
            }

            catch (Exception ex)
            {
                Logueo.Error("ActualizaFicheroProcesado() " + ex.Message);
                return false;
            }
        }

        internal static bool HayArchivosPorProcesar(long idFichero, string fechaProesamiento, SqlConnection dbConnections)
        {
            try
            {
                //using (SqlConnection con = new SqlConnection(BDT112.strBDLecturaArchivo))
                //{
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = dbConnections;
                        cmd.CommandText = "Proc_HayArchivosPorProcesarFichero_T112ApiCacao";
                        cmd.Parameters.Add(new SqlParameter("@IdFichero", idFichero));
                        cmd.Parameters.Add(new SqlParameter("@fechaProesamiento", fechaProesamiento));

                        cmd.CommandType = CommandType.StoredProcedure;
                        //con.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                return reader["PorProcesarEnFichero"].ToString().Equals("1");
                            }
                        }

                        throw new Exception("No se pudieron validar si hay archivos pendientes por procesar");

                    }

                //}

            }

            catch (Exception ex)
            {
                Logueo.Error("HayArchivosPorProcesar() " + ex.Message);

                throw ex;
            }
        }

        internal static bool HayTransaccionesRestantesPorFecha(string fechaProesamiento, SqlConnection dbConnections)
        {
            try
            {
                //using (SqlConnection con = new SqlConnection(BDT112.strBDLecturaArchivo))
                //{
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = dbConnections;
                    cmd.CommandText = "Proc_HayArchivosPorProcesarPorFecha_T112ApiCacao";
                    cmd.Parameters.Add(new SqlParameter("@fechaProesamiento", fechaProesamiento));

                    cmd.CommandType = CommandType.StoredProcedure;
                    //con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader["PorProcesarEnFichero"].ToString().Equals("1");
                        }
                    }

                    throw new Exception("No se pudieron validar si hay archivos pendientes por procesar");

                }

                //}

            }

            catch (Exception ex)
            {
                Logueo.Error("HayArchivosPorProcesar() " + ex.Message);

                throw ex;
            }
        }

        internal static bool ActualizaFicheroAProcesadoPorFecha(string strFechaProcesamiento, SqlConnection dbConnections, string cveEstatus)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = dbConnections;
                    cmd.CommandText = "Proc_ActualizaFicheroAProcesadoPorFecha";
                    cmd.Parameters.Add(new SqlParameter("@FechaProceso", strFechaProcesamiento));
                    cmd.Parameters.Add(new SqlParameter("@CveEstatus", cveEstatus));
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter paramcode = new SqlParameter("@Codigo", SqlDbType.VarChar, 5);
                    paramcode.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(paramcode);

                    SqlParameter paramMessage = new SqlParameter("@Mensaje", SqlDbType.VarChar, 5);
                    paramMessage.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(paramMessage);

                    cmd.CommandTimeout = 0;
                    //con.Open();
                    cmd.ExecuteNonQuery();

                    //Logueo.Evento(String.Format("Resultado actualizacion ActualizaFicheroAProcesadoPorFecha T112 {0} {1}", cmd.Parameters["@Codigo"].Value, cmd.Parameters["@Mensaje"].Value));

                    return cmd.Parameters["@Codigo"].Value.ToString().Equals("00");

                }
            }

            catch (Exception ex)
            {
                Logueo.Error("ActualizaFicheroProcesado() " + ex.Message);
                return false;
            }
        }

        public static Decimal obtieneDecimalValor(object value)
        {
            try
            {
                if (value == null)
                    return 0;

                if (String.IsNullOrEmpty(value.ToString()) || String.IsNullOrWhiteSpace(value.ToString()))
                    return 0;

                return Decimal.Parse(value.ToString());
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Actualiza el estatus de compensación de la operación en la base de datos T112
        /// </summary>
        /// <param name="idRegistro">Identificador del registro por actualizar</param>
        /// <param name="claveEstatus">Clave del estatus de compensación</param>
        public static bool ActualizaEstatusCompensacionRegistro(Int64 idRegistro, String claveEstatus, SqlConnection dbConnections)
        {
            try
            {
                //using (SqlConnection con = new SqlConnection(BDT112.strBDEscrituraArchivo))
                //{
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = dbConnections;
                        cmd.CommandText = "Proc_ActualizaEstatusCompensacionRegistro";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@IdFicheroDetalle", idRegistro));
                        cmd.Parameters.Add(new SqlParameter("@ClaveEstatus", claveEstatus));

                        SqlParameter paramcode = new SqlParameter("@Codigo", SqlDbType.VarChar, 5);
                        paramcode.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(paramcode);

                        SqlParameter paramMessage = new SqlParameter("@Mensaje", SqlDbType.VarChar, 5);
                        paramMessage.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(paramMessage);

                        //con.Open();
                        cmd.ExecuteNonQuery();

                        //Logueo.Evento(String.Format("Resultado de la actualización del FiccheroDetalle(Transacción en T112) {0} - {1}",
                        //    cmd.Parameters["@Codigo"].Value.ToString(),
                        //    cmd.Parameters["@Mensaje"].Value.ToString()));

                        var resp = cmd.Parameters["@Codigo"].Value;

                        return cmd.Parameters["@Codigo"].Value.ToString().Equals("00");

                    }
                //}
            }

            catch (Exception ex)
            {
                Logueo.Error("ActualizaEstatusCompensacionRegistro()|Error al actualizar el estatus del registro con ID: " + idRegistro.ToString() + ex.Message);
                return false;
            }
        }

        internal static void ActualizaProcesoT112ApiCacao(long idFicheroActual, string claveEstatus, SqlConnection dbConnections)
        {
            try
            {
                //using (SqlConnection con = new SqlConnection(BDT112.strBDEscrituraArchivo))
                //{
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = dbConnections;
                        cmd.CommandText = "Proc_ActualizaEstatusProcesoT112ApiCacao";
                        cmd.Parameters.Add(new SqlParameter("@IdFichero", idFicheroActual));
                        cmd.Parameters.Add(new SqlParameter("@Clave", claveEstatus));
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter paramcode = new SqlParameter("@Codigo", SqlDbType.VarChar, 5);
                        paramcode.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(paramcode);

                        SqlParameter paramMessage = new SqlParameter("@Mensaje", SqlDbType.VarChar, 200);
                        paramMessage.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(paramMessage);

                        cmd.CommandTimeout = 0;
                        //con.Open();
                        cmd.ExecuteNonQuery();

                        Logueo.Evento(String.Format("Resultado actualizacion ProcesoT112ApiCacao {0} {1}", cmd.Parameters["@Codigo"].Value, cmd.Parameters["@Mensaje"].Value));

                    }

                //}

            }

            catch (Exception ex)
            {
                Logueo.Error("ActualizaProcesoT112ApiCacao() " + ex.Message);
            }
        }

        internal static string ObtenerFechaProcesamientoMedianteTopeConfigurado(string fechaProcesamiento, string fechaFinal, SqlConnection dbConnections)
        {
            try
            {
                //using (SqlConnection con = new SqlConnection(BDT112.strBDLecturaArchivo))
                //{
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = dbConnections;
                        cmd.CommandText = "Proc_ObtieneFechaMinimaProcesamientoConsiliacionT112";
                        cmd.Parameters.Add(new SqlParameter("@FechaProcesamiento", fechaProcesamiento));
                        cmd.Parameters.Add(new SqlParameter("@FechaTope", fechaFinal));
                        cmd.CommandType = CommandType.StoredProcedure;

                    

                        cmd.CommandTimeout = 0;
                        //con.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                return reader["FechaProceso"].ToString();
                            }
                        }

                    //return DateTime.Now.ToString("yyyy-MM-dd");
                    return string.Empty;
                    }

                //}

            }

            catch (Exception ex)
            {
                Logueo.Error(String.Format("ObtenerFechaProcesamientoMedianteTopeConfigurado() - {0} ",ex.Message));
                throw ex;
            }
        }
       
    }
}
