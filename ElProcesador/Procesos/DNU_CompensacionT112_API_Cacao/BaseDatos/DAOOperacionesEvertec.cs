using CommonProcesador;
using DNU_CompensacionT112_API_Cacao.BaseDatos;
using DNU_CompensacionT112_API_Cacao.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DNU_CompensacionT112Evertec.BaseDatos
{
    public class DAOOperacionesEvertec
    {

        //static BDOperacionesConnections dbConnections = new BDOperacionesConnections();

        /// <summary>
        /// Realiza la compensación de la operación en la BD de operaciones Evertec
        /// </summary>
        /// <param name="unRegistro">Registro con los datos de la operación</param>
        /// <returns>Cadena con la respuesta de base de datos o con el mensaje de error</returns>
        public static string CompensaOperacion(RegistroACompensar unRegistro, SqlConnection dbConnections)
        {
            string resp = String.Empty;

            try
            {
                //using (SqlConnection con = new SqlConnection(BDOperacionesEvertec.strBDEscrituraArchivo))
                //{
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = dbConnections;
                        command.CommandText = "Proc_CompensaT112_API_CACAO";
                        command.CommandType = CommandType.StoredProcedure;
                      

                       // command.Parameters.Add(new SqlParameter("@EsFicheroDuplicado", unRegistro.EsFicheroDuplicado));
                        command.Parameters.Add(new SqlParameter("@NumAutorizacion", unRegistro.NumAutorizacion));
                        command.Parameters.Add(new SqlParameter("@NumTarjeta", unRegistro.NumTarjeta));
                        command.Parameters.Add(new SqlParameter("@ImporteCompensadoPesos", unRegistro.ImporteCompensadoEnPesos));
                        command.Parameters.Add(new SqlParameter("@ImporteCompensadoDolar", unRegistro.ImporteCompensadoEnDolares));
                        command.Parameters.Add(new SqlParameter("@ImporteCompensadoLocal", unRegistro.ImporteCompensadoLocal));
                        command.Parameters.Add(new SqlParameter("@CodigoMonedaLocal", unRegistro.CodigoMonedaLocal));
                        command.Parameters.Add(new SqlParameter("@CuotaIntercambio", unRegistro.CuotaIntercambio));
                        command.Parameters.Add(new SqlParameter("@T112_CodigoTx", unRegistro.T112_CodigoTx));
                        command.Parameters.Add(new SqlParameter("@T112_Comercio", unRegistro.T112_Comercio));
                        command.Parameters.Add(new SqlParameter("@T112_Ciudad", unRegistro.T112_Ciudad));
                        command.Parameters.Add(new SqlParameter("@T112_Pais", unRegistro.T112_Pais));
                        command.Parameters.Add(new SqlParameter("@T112_MCC", unRegistro.T112_MCC));
                        command.Parameters.Add(new SqlParameter("@T112_Moneda1", unRegistro.T112_Moneda1));
                        command.Parameters.Add(new SqlParameter("@T112_Moneda2", unRegistro.T112_Moneda2));
                        command.Parameters.Add(new SqlParameter("@T112_Referencia", unRegistro.T112_Referencia));
                        command.Parameters.Add(new SqlParameter("@T112_FechaProc", unRegistro.T112_FechaProc));
                        command.Parameters.Add(new SqlParameter("@T112_FechaJuliana", unRegistro.T112_FechaJuliana));
                        command.Parameters.Add(new SqlParameter("@T112_FechaConsumo", unRegistro.T112_FechaConsumo));
                        command.Parameters.Add(new SqlParameter("@T112_FechaPresentacion", unRegistro.T112_FechaPresentacion));
                        command.Parameters.Add(new SqlParameter("@T112_Ciclo", unRegistro.T112_Ciclo));

                        //command.Parameters.Add(new SqlParameter("@IVA", unRegistro.IVA));



                        var sqlParameter = new SqlParameter("@Codigo", SqlDbType.VarChar);
                        sqlParameter.Direction = ParameterDirection.Output;
                        sqlParameter.Size = 5;
                        command.Parameters.Add(sqlParameter);

                        var paramMessage = new SqlParameter("@Mensaje", SqlDbType.VarChar);
                        paramMessage.Direction = ParameterDirection.Output;
                        paramMessage.Size = 200;
                        command.Parameters.Add(paramMessage);

                        //con.Open();

                        command.CommandTimeout = 5;
                        command.ExecuteNonQuery();

                        resp = command.Parameters["@Codigo"].Value.ToString();

                        //Logueo.Evento(String.Format("Estatus de la compensación {0}  - {1}",
                        //    command.Parameters["@Codigo"].Value.ToString(),
                        //    command.Parameters["@Mensaje"].Value.ToString()));
                    }
                //}
            }

            catch (Exception ex)
            {
                resp = "Error al compensar la operación con ID: " + unRegistro.IdFicheroDetalle.ToString();
                Logueo.Error("CompensaOperacion()|" + resp + "|" + ex.Message);
            }

            return resp;
        }

        internal static List<ArchivoOperaciones> ObtieneArchivosOperacionesPorMes(string fecha, SqlConnection dbConnections)
        {
            List<ArchivoOperaciones> ficheros = new List<ArchivoOperaciones>();
            try
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = dbConnections;
                    command.CommandText = "ProcNoc_ObtieneArchivosOperacionesPorMes";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@fecha", fecha));

                    //con.Open();

                    command.CommandTimeout = 5;
                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            ficheros.Add(new ArchivoOperaciones
                            {
                                ID_Estatus = reader["ID_EstatusArchivo"].ToString(),
                                Nombre = reader["NombreArchivo"].ToString(),
                            });
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Logueo.Error(String.Format("VerificaRegistroExistenteEnOperaciones()| {0}", ex.Message));
                throw ex;
            }

            return ficheros;
        }

        internal static OperacionEvertec VerificaRegistroExistenteEnOperaciones(RegistroACompensar unRegistro, SqlConnection dbConnections)
        {
            OperacionEvertec opEt = new OperacionEvertec();

            try
            {
                //using (SqlConnection con = new SqlConnection(BDOperacionesEvertec.strBDEscrituraArchivo))
                //{
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = dbConnections;
                        command.CommandText = "Proc_VerificaEstatusTrx";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@NumAutorizacion", unRegistro.NumAutorizacion));
                        command.Parameters.Add(new SqlParameter("@NumTarjeta", unRegistro.NumTarjeta));

                        //con.Open();

                        command.CommandTimeout = 5;
                        using (var reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                opEt.EstatusAutorizacion = reader["Clave"].ToString();
                                opEt.EsCompensada = reader["Compensada"] == null ? false : reader["Compensada"].ToString() == "1" ? true : false;
                            }
                        }
                    }
                //}
            }

            catch (Exception ex)
            {
                Logueo.Error(String.Format("VerificaRegistroExistenteEnOperaciones()| {0}" , ex.Message));
                throw ex;
            }

            return opEt;
        }

        internal static string LlenaCamposDevolucion(RegistroACompensar unRegistro, SqlConnection dbConnections)
        {
            string resp = String.Empty;

            try
            {
                //using (SqlConnection con = new SqlConnection(BDOperacionesEvertec.strBDEscrituraArchivo))
                //{
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = dbConnections;
                        command.CommandText = "Proc_Compensa_TX20_T112_API_CACAO";
                        command.CommandType = CommandType.StoredProcedure;


                        command.Parameters.Add(new SqlParameter("@NumAutorizacion", unRegistro.NumAutorizacion));
                        command.Parameters.Add(new SqlParameter("@NumTarjeta", unRegistro.NumTarjeta));
                        command.Parameters.Add(new SqlParameter("@ImporteCompensadoPesos", unRegistro.ImporteCompensadoEnPesos));
                        command.Parameters.Add(new SqlParameter("@ImporteCompensadoDolar", unRegistro.ImporteCompensadoEnDolares));
                        command.Parameters.Add(new SqlParameter("@ImporteCompensadoLocal", unRegistro.ImporteCompensadoLocal));
                        command.Parameters.Add(new SqlParameter("@CodigoMonedaLocal", unRegistro.CodigoMonedaLocal));
                        command.Parameters.Add(new SqlParameter("@CuotaIntercambio", unRegistro.CuotaIntercambio));
                        command.Parameters.Add(new SqlParameter("@T112_CodigoTx", unRegistro.T112_CodigoTx));
                        command.Parameters.Add(new SqlParameter("@T112_Comercio", unRegistro.T112_Comercio));
                        command.Parameters.Add(new SqlParameter("@T112_Ciudad", unRegistro.T112_Ciudad));
                        command.Parameters.Add(new SqlParameter("@T112_Pais", unRegistro.T112_Pais));
                        command.Parameters.Add(new SqlParameter("@T112_MCC", unRegistro.T112_MCC));
                        command.Parameters.Add(new SqlParameter("@T112_Moneda1", unRegistro.T112_Moneda1));
                        command.Parameters.Add(new SqlParameter("@T112_Moneda2", unRegistro.T112_Moneda2));
                        command.Parameters.Add(new SqlParameter("@T112_Referencia", unRegistro.T112_Referencia));
                        command.Parameters.Add(new SqlParameter("@T112_FechaProc", unRegistro.T112_FechaProc));
                        command.Parameters.Add(new SqlParameter("@T112_FechaJuliana", unRegistro.T112_FechaJuliana));
                        command.Parameters.Add(new SqlParameter("@T112_FechaConsumo", unRegistro.T112_FechaConsumo));
                        command.Parameters.Add(new SqlParameter("@T112_FechaPresentacion", unRegistro.T112_FechaPresentacion));
                        command.Parameters.Add(new SqlParameter("@T112_Ciclo", unRegistro.T112_Ciclo));



                        var sqlParameter = new SqlParameter("@Codigo", SqlDbType.VarChar);
                        sqlParameter.Direction = ParameterDirection.Output;
                        sqlParameter.Size = 5;
                        command.Parameters.Add(sqlParameter);

                        var paramMessage = new SqlParameter("@Mensaje", SqlDbType.VarChar);
                        paramMessage.Direction = ParameterDirection.Output;
                        paramMessage.Size = 200;
                        command.Parameters.Add(paramMessage);

                        //con.Open();

                        command.CommandTimeout = 5;
                        command.ExecuteNonQuery();

                        resp = command.Parameters["@Codigo"].Value.ToString();

                        //Logueo.Evento(String.Format("Estatus de la compensación por codigo 20 {0}  - {1}",
                        //    command.Parameters["@Codigo"].Value.ToString(),
                        //    command.Parameters["@Mensaje"].Value.ToString()));
                    }
                //}
            }

            catch (Exception ex)
            {
                resp = "Error al compensar la operación con ID: " + unRegistro.IdFicheroDetalle.ToString();
                Logueo.Error("CompensaOperacion()|" + resp + "|" + ex.Message);
            }

            return resp;
        }
    }
}
