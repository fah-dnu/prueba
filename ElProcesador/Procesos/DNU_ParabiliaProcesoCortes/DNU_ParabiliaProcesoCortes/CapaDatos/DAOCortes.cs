using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.Utilidades;
using Executer.Entidades;
using Interfases.Entidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.CapaDatos
{
    class DAOCortes
    {

        private BaseDeDatos consultaBDRead;
        private BaseDeDatos consultaBDWrite;
        private DataTable dtResultado;
        public string nuevaPoliza { get; set; }
        public DAOCortes()
        {

            consultaBDRead = new BaseDeDatos("");
            consultaBDWrite = new BaseDeDatos("");

        }
        public DataTable obtenerDatosParaExecute(DatosOperacionesExecute solicitudDatos, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@tarjeta", parametro = solicitudDatos.tarjeta });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_ObtenerDatosParaExecute]");
                dtResultado = consultaBDRead.ConstruirConsulta("[ws_Ejecutor_ObtenerDatosParaExecute]", parametros, "obtenerDatosTarjetaSpeiParaExecute", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito] Error al obtener los datos para execute: " + ex.Message + " " + ex.StackTrace + "]");
                return dtResultado;
            }
        }

        public Dictionary<String, Parametro> ObtenerDatosParametros(Bonificacion elAbono, DataTable datosDiccionario, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            
            try
            {

                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                //Se consultan los parámetros del contrato
                //ya sea este o el proceso de abajo ambos deben de funcionar
                //losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                //(elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "");

                //funciona
                DAOEventoExecute _DaoEvento = new DAOEventoExecute(conn, transaccionSQL);
                losParametros = _DaoEvento.ListaDeParamentrosContrato
                (elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "", conn, transaccionSQL);



                //  losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                //(elAbono.ClaveColectiva, elAbono.Tarjeta, "TAR",elAbono.ClaveEvento, conn,transaccionSQL);


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
        public DataTable ActualizarDatoCorreoYPDF(bool generoPDF, bool generoCorreo, Int64 idCorte, SqlConnection conn = null, SqlTransaction transaccionSQL = null, string rutaCompleta = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {

                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@PDFGenerado", parametro = generoPDF ? "1" : "0" });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@CorreoEnviado", parametro = generoCorreo ? "1" : "0" });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@IDCorte", parametro = idCorte });
                consultaBDRead.verificarParametrosNulosString(parametros, "@ruta", rutaCompleta);

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_Cortes_ActualizarCorte]");
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_Cortes_ActualizarCorte]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [error al actualizar corte:" + ex.Message + ex.StackTrace + "]");
                return dtResultado;
            }
        }
        public DataSet ObtenerDatosPagos(string id_corte, string cadenaConexion = null, SqlConnection conn = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtenerDatosPagos]");
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ObtenerDatosPagos]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn);
                dtResultado.TableName = "DataTablePagos";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [error al actualizar corte:" + ex.Message + ex.StackTrace + "]");
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosPagosDebito(string id_corte, string cadenaConexion = null, SqlConnection conn = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtenerDatosPagosDebito]");
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ObtenerDatosPagosDebito]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn);
                dtResultado.TableName = "DataTablePagosDebito";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [error al actualizar corte:" + ex.Message + ex.StackTrace + "]");
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosCH(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtenerDatosCuentahabiente]");
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ObtenerDatosCuentahabiente]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableCH";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [error al actualizar corte:" + ex.Message + ex.StackTrace + "]");
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosCHDebito(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtenerDatosCuentahabienteDebito]");
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ObtenerDatosCuentahabienteDebito]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableDatosCHDebito";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [error al actualizar corte:" + ex.Message + ex.StackTrace + "]");
                return dtResultado.DataSet;
            }
        }

        public DataTable GenerarNuevoCorteDebito(Int64 idCorte, string ruta, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = idCorte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ruta", parametro = ruta });
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EstadoCuenta_GenerarNuevoCorteDebito]");
                dtResultado = consultaBDRead.ConstruirConsulta("[EstadoCuenta_GenerarNuevoCorteDebito]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [error al actualizar corte:" + ex.Message + ex.StackTrace + "]");
               return dtResultado;
            }
        }

        public DataTable obtenerSaldoInsoluto(string saldoInsoluto, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                return new DataTable();
                //    List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                //    parametros.Add(new ParametrosProcedimiento { Nombre = "@tarjeta", parametro = solicitudDatos.tarjeta });
                //    parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                //    parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                //    parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                //    // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);

                //    dtResultado = consultaBDRead.ConstruirConsulta("[ws_Ejecutor_ObtenerDatosParaExecute]", parametros, "obtenerDatosTarjetaSpeiParaExecute", "", conn, transaccionSQL);
                //    return dtResultado;
                //}
            }
            catch (Exception ex)
            {

                return dtResultado;
            }
        }

    }
}
