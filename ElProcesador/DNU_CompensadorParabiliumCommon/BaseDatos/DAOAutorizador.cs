using DNU_CompensadorParabiliumCommon.Constants;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Utilidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    public static class DAOAutorizador
    {

        public static void insertaDevolucion(DevolucionModel dev, SqlConnection conn, SqlTransaction tran)
        {
            try
            {

                SqlCommand command = new SqlCommand("ProcNoct_COMP_InsertaDevolucion", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;

                command.Parameters.Add(new SqlParameter("@Tipo", dev.tipo));
                //  command.Parameters.Add(new SqlParameter("@ClaveMA", dev.claveMa));

                SqlParameter paramSSN = command.CreateParameter();
                paramSSN.ParameterName = "@ClaveMA";
                paramSSN.DbType = DbType.AnsiStringFixedLength;
                paramSSN.Direction = ParameterDirection.Input;
                paramSSN.Value = dev.claveMa;
                paramSSN.Size = dev.claveMa.Length;
                command.Parameters.Add(paramSSN);

                command.Parameters.Add(new SqlParameter("@Referencia", dev.referencia));
                command.Parameters.Add(new SqlParameter("@Importe", dev.importe));
                command.Parameters.Add(new SqlParameter("@Moneda", dev.moneda));
                command.Parameters.Add(new SqlParameter("@Autorizacion", dev.autorizacion));
                command.Parameters.Add(new SqlParameter("@ID_FicheroDetalle", dev.ID_FicheroDetalle));
                command.Parameters.Add(new SqlParameter("@ID_Operacion", dev.ID_Operacion));

                command.Parameters.Add(new SqlParameter("@ID_EstatusOperacion", dev.ID_EstatusOperacion));
                command.Parameters.Add(new SqlParameter("@ID_EstatusPostOperacion", dev.ID_EstatusPostOperacion));
                command.Parameters.Add(new SqlParameter("@ID_Poliza", dev.ID_Poliza));

                command.ExecuteNonQuery();



            }
            catch (Exception err)
            {
                Log.Error(err);
                throw err;
            }

        }

        public static RespuestaValidacionOperacionReversada ValidaOperacionCopia(string claveMA, long idOperacionReversada, string numeroAutorizacion, SqlConnection conn)
        {
            var response = new RespuestaValidacionOperacionReversada { CodigoRespuesta = ConstantesCompensacion.CodigoRespuestaError };
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.CommandText = "PROCNOC_COMP_ObtieneOperacionEnTransitoDeReversada";
                    cmd.Parameters.Add(new SqlParameter("@ID_OperacionReversada", idOperacionReversada));
                    cmd.Parameters.Add(new SqlParameter("@ClaveMA", claveMA));
                    cmd.Parameters.Add(new SqlParameter("@Autorizacion", numeroAutorizacion));

                    using (var read = cmd.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            response.CodigoRespuesta = ConstantesCompensacion.CodigoRespuestaAutorizado;
                            response.IdOperacion = Convert.ToInt64(read["ID_Operacion"].ToString());
                            response.IdEstatusOperacion = Convert.ToInt32(read["ID_Estatus"].ToString());

                            var temp = read["ID_EstatusPostOperacion"].ToString();

                            response.IdEstatusPostOperacion = !String.IsNullOrEmpty(read["ID_EstatusPostOperacion"].ToString()) ? Convert.ToInt32(read["ID_EstatusPostOperacion"].ToString()) : 0;

                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(new Exception("[ValidaOperacionCopia] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]", ex));
                return response;
            }

        }

        public static void ActualizaEstatusDevolucion(DevolucionModel dev, SqlConnection conn, SqlTransaction tran)
        {
            try
            {

                SqlCommand command = new SqlCommand("ProcNoct_COMP_ActualizaEstatusDevolucion", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;

                command.Parameters.Add(new SqlParameter("@ID_Devolucion", dev.ID_Devolucion));
                command.Parameters.Add(new SqlParameter("@Estatus", dev.Estatus));
                command.Parameters.Add(new SqlParameter("@ID_Poliza", dev.ID_Poliza));

                command.ExecuteNonQuery();



            }
            catch (Exception err)
            {
                Log.Error(err);
                throw err;
            }

        }



        public static Boolean EsSaldoInterno(String ClaveMA, SqlConnection conn)
        {
            Boolean saldosInternos = false;
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.CommandText = "PROCNOC_Parabilia_ValidaOperacionSaldosInternos";
                    cmd.Parameters.Add(new SqlParameter("@ClaveMA", ClaveMA));

                    var codResultado = new SqlParameter("@Resultado", SqlDbType.VarChar, 5);
                    codResultado.Value = "";
                    codResultado.Direction = ParameterDirection.Output;

                    cmd.Parameters.Add(codResultado);


                    cmd.ExecuteNonQuery();
                    saldosInternos = cmd.Parameters["@Resultado"].Value.ToString().Equals("1");
                }
            }
            catch (Exception ex)
            {
                Log.Error(new Exception("[EsSaldoExterno] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]", ex));
                throw ex;
            }

            return saldosInternos;

        }


        public static List<DevolucionModel> ObtieneDevoluciones(int ID_TipoIntegracion, SqlConnection conn)
        {
            List<DevolucionModel> lstDevoluciones = new List<DevolucionModel>();
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.CommandText = "PROCNOC_COMP_ObtieneRegistrosRecursosADevolver";
                    cmd.Parameters.Add(new SqlParameter("@ID_TipoIntegracion", ID_TipoIntegracion));

                    using (var read = cmd.ExecuteReader())
                    {
                        int? defaultint = null;
                        while (read.Read())
                        {
                            if (!lstDevoluciones.Exists(d => d.ID_Devolucion == Convert.ToInt64(read["ID_Devolucion"])))
                            {
                                lstDevoluciones.Add(new DevolucionModel
                                {
                                    autorizacion = read["autorizacion"].ToString(),
                                    claveMa = read["claveMa"].ToString(),
                                    Estatus = read["Estatus"] != null ? Convert.ToInt32(read["Estatus"].ToString()) : defaultint,
                                    moneda = read["Moneda"].ToString(),
                                    referencia = read["Referencia"].ToString(),
                                    importe = Convert.ToDecimal(read["Importe"]),
                                    tipo = read["tipo"].ToString(),
                                    ID_Operacion = Convert.ToInt64(read["ID_Operacion"]),
                                    ID_Devolucion = Convert.ToInt64(read["ID_Devolucion"]),
                                    ID_Evento = Convert.ToInt64(read["ID_Evento"]),
                                    ClaveEvento = read["ClaveEvento"].ToString(),
                                    ID_EstatusOperacion = Convert.ToInt32(read["ID_Estatus"].ToString()),
                                    ID_EstatusPostOperacion = Convert.ToInt32(read["ID_EstatusPostOperacion"].ToString()),
                                    ClaveColectiva = read["ClaveColectiva"].ToString(),
                                    FechaOperacion = read["FechaOperacion"].ToString(),
                                });
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(new Exception("[ObtieneDevoluciones] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]", ex));

            }

            return lstDevoluciones;

        }


        public static List<int> ObtieneTiposIntegracion(SqlConnection conn)
        {
            List<int> lstTiposIntegracion = new List<int>();
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.CommandText = "PROCNOC_COMP_ObtieneTiposIntegracion";

                    using (var read = cmd.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            lstTiposIntegracion.Add(Convert.ToInt32(read["ID_TipoIntegracion"]));
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(new Exception("[ObtieneTiposIntegracion] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]", ex));

            }

            return lstTiposIntegracion;

        }

        public static RespDevSinCompensar getValidaDevolucion(string evento, string idOperacion, SqlConnection conn, SqlTransaction tran)
        {
            var response = new RespDevSinCompensar();
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Transaction = tran;
                    cmd.CommandText = "Procnoc_Comp_EventoDevolucionSinCompensar";
                    cmd.Parameters.Add(new SqlParameter("@cvEvento", evento));
                    cmd.Parameters.Add(new SqlParameter("@idOperacion", idOperacion));

                    using (var read = cmd.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            response.idEvento = read["Evento"].ToString();
                            response.ImporteReal = read["importeReal"].ToString();
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(new Exception("[ValidaDevolucionSinCompensar] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]", ex));
                return response;
            }

        }

        public static void getValidaDevolucion(string evento, int idOperacion, SqlConnection conn, SqlTransaction tran)
        {
            try
            {

                SqlCommand command = new SqlCommand("Procnoc_Comp_EventoDevolucionSinCompensar", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;

                command.Parameters.Add(new SqlParameter("@cvEvento", evento));
                command.Parameters.Add(new SqlParameter("@idOperacion", idOperacion));

                command.ExecuteNonQuery();



            }
            catch (Exception err)
            {
                Log.Error(err);
                throw err;
            }

        }

        #region CompensadorV6
        public static void InsertarRegistroComp(RegistroCompensar registroCompensar, Dictionary<String, Parametro> lstParametros
                                , SqlConnection conn, SqlTransaction tran, string etiquetaLogueo, string idPolizas, string cveEstatusDestinoOp
                                , string cveVariante, ref bool insertaRegistroValido, ref string repuestaError)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {

                SqlCommand command = new SqlCommand("PN_CMP_InsertaRegistroCompensacion", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;

                command.Parameters.Add(new SqlParameter("@idFicheroDetalle", registroCompensar.idFicheroDetalle));
                command.Parameters.Add(new SqlParameter("@idOperacion", lstParametros["@idOperacion"].Valor));
                command.Parameters.Add(new SqlParameter("@cveEstatus_Destino", cveEstatusDestinoOp));
                command.Parameters.Add(new SqlParameter("@cveVariante", cveVariante));
                command.Parameters.Add(new SqlParameter("@codigoProceso", registroCompensar.codigoProceso));
                command.Parameters.Add(new SqlParameter("@codigoFuncion", registroCompensar.codigoFuncion));
                command.Parameters.Add(new SqlParameter("@reverso", registroCompensar.reverso));
                command.Parameters.Add(new SqlParameter("@representacion", registroCompensar.representacion));
                command.Parameters.Add(new SqlParameter("@bin", registroCompensar.bin));
                //For parameters encrypt
                SqlParameter paramSSN = command.CreateParameter();
                paramSSN.ParameterName = "@tarjeta";
                paramSSN.DbType = DbType.AnsiStringFixedLength;
                paramSSN.Direction = ParameterDirection.Input;
                paramSSN.Value = registroCompensar.tarjeta;
                paramSSN.Size = registroCompensar.tarjeta.Length;
                command.Parameters.Add(paramSSN);

                command.Parameters.Add(new SqlParameter("@tarjetaEnmascarada", registroCompensar.tarjetaEnmascarada));
                command.Parameters.Add(new SqlParameter("@autorizacion", registroCompensar.autorizacion));
                command.Parameters.Add(new SqlParameter("@referencia", registroCompensar.referencia));
                command.Parameters.Add(new SqlParameter("@referencia2", registroCompensar.referencia2));
                command.Parameters.Add(new SqlParameter("@impOrigen", registroCompensar.impOrigen));
                command.Parameters.Add(new SqlParameter("@cveDivisaOrigen", registroCompensar.cveDivisaOrigen));
                command.Parameters.Add(new SqlParameter("@impDestino", registroCompensar.impDestino));
                command.Parameters.Add(new SqlParameter("@cveDivisaDestino", registroCompensar.cveDivisaDestino));
                command.Parameters.Add(new SqlParameter("@impCuotaIntercambio", registroCompensar.impCuotaIntercambio));
                command.Parameters.Add(new SqlParameter("@cveDivisaCuotaIntercambio", registroCompensar.cveDivisaCuotaIntercambio));
                command.Parameters.Add(new SqlParameter("@impIva", registroCompensar.impIva));
                command.Parameters.Add(new SqlParameter("@cveDivisaIva", registroCompensar.cveDivisaIva));
                command.Parameters.Add(new SqlParameter("@impMCCR", registroCompensar.impMCCR));
                command.Parameters.Add(new SqlParameter("@cveDivisaMCCR", registroCompensar.cveDivisaMCCR));
                command.Parameters.Add(new SqlParameter("@impMarkUp", registroCompensar.impMarkUp));
                command.Parameters.Add(new SqlParameter("@cveDivisaMarkUp", registroCompensar.cveDivisaMarkUp));
                command.Parameters.Add(new SqlParameter("@impCompensacion", registroCompensar.impCompensacion));
                command.Parameters.Add(new SqlParameter("@cveDivisaCompensacion", registroCompensar.cveDivisaCompensacion));

                SqlParameter param4 = new SqlParameter("@fechaOperacion", SqlDbType.Date);
                param4.Value = registroCompensar.fechaOperacion;
                command.Parameters.Add(param4);

                command.Parameters.Add(new SqlParameter("@horaOperacion", registroCompensar.horaOperacion));
                command.Parameters.Add(new SqlParameter("@fechaJuliana", registroCompensar.fechaJuliana));

                SqlParameter param5 = new SqlParameter("@fechaPresentacion", SqlDbType.Date);
                param5.Value = registroCompensar.fechaPresentacion;
                command.Parameters.Add(param5);

                command.Parameters.Add(new SqlParameter("@comercio", registroCompensar.comercio));
                command.Parameters.Add(new SqlParameter("@comercioCiudad", registroCompensar.comercioCiudad));
                command.Parameters.Add(new SqlParameter("@comercioPais", registroCompensar.comercioPais));
                command.Parameters.Add(new SqlParameter("@comercioMCC", registroCompensar.comercioMCC));
                command.Parameters.Add(new SqlParameter("@nombreFichero", registroCompensar.fichero));
                command.Parameters.Add(new SqlParameter("@ciclo", registroCompensar.ciclo));
                command.Parameters.Add(new SqlParameter("@idPolizas", idPolizas));
                command.Parameters.Add(new SqlParameter("@cveTipoRed", registroCompensar.cveTipoRed));

                command.Parameters.Add(new SqlParameter("@tipoCambio", registroCompensar.tipoCambio));
                command.Parameters.Add(new SqlParameter("@codigoPostal", registroCompensar.codigoPostal));
                command.Parameters.Add(new SqlParameter("@estado", registroCompensar.estado));
                command.Parameters.Add(new SqlParameter("@importeOriginal", registroCompensar.importeOriginal));
                command.Parameters.Add(new SqlParameter("@importeComisiones", registroCompensar.importeComisiones));
                command.Parameters.Add(new SqlParameter("@exponentesMoneda", registroCompensar.exponentesMoneda));
                command.Parameters.Add(new SqlParameter("@referenciaEmisor", registroCompensar.referenciaEmisor));
                command.Parameters.Add(new SqlParameter("@identificadorMensaje", registroCompensar.identificadorMensaje));
                command.Parameters.Add(new SqlParameter("@transaccionDestino", registroCompensar.transaccionDestino));
                command.Parameters.Add(new SqlParameter("@transaccionOrigen", registroCompensar.transaccionOrigen));
                command.Parameters.Add(new SqlParameter("@codigoAdquirente", registroCompensar.codigoAdquirente));
                command.Parameters.Add(new SqlParameter("@codigoDocumento", registroCompensar.codigoDocumento));
                command.Parameters.Add(new SqlParameter("@codigoRazonMensaje", registroCompensar.codigoRazonMensaje));
                command.Parameters.Add(new SqlParameter("@actividadNegocio", registroCompensar.actividadNegocio));
                command.Parameters.Add(new SqlParameter("@modoEntradaPos", registroCompensar.modoEntradaPos));
                command.Parameters.Add(new SqlParameter("@tipoTerminal", registroCompensar.tipoTerminal));
                command.Parameters.Add(new SqlParameter("@numeroControl", registroCompensar.numeroControl));
                command.Parameters.Add(new SqlParameter("@indicadorLiquidacion", registroCompensar.indicadorLiquidacion));
                command.Parameters.Add(new SqlParameter("@indicadorDocumentacion", registroCompensar.indicadorDocumentacion));

                command.Parameters.Add(new SqlParameter("@identificadorTransaccion", registroCompensar.identificadorTransaccion));

                SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                param2.Direction = ParameterDirection.Output;
                command.Parameters.Add(param2);

                SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                param3.Direction = ParameterDirection.Output;
                command.Parameters.Add(param3);


                command.ExecuteNonQuery();

                respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                LogueoSPS(respuestaGral, etiquetaLogueo);

                if (EvaluarRespuestaSPS(respuestaGral))
                    insertaRegistroValido = true;
                else
                    repuestaError = respuestaGral.DescRespuesta;

            }
            catch (Exception ex)
            {
                repuestaError = "[{{\"responseCode\":\"406\", \"responseDate\":\"" +
                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                                "\", \"responseMessage\":\"" + ex.Message + "\", \"objectName\":\"InsertarRegistroComp\", " +
                                                "\"objectVersion\":\"1.0.0.0\", \"objectType\":Método\"}}]";
                Log.Error(etiquetaLogueo + $"{NombreMetodo()}: " + ex.Message);
            }

        }

        private static void LogueoSPS(RespuestaGral respuestaGral, string etiquetaLogueo)
        {
            int valCodigo = Convert.ToInt32(respuestaGral.CodRespuesta);
            if (valCodigo >= 500 && valCodigo <= 599)
                Log.Error(etiquetaLogueo + "[" + respuestaGral.DescRespuesta + "]");
            else
                Log.Evento(etiquetaLogueo + "[" + respuestaGral.DescRespuesta + "]");

        }

        /// <summary>
        /// Devuelve el nombre del metodo que llamo a este metodo, ejemplo el metodo consultaDato llama a este metodo el resultado es: consultaDato
        /// </summary>
        /// <returns>Nombre del metodo que lo invocó</returns>
        private static string NombreMetodo([CallerMemberName] string metodo = null)
        {
            return metodo;
        }

        private static bool EvaluarRespuestaSPS(RespuestaGral respuestaGral)
        {
            try
            {
                int valCodigo = Convert.ToInt32(respuestaGral.CodRespuesta);
                if (valCodigo >= 200 && valCodigo <= 299)
                    return true;       //Regresa true ya que es una respuesta exitosa
                else
                    return false;       //Regresa false ya que es una respuesta de error
            }
            catch (Exception e)
            {
                return false;           //Regresa false por que hubo un error en la respuesta
            }

        }

        #endregion
    }
}
