using CommonProcesador;
using DNU_ParabiliaAltaTarjetasNominales.Entidades;
using Executer.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;

namespace DNU_ParabiliaAltaTarjetasNominales.BaseDatos
{
    class DAODatosBase
    {
        public static Dictionary<String, Parametro> ObtenerDatosParametros(Bonificacion elAbono, DataTable datosDiccionario, Response respuesta, LogueoAltaEmpleadoV2 logEmpleado, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {

                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                losParametros = ListaDeParamentrosContrato(elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "", respuesta, logEmpleado, conn, transaccionSQL);

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
                throw err;
            }
        }

        public static Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta
            , String ClaveEvento, String elUsuario, Response respuesta, LogueoAltaEmpleadoV2 logEmpleado, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                logEmpleado.Info("[BatchTarjetasNominativas] [LimiteCredito] [PROCESADORNOCTURNO] [Ejecuta SP ws_Ejecutor_ObtieneValoresContratos]");
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
            , string limiteCredito, SqlConnection pConn, LogueoAltaEmpleadoV2 logEmpleado)
        {

            logEmpleado.Info("[BatchTarjetasNominativas] [LimiteCredito] [PROCESADORNOCTURNO] [Ejecuta SP ws_Parabilium_ObtenerDatosTarjetaAsignaLimiteCredito]");
            SqlCommand query = new SqlCommand("ProcNoct_ObtenerDatosLimiteCreditoNominativas", pConn);
            query.CommandType = CommandType.StoredProcedure;

            DbParameter paramSSN = query.CreateParameter();
            paramSSN.ParameterName = "@tarjeta";
            paramSSN.DbType = DbType.AnsiStringFixedLength;
            paramSSN.Direction = ParameterDirection.Input;
            paramSSN.Value = numeroTarjeta.TrimEnd(' ');
            paramSSN.Size = 50;
            query.Parameters.Add(paramSSN);

            query.Parameters.AddWithValue("@claveEvento", PNConfig.Get("ALTAEMPLEADODNU", "eventoAsignaLimiteCredito"));

            query.Parameters.AddWithValue("@importe", limiteCredito);


            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }

        public static bool RealizarFondeoORetiro(Bonificacion elAbono, Dictionary<String, Parametro> losParametros, Response errores,
                                    SqlTransaction transaccionSQLTraspaso, SqlConnection conn, LogueoAltaEmpleadoV2 logEmpleado)
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
                        logEmpleado.Info("[BatchTarjetasNominativas] [LimiteCredito] [PROCESADORNOCTURNO] [Se Realizó la Bonificación de Puntos a la Cuenta de la Colectiva: " + elAbono.IdColectiva.ToString() + "]");
                    }
                }

                catch (Exception err)
                {
                    throw err;
                }
            }

            catch (Exception err)
            {
                logEmpleado.Error("[BatchTarjetasNominativas] [LimiteCredito] [PROCESADORNOCTURNO] [RegistraEvManual_AbonoPuntos() " + err.Message + "]");
                throw new Exception("RegistraEvManual_AbonoPuntos() " + err.Message);
            }

            return polizaCreada;
        }

    }
}
