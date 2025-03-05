using CommonProcesador;
using DNU_ParabiliaAsignarLimiteCredito.Entidades;
using DNU_ParabiliaAsignarLimiteCredito.Utilidades;
using DocumentFormat.OpenXml.Wordprocessing;
using Executer.Entidades;
using Interfases.Entidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAsignarLimiteCredito.BaseDatos
{
    class DAODatosBase
    {


        public static Dictionary<String, Parametro> ObtenerDatosParametros(Bonificacion elAbono, DataTable datosDiccionario, Response respuesta, string ip, string log, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {

                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                losParametros = ListaDeParamentrosContrato(elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "", respuesta, ip, log, conn, transaccionSQL);
        
                if (string.IsNullOrEmpty(elAbono.Concepto))
                {
                    if (elAbono.cuentaOrigen == null)
                    {
                        elAbono.Concepto = losParametros.Values.ElementAt(0).Valor;
                    }
                    else
                    {
                        try
                        {
                            string cuentas = ": Origen:***" + elAbono.cuentaOrigen.Substring((elAbono.cuentaOrigen.Length - 5), 5) + " Destino:***" + elAbono.cuentaDestino.Substring((elAbono.cuentaDestino.Length - 5), (5));
                            elAbono.Concepto = losParametros.Values.ElementAt(0).Valor + cuentas;
                        }
                        catch (Exception ex)
                        {
                            elAbono.Concepto = losParametros.Values.ElementAt(0).Valor;
                        }
                    }

                }
                if (string.IsNullOrEmpty(elAbono.Observaciones))
                {
                    elAbono.Observaciones = "";
                }
                if (elAbono.RefNumerica == null)
                {
                    elAbono.RefNumerica = 0;
                }

                foreach (DataRow fila in datosDiccionario.Rows)
                {
                    if (!string.IsNullOrEmpty(fila["idTipocolectiva"].ToString()))
                    {
                        losParametros[fila["nombre"].ToString()] = new Parametro()
                        {
                            Nombre = fila["nombre"].ToString(),
                            Valor = fila["valor"].ToString(),
                            Descripcion = fila["descripcion"].ToString(),
                            ID_TipoColectiva = Convert.ToInt32(fila["idTipocolectiva"].ToString())
                        };
                    }
                    else
                    {
                        losParametros[fila["nombre"].ToString()] = new Parametro()
                        {
                            Nombre = fila["nombre"].ToString(),
                            Valor = fila["valor"].ToString(),
                            Descripcion = fila["descripcion"].ToString(),
                        };


                    }

                }

                return losParametros;

            }

            catch (Exception err)
            {
                //transaccionSQLTraspaso.Rollback();
                throw err;
            }
        }

        public static Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta
            , String ClaveEvento, String elUsuario, Response respuesta, string ip, string log, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_ObtieneValoresContratos]");
                SqlCommand query = new SqlCommand("ws_Ejecutor_ObtieneValoresContratos", conn, transaccionSQL);
                query.CommandType = CommandType.StoredProcedure;
                query.Parameters.AddWithValue("@ClaveCadenaComercial", ClaveCadenaComercial);
                query.Parameters.AddWithValue("@ClaveEvento", ClaveEvento);

                DbParameter paramSSN = query.CreateParameter();
                paramSSN.ParameterName = "@Tarjeta";
                paramSSN.DbType = DbType.AnsiStringFixedLength;
                paramSSN.Direction = ParameterDirection.Input;
                paramSSN.Value = Tarjeta.TrimEnd(' ');
                paramSSN.Size = 50;
                query.Parameters.Add(paramSSN);

                SqlDataAdapter sda = new SqlDataAdapter(query);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                DataTable Respuesta = opcional.Tables["Table"];

                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();


                for (int k = 0; k < Respuesta.Rows.Count; k++)
                {
                    Parametro unParamentro = new Parametro();
                    unParamentro.Nombre = (Respuesta.Rows[k]["Nombre"]).ToString();
                    unParamentro.ID_TipoColectiva = Int16.Parse((Respuesta.Rows[k]["ID_TipoColectiva"]).ToString());
                    unParamentro.Valor = (Respuesta.Rows[k]["valor"]).ToString();
                    larespuesta.Add(unParamentro.Nombre, unParamentro);

                }

                return larespuesta;
            }
            catch (Exception ex)
            {
                respuesta.CodRespuesta = "9091";
                respuesta.DescRespuesta = "Error al obtener los contratos";
                throw ex;
            }
        }

        public static DataSet obtenerDatosTarjetaEventoAsigLimCreditoParaExecute(string numeroTarjeta, string claveColectiva
            , string limiteCredito, SqlConnection pConn, string ip, string log, string cuentaOrigen)
        {

            LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Parabilium_ObtenerDatosTarjetaAsignaLimiteCredito]");
            SqlCommand query = new SqlCommand("ProcNoct_ObtenerDatosLimiteCredito", pConn);
            query.CommandType = CommandType.StoredProcedure;

            DbParameter paramSSN = query.CreateParameter();
            paramSSN.ParameterName = "@tarjeta";
            paramSSN.DbType = DbType.AnsiStringFixedLength;
            paramSSN.Direction = ParameterDirection.Input;
            paramSSN.Value = numeroTarjeta.TrimEnd(' ');
            paramSSN.Size = 50;
            query.Parameters.Add(paramSSN);

            query.Parameters.AddWithValue("@claveEvento", PNConfig.Get("ASIGNALIMITECREDITO", "eventoAsignaLimiteCredito"));
            query.Parameters.AddWithValue("@clavePadre", claveColectiva);
            query.Parameters.AddWithValue("@importe", limiteCredito);

            DbParameter paramSSN_CtaOrigen = query.CreateParameter();
            paramSSN_CtaOrigen.ParameterName = "@cuentaOrigen";
            paramSSN_CtaOrigen.DbType = DbType.AnsiStringFixedLength;
            paramSSN_CtaOrigen.Direction = ParameterDirection.Input;
            paramSSN_CtaOrigen.Value = cuentaOrigen.TrimEnd(' ');
            paramSSN_CtaOrigen.Size = 50;
            query.Parameters.Add(paramSSN_CtaOrigen);


            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }

        public static bool RealizarFondeoORetiro(Bonificacion elAbono, Dictionary<String, Parametro> losParametros, Response errores,
                                    SqlTransaction transaccionSQLTraspaso, SqlConnection conn, string ip, string log)
        {
            bool polizaCreada = false;
            try
            {
                try
                {
                    Poliza laPoliza = null;

                    //Genera y Aplica la Poliza
                    Executer.EventoManual aplicador = new Executer.EventoManual(elAbono.IdEvento,
                    elAbono.Concepto, false, Convert.ToInt64(elAbono.RefNumerica), losParametros, elAbono.Observaciones, conn, transaccionSQLTraspaso);
                    laPoliza = aplicador.AplicaContablilidad();

                    if (laPoliza.CodigoRespuesta != 0)
                    {
                        if (laPoliza.CodigoRespuesta.ToString() == "1016")
                        {
                            errores.DescRespuesta = "Fondos Insuficientes";
                            errores.CodRespuesta = "1016";
                            return false;
                        }
                        errores.CodRespuesta = laPoliza.CodigoRespuesta.ToString();
                        errores.DescRespuesta = laPoliza.DescripcionRespuesta.Replace("[EventoManual]", "").Replace("NO", "No").Replace(laPoliza.CodigoRespuesta.ToString(), "").Replace("Error:", "").Trim();
                        throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                    }

                    else
                    {
                        polizaCreada = true;
                        LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Se Realizó la Bonificación de Puntos a la Cuenta de la Colectiva: " + elAbono.IdColectiva.ToString() + "]");
                    }
                }

                catch (Exception err)
                {
                    throw err;
                }
            }

            catch (Exception err)
            {
                LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [RegistraEvManual_AbonoPuntos() " + err.Message + "]");
                throw new Exception("RegistraEvManual_AbonoPuntos() " + err.Message);
            }

            return polizaCreada;
        }

        public static DataSet GuardarArchivo(string pathOriginal, string pathFinal, List<DLimiteCred> listaRegistros )
        {
            using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraBatch))
            {
                conn.Open();

                SqlCommand query = new SqlCommand("ProcNoct_GuardarArchivo", conn);
                query.CommandType = CommandType.StoredProcedure;

                query.Parameters.AddWithValue("@descripcion", "Archivo de Asignación de LC");
                query.Parameters.AddWithValue("@claveProceso", "ASIGNALIMITECREDITO");
                query.Parameters.AddWithValue("@nombre", pathOriginal);
                query.Parameters.AddWithValue("@tipoArchivo", ".xlsx");
                query.Parameters.AddWithValue("@rutaSalida", pathFinal);

                query.Parameters.AddWithValue("@regValidos", listaRegistros.Where(x => x.CodRespuesta.Equals("0000")).Count());
                query.Parameters.AddWithValue("@registrosError", listaRegistros.Where(x => !x.CodRespuesta.Equals("0000")).Count());


                SqlDataAdapter sda = new SqlDataAdapter(query);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                return opcional.Tables.Count > 0 ? opcional : null;
            }
        }

    }
}
