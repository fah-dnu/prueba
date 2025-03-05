using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Entidades;
using Executer.Entidades;
using Interfases.Entidades;
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
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@tarjeta", parametro = solicitudDatos.tarjeta, encriptado=true, longitud= Constantes.longitudMedioAcceso });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);

                dtResultado = consultaBDRead.ConstruirConsulta("[ws_Ejecutor_ObtenerDatosParaExecute]", parametros, "obtenerDatosTarjetaSpeiParaExecute", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] Error al obtener los datos para execute: "+ex.Message+" "+ex.StackTrace);

                return dtResultado;
            }
        }

        public Dictionary<String, Parametro> ObtenerDatosParametros(Bonificacion elAbono, DataTable datosDiccionario, SqlConnection conn = null, SqlTransaction transaccionSQL = null, string conexionBD = null)
        {
            try
            {

                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                //Se consultan los parámetros del contrato
                //ya sea este o el proceso de abajo ambos deben de funcionar
                //losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                //(elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "");

                //funciona
                DAOEventoExecute _DaoEvento = new DAOEventoExecute(conn);
                losParametros = _DaoEvento.ListaDeParamentrosContrato
                (elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "", conn, transaccionSQL, conexionBD);



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

        public Dictionary<String, Parametro> ObtenerDatosParametrosCliente(Bonificacion elAbono, DataTable datosDiccionario, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {

                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                //Se consultan los parámetros del contrato
                //ya sea este o el proceso de abajo ambos deben de funcionar
                //losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                //(elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "");

                //funciona
                DAOEventoExecute _DaoEvento = new DAOEventoExecute(conn);
                losParametros = _DaoEvento.ListaDeParamentrosContratoCliente
                (elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "", conn);



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
            try
            {

                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@PDFGenerado", parametro = generoPDF ? "1" : "0" });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@CorreoEnviado", parametro = generoCorreo ? "1" : "0" });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@IDCorte", parametro = idCorte });
                consultaBDRead.verificarParametrosNulosString(parametros, "@ruta", rutaCompleta);


                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_Cortes_ActualizarCorte]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }
        public DataSet ObtenerDatosPagos(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ObtenerDatosPagos]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTablePagos";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosPagosDebito(string id_corte, string cadenaConexion = null, SqlConnection conn = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ObtenerDatosPagosDebito]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn);
                dtResultado.TableName = "DataTablePagosDebito";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosCH(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ObtenerDatosCuentahabiente]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableCH";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosCHTxt(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ObtenerDatosCuentahabienteEdoCuenta]", parametros, "EJECUTOR_ObtenerDatosCuentahabienteEdoCuenta", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableCH";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }
        public DataSet ObtenerDatosCHDebito(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ObtenerDatosCuentahabienteDebito]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableDatosCHDebito";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }

        public DataTable GenerarNuevoCorteDebito(Int64 idCorte, string ruta, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = idCorte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ruta", parametro = ruta });

                dtResultado = consultaBDRead.ConstruirConsulta("[EstadoCuenta_GenerarNuevoCorteDebito]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }

        /// <summary>
        /// metodo que ejcuta el sp para obtener datos contratos por la colectiva del cliente
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="claveColctiva"></param>
        /// <returns></returns>
        public DataTable ObtenerDatosContratosPorColectiva(string claveColctiva,string cadenaConexion ,SqlConnection conn = null )
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@cveColectiva", parametro = claveColctiva });


                dtResultado = consultaBDRead.ConstruirConsulta("PROCNOC_Parabilia__ObtenerDatosContratoPorCveColectiva", parametros, "PROCNOC_Contratos_Colectiva", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }
        /// <summary>
        /// metodo que consume el sp que trae  la configuracion  del template  en procesos
        /// </summary>
        /// <param name="templateConfigurado"></param>
        /// <param name="claveProceso"></param>
        /// <param name="cadenaConexion"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public DataTable ObtenerDatosTemplateParaMovimiento(string templateConfigurado, string claveProceso , string cadenaConexion, SqlConnection conn = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@claveProceso", parametro = claveProceso });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@claveTemplateSalida", parametro = templateConfigurado });

                dtResultado = consultaBDRead.ConstruirConsulta("[ConsultaTemplateSalida]", parametros, "ConsultaTemplateSalida", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
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

        public DataTable ObtenerFactura(string idCorte, string ruta, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
         
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idCorte ", parametro = idCorte });
             
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_ObtenerFacturaCorteCredito]", parametros, "EstadoCuenta_ObtenerFacturaCorteCredito", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }

        public DataTable ObtenerFacturaV1(string idCorte, string ruta, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {

                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idCorte", parametro = idCorte });

                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_ObtenerFacturaCorteCredito1]", parametros, "EstadoCuenta_ObtenerFacturaCorteCredito", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }


        public DataTable ObtenerFacturaDetalles(string idCorte, string ruta, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idFactura", parametro = idCorte });

                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_ObtenerFacturaDetallesCorteCredito]", parametros, "EstadoCuenta_ObtenerFacturaCorteCredito", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }

        //estadod e cuenta externos
        public DataTable ObtenerCortesExternos(SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ObtieneCuentasPorProcesar]", parametros, "procnoc_travel_ObtieneCuentasPorProcesar", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al obtener corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }

        //acualiza registris
        public DataTable ActualizarCorteExterno(String id,SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@Id", parametro = id });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ActualizaCuentasPorProcesar]", parametros, "procnoc_travel_ActualizaCuentasPorProcesar", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar registro corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }

        public DataTable ActualizarCorteExternoError(String id, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@Id", parametro = id });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ActualizaCuentasPorProcesarError]", parametros, "procnoc_travel_ActualizaCuentasPorProcesar", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar registro corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }

        #region externos
        public DataSet ObtenerDatosCHExterno(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@id", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ObtieneDatosCHExterno]", parametros, "procnoc_travel_ObtieneDatosCHExterno", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableDatosCHDebito";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosCHExterno(string id_corte, CuentaAhorroCLABE cuentaYCLABE , string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@id", parametro = id_corte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@cuentaAhorro", parametro = cuentaYCLABE.cuentaAhorro});
                parametros.Add(new ParametrosProcedimiento { Nombre = "@clabe", parametro = cuentaYCLABE.clabe });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ObtieneDatosCHExternoAhorro]", parametros, "procnoc_travel_ObtieneDatosCHExterno", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableDatosCHDebito";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosPagosDebitoExterno(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null, DetalleArchivoXLSX detalleRegistro=null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@id", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ObtieneDatosPagosCTA]", parametros, "procnoc_travel_ObtieneDatosPagosCTA", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTablePagosDebito";
                if (detalleRegistro != null && (dtResultado != null))
                {
                    if (dtResultado.Rows != null && dtResultado.Rows.Count > 0)
                    {
                     //   detalleRegistro.NumeroOperaciones = detalleRegistro.NumeroOperaciones + dtResultado.Rows.Count;
                    }
                }
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosPagosDebitoExterno(string id_corte, CuentaAhorroCLABE cuentaYCLABE, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null, DetalleArchivoXLSX detalleRegistro=null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@id", parametro = id_corte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@cuentaAhorro", parametro = cuentaYCLABE.cuentaAhorro });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@clabe", parametro = cuentaYCLABE.clabe });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ObtieneDatosPagosCTAAhorro]", parametros, "procnoc_travel_ObtieneDatosPagosCTA", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTablePagosDebito";
                if (detalleRegistro != null && (dtResultado != null))
                {
                    if (dtResultado.Rows != null && dtResultado.Rows.Count > 0)
                    {
                      //  detalleRegistro.NumeroOperaciones = detalleRegistro.NumeroOperaciones + dtResultado.Rows.Count;
                    }
                }
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }


        public DataTable ObtenerDatosCargosObjetados(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@id", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ObtieneDatosCargosObjetadosCTA]", parametros, "procnoc_travel_ObtieneDatosCargosObjetadosCTA", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableCargosObjetados";
                return dtResultado.DataSet.Tables[0]; ;//.Tables[0];
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet.Tables[0]; ;//.Tables[0];
            }
        }

        public DataTable ObtenerDatosComisionesRelevantes(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@id", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ObtieneDatosPagosComisionesCTA]", parametros, "procnoc_travel_ObtieneDatosPagosComisionesCTA", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableComisiones";
                return dtResultado.DataSet.Tables[0];
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet.Tables[0];
            }
        }


        public DataTable ObtenerDatosComisiones(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@id", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_travel_ObtieneDatosComisionesCTA]", parametros, "[procnoc_travel_ObtieneDatosComisionesCTA]", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableComisiones";
                return dtResultado.DataSet.Tables[0];
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet.Tables[0];
            }
        }

        public DataTable ObtenerDatosColectivaTimbre(string claveColectiva, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            try
            {
                if (cadenaConexion != null)
                {
                    consultaBDRead = new BaseDeDatos(cadenaConexion);
                }
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                 parametros.Add(new ParametrosProcedimiento { Nombre = "@claveColectiva", parametro = claveColectiva });
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_ProcesaEstadoCuenta_ObtieneColectivaTimbrado]", parametros, "[Procnoc_ProcesaEstadoCuenta_ObtieneColectivaTimbrado]", "", conn, null, null, null, conjuntoDatos);
                return dtResultado.DataSet.Tables[0];
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al obtener corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet.Tables[0];
            }
        }

        public DataTable ObtenerCuentasPorCliente(string fecha,string idRegistro,SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@id", parametro = idRegistro });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@fecha", parametro = fecha });
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_CortesExternos_ObtieneCuentasPorCliente]", parametros, "procnoc_travel_ObtieneCuentasPorProcesar", "", conn);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito]error al obtener corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }


        #endregion

        public DataTable ObtenerDatosCortes(string store, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                dtResultado = consultaBDRead.ConstruirConsulta(store, parametros, store, "", conn, null, null, null);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
        }

        public DataSet ObtenerDatosSubreporteMSI(string id_corte, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = id_corte });
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_MSI_ObtenerMovimientosDiferidos]", parametros, "EJECUTOR_Cortes_ActualizarCorte", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableCargosObjetados";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }


        public DataSet ObtenerDatosCHAdicional(string idCorte,string idColectiva,string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = idCorte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_ColectivaAdicional", parametro = idColectiva });
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_Cortes_ObtenerDatosCuentahabienteAdicional]", parametros, "Procnoc_Cortes_ObtenerDatosCuentahabienteAdicional", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTableCH";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }

        public DataSet ObtenerDatosPagosAdicional(string idCorte, string idCuenta, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = idCorte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_CuentaAdicional", parametro = idCuenta });
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_Cortes_ObtenerDatosPagosAdicional]", parametros, "Procnoc_Cortes_ObtenerDatosPagosAdicional", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "DataTablePagos";
                return dtResultado.DataSet;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al actualizar corte:" + ex.Message + ex.StackTrace);
                return dtResultado.DataSet;
            }
        }


    }
}
