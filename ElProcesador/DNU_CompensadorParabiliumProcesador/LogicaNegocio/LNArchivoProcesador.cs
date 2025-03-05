using CommonProcesador;
using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Constants;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.LogicaNegocio;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumProcesador.Model;
using DNU_CompensadorParabiliumProcesador.Utilidades;
using Executer.BaseDatos;
using Executer.Entidades;
using Executer.Model;
using Interfases.Entidades;
using Microsoft.Azure.KeyVault.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Math.EC.Multiplier;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumProcesador.LogicaNegocio
{
    public class LNArchivoProcesador
    {
        private static string etiquetaProcesoComp = Processes.PROCESA_COMPENSACION.ToString();
        /// <summary>
        /// Mentodo que obtiene los resultados de la consulta para validar el reproceso
        /// </summary>
        /// <returns></returns>
        public static int GenerarReprocesoCompesacion()
        {
            return DAOComparador.ValidaActualizarMovimientosTimeout();
        }

        /// <summary>
        /// Mentodo que obtiene los resultados de la consulta para validar el reproceso
        /// </summary>
        /// <returns></returns>
        public static void CompensarImportes(string connConsulta, string etiquetaLogueo)
        {
            DAOComparador.CompensaOperacionesImporteCero(connConsulta, etiquetaLogueo);
        }

        public static RespuestaProceso ProcesaArchivosDesdeBD(string etiquetaLogueoComp)
        {

            RespuestaProceso laRespuestadelFichero = new RespuestaProceso();

            StringBuilder lasRespuesta = new StringBuilder();
            DAOVariante reglaVariante = new DAOVariante();
            bool insertarSolicitudesReportes = true;
            try
            {

                string connAutoRead = PNConfig.Get(etiquetaProcesoComp, "BDReadAutorizador");
                string connAutoWrite = PNConfig.Get(etiquetaProcesoComp, "BDWriteAutorizador");
                string connCompRead = PNConfig.Get(etiquetaProcesoComp, "BDReadCompensador");
                string connCompWrite = PNConfig.Get(etiquetaProcesoComp, "BDWriteCompensador");

                Dictionary<String, Parametro> lstParametrosContrato = new Dictionary<string, Parametro>();
                Dictionary<String, Parametro> lstParametrosPMA = new Dictionary<string, Parametro>();

                Dictionary<String, Parametro> parametrosExec = new Dictionary<string, Parametro>();

                ///Relación CSOs
                //CompensarSinOperaciones();//Pendiente de actualizar con documento y Revisión

                ///actualiza operaciones con importe en cero
                CompensarImportes(connAutoWrite, etiquetaLogueoComp);


                //Se agrega subModulo de Obtener ValoresContrato
                List<ClaveContrato> lstClavesContrato = DAOComparador.ObtenerValoresContratos(connAutoRead, etiquetaLogueoComp);

                if (lstClavesContrato.Count < 1)
                {
                    Log.Evento(etiquetaLogueoComp + "[ObtenerValoresContratos] NO SE OBTUVO NINGUN VALOR DE CONTRATO");
                    throw new Exception("[ObtenerValoresContratos] NO SE OBTUVO NINGUN VALOR DE CONTRATO");
                }

                //Se agrega subModulo de Obtener ParametrosMultiasignacion
                List<ClaveParametroMultiasignacion> clavesParametrosMultiasignacion = DAOComparador.ObtenerParametrosMultiasignacion(connAutoRead, etiquetaLogueoComp);

                if (clavesParametrosMultiasignacion.Count < 1)
                {
                    Log.Evento(etiquetaLogueoComp + "[ObtenerParametrosMultiasignacion] NO SE OBTUVO NINGUN PARAMETRO MULTIASIGNACION");
                    throw new Exception("[ObtenerParametrosMultiasignacion] NO SE OBTUVO NINGUN PARAMETRO MULTIASIGNACION");
                }

                List<Colectiva> lstColectivas = DAOComparador.ObtenerColectivas(connCompRead, etiquetaLogueoComp);

                foreach (Colectiva colectiva in lstColectivas)
                {
                    insertarSolicitudesReportes = true;
                    try
                    {
                        lstParametrosContrato = new Dictionary<string, Parametro>();

                        foreach (ClaveContrato claveContrato in lstClavesContrato)
                        {
                            claveContrato.valorContrato = DAOComparador.ObtenerValorContrato(colectiva.CveColectiva, claveContrato.cveValorContrato, connAutoRead, etiquetaLogueoComp);


                            if (claveContrato.valorContrato == null)
                            {
                                Log.Evento(etiquetaLogueoComp + $"[{NombreMetodo()}] [ObtenerValorContrato] NO SE OBTUVO NINGUN VALOR DE CONTRATO: {claveContrato.cveValorContrato}, Y COLECTIVA: {colectiva.CveColectiva}");
                                throw new Exception($"[ObtenerValorContrato] NO SE OBTUVO NINGUN VALOR DE CONTRATO: {claveContrato.cveValorContrato}, Y COLECTIVA: {colectiva.CveColectiva}");
                            }
                            else
                            {
                                ParametroV6 parametroContrato = new ParametroV6();
                                parametroContrato.Nombre = claveContrato.valorContrato.NombreParametro;
                                parametroContrato.Valor = claveContrato.valorContrato.Valor;
                                parametroContrato.TipoDato = DNU_CompensadorParabiliumCommon.BaseDatos.TipoDato.getTipoDatoSQL(claveContrato.valorContrato.TipoDato);
                                lstParametrosContrato.Add(parametroContrato.Nombre, parametroContrato);
                            }
                        }

                        List<Producto> lstProductos = DAOComparador.ObtenerProductos(connCompRead, colectiva.CveColectiva, etiquetaLogueoComp);

                        foreach (Producto producto in lstProductos)
                        {
                            lstParametrosPMA = new Dictionary<string, Parametro>();
                            foreach (ClaveParametroMultiasignacion claveParametroMultiasignacion in clavesParametrosMultiasignacion)
                            {
                                claveParametroMultiasignacion.valorParametroMultiasignacion = DAOComparador.ObtenerParametroMultiasignacion(producto.CveProducto, claveParametroMultiasignacion.cveParametroMultiasignacion, connAutoRead, etiquetaLogueoComp);

                                if (claveParametroMultiasignacion.valorParametroMultiasignacion == null)
                                {
                                    Log.Evento(etiquetaLogueoComp + $"[{NombreMetodo()}] [ObtenerParametroMultiasignacion] NO SE OBTUVO VALOR DEL PARAMETRO MULTIASIGNACION: {claveParametroMultiasignacion.cveParametroMultiasignacion}, Y PRODUCTO: {producto.CveProducto}");
                                    throw new Exception($"[ObtenerParametroMultiasignacion] NO SE OBTUVO VALOR DEL PARAMETRO MULTIASIGNACION: {claveParametroMultiasignacion.cveParametroMultiasignacion}, Y PRODUCTO: {producto.CveProducto}");
                                }
                                else
                                {
                                    Parametro parametroPMA = new Parametro();
                                    parametroPMA.Nombre = claveParametroMultiasignacion.valorParametroMultiasignacion.NombreParametro;
                                    parametroPMA.Valor = claveParametroMultiasignacion.valorParametroMultiasignacion.Valor;
                                    parametroPMA.TipoDato = DNU_CompensadorParabiliumCommon.BaseDatos.TipoDato.getTipoDatoSQL(claveParametroMultiasignacion.valorParametroMultiasignacion.TipoDato);
                                    lstParametrosPMA.Add(parametroPMA.Nombre, parametroPMA);
                                }
                            }

                            List<RegistroCompensar> lstRegCompensar = DAOComparador.ObtenerRegistrosPorCompensar(connCompRead, producto.CveProducto, etiquetaLogueoComp);

                            if (lstRegCompensar.Count < 1)
                            {
                                Log.Evento(etiquetaLogueoComp + $"[{NombreMetodo()}] NO SE OBTUVO NINGUN REGISTRO POR COMPENSAR");
                                continue;
                            }

                            
                            foreach (RegistroCompensar regComp in lstRegCompensar)
                            {
                                parametrosExec = new Dictionary<string, Parametro>();
                                asignarParametrosContrao_PMA(parametrosExec, lstParametrosContrato);
                                asignarParametrosContrao_PMA(parametrosExec, lstParametrosPMA);

                                ObtenerParametros(ref parametrosExec, regComp);

                                DAOComparador.ObtenerColectivasRelacionadas(connAutoRead, ref parametrosExec, etiquetaLogueoComp);

                                if (insertarSolicitudesReportes)
                                {
                                    List<string> lstFechasPresentacion = lstRegCompensar.Select(reg => reg.fechaPresentacion).Distinct().ToList();

                                    foreach (string fechaPresentacion in lstFechasPresentacion)
                                    {
                                        DAOComparador.InsertarSolicitudesReportes(connCompWrite, parametrosExec["@idColectiva_Emisor"].ClaveTipoColectiva, colectiva.CveColectiva, fechaPresentacion, etiquetaLogueoComp);
                                    }

                                    insertarSolicitudesReportes = false;
                                }
                                try
                                {
                                    CompensarRegistrosDetallado(regComp, connAutoWrite, connCompWrite, connAutoRead, connCompRead
                                                , etiquetaLogueoComp, parametrosExec, producto.CveProducto, colectiva.IdColectiva);
                                }
                                catch (Exception ex)
                                {
                                    string errorMsj = "{\"responseCode\":\"406\", \"responseDate\":\"" +
                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                                "\", \"responseMessage\":\"" + ex.Message + "\", \"objectName\":\"CompensarRegistrosDetallado\", " +
                                                "\"objectVersion\":\"1.3.0.0\", \"objectType\":\"Metodo\"}";
                                    DAOComparador.ActualizarEstatusFicheroDetalle(regComp.idFicheroDetalle, EstatusFicheroDetalle.ERROR.ToString(), connCompWrite, etiquetaLogueoComp, errorMsj);
                                    Log.Error(etiquetaLogueoComp + $"[{NombreMetodo()}] [Clave colectiva: {colectiva.CveColectiva}] [CompensarRegistrosDetallado] " + ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(etiquetaLogueoComp + $"[{NombreMetodo()}] [Clave colectiva: {colectiva.CveColectiva}] " + ex.Message);
                    }
                }

                List<SolicitudReporte> SolicitudesReportes = DAOComparador.ObtenerSolicitudesReportes(connCompRead, etiquetaLogueoComp);

                if (SolicitudesReportes.Count < 1)
                {
                    Log.Evento(etiquetaLogueoComp + "[ObtenerSolicitudesReportes] NO SE OBTUVO NINGUNA SOLICITUD DE REPORTES");
                    throw new Exception("[ObtenerSolicitudesReportes] NO SE OBTUVO NINGUNA SOLICITUD DE REPORTES");
                }

                foreach (SolicitudReporte solicitudReporte in SolicitudesReportes)
                {
                    try
                    {
                        string idGeneracionReporte = string.Empty;
                        bool inserto = DAOComparador.InsertarSolicitudReporteAuto(solicitudReporte, connAutoWrite, etiquetaLogueoComp, ref idGeneracionReporte);

                        if (inserto)
                        {
                            DAOComparador.ActualizaEstatusSolicitudReporte(idGeneracionReporte, solicitudReporte.ID_SolicitudReporte, "OK", connCompWrite, etiquetaLogueoComp);
                        }
                        else
                        {
                            Log.Evento(etiquetaLogueoComp + $" [{NombreMetodo()}] [InsertarSolicitudReporteAuto] NO SE INSERTO NINGUN ID SOLICITUD DE REPORTES, ClavePlugIn:  {solicitudReporte.ClavePlugIn}, ClaveReporte: {solicitudReporte.ClaveReporte}, idGeneracionReporte: {idGeneracionReporte}");

                            DAOComparador.ActualizaEstatusSolicitudReporte(idGeneracionReporte, solicitudReporte.ID_SolicitudReporte, "ERROR", connCompWrite, etiquetaLogueoComp);
                            throw new Exception($"[InsertarSolicitudReporteAuto] NO SE INSERTO NINGUN ID SOLICITUD DE REPORTES, ClavePlugIn:  {solicitudReporte.ClavePlugIn}, ClaveReporte: {solicitudReporte.ClaveReporte}, idGeneracionReporte: {idGeneracionReporte}");
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Error(etiquetaLogueoComp + $"{NombreMetodo()}: " + ex.Message);
                    }
                }

                DAOComparador.RestableceSolicitudesReportes(connCompWrite, etiquetaLogueoComp);

            }
            catch (Exception ERR)
            {
                Log.Error(etiquetaLogueoComp + $"{NombreMetodo()}: " + ERR.Message);
            }

            return laRespuestadelFichero;

        }


        private static void CompensarSinOperaciones()
        {
            DataTable tabLst = DAOComparador.ObtenerListaCSOs("P");
            SqlConnection conn = null;
            Int64 idRelacionCSO, idRegistroComp, idOperacion, idFicheroDetalle;
            string autorizacion;

            foreach (DataRow dr in tabLst.Rows)
            {
                conn = new SqlConnection(BDAutorizador.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction("T112_CSO"))
                {
                    try
                    {
                        idRelacionCSO = Convert.ToInt64(dr["ID_RelacionCSO"]);
                        idRegistroComp = Convert.ToInt64(dr["ID_RegistroCompensacion"]);
                        idOperacion = Convert.ToInt64(dr["ID_Operacion"]);
                        idFicheroDetalle = Convert.ToInt64(dr["ID_FicheroDetalle"]);
                        autorizacion = dr["Autorizacion"].ToString();

                        DAOComparador.CancelarPolizasRelacionCSO(conn, transaccionSQL, idRelacionCSO
                                        , idRegistroComp, idOperacion);

                        DAOFichero.ObtenerAutorizacionOperacion(idFicheroDetalle, autorizacion);


                        DAOComparador.ActualizarEstatusRelacionCSO(conn, transaccionSQL, idRelacionCSO
                                        , "R");

                        transaccionSQL.Commit();

                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[CompensarSinOperaciones] [ " + ex.Message + "] " + "[" + ex.StackTrace + "]");
                        transaccionSQL.Rollback();

                    }
                }
            }
        }

        private static void CompensarRegistrosDetallado(RegistroCompensar registroCompensar, string connAutorizadorW, string connCompensadorW
                                                , string connAutorizadorR, string connCompensadorR, string etiquetaLogueoComp
                                                , Dictionary<String, Parametro> lstParametros, string cveProducto, string idColectiva)
        {
            bool respActualizaFicheroDetalle = DAOComparador.ActualizarEstatusFicheroDetalle(registroCompensar.idFicheroDetalle, EstatusFicheroDetalle.RUNNING.ToString(), connCompensadorW, etiquetaLogueoComp, null);
            List<Variante> lstVariantes = new List<Variante>();
            bool respuestaEjecucionVariantes = false;
            string mensajeError = null, cveVariante = null;

            if (respActualizaFicheroDetalle)
            {
                bool respObtenerInfoOperacion = DAOComparador.ObtenerInformacionOperacion(registroCompensar, connAutorizadorR, etiquetaLogueoComp, ref lstParametros, ref mensajeError);

                if (respObtenerInfoOperacion)
                {
                    lstVariantes = DAOComparador.ObtenerVariantes(lstParametros["@cvePertenenciaTipo"].Valor, lstParametros, connAutorizadorR, etiquetaLogueoComp);

                    if (lstVariantes.Count() > 0)
                    {
                        respuestaEjecucionVariantes = EjecutarVariantes(lstVariantes, lstParametros, ref mensajeError, ref cveVariante, connAutorizadorR, etiquetaLogueoComp);

                        if (respuestaEjecucionVariantes)
                        {
                            CompensarRegistro(registroCompensar, connAutorizadorW, connCompensadorW
                                                , connAutorizadorR, connCompensadorR, etiquetaLogueoComp
                                                , lstParametros, cveVariante, cveProducto, idColectiva);
                        }
                        else
                        {
                            DAOComparador.ActualizarEstatusFicheroDetalle(registroCompensar.idFicheroDetalle, EstatusFicheroDetalle.ERROR.ToString(), connCompensadorW, etiquetaLogueoComp, mensajeError);
                        }
                    }
                    else
                    {
                        CompensarRegistro(registroCompensar, connAutorizadorW, connCompensadorW
                                                                        , connAutorizadorR, connCompensadorR, etiquetaLogueoComp
                                                                        , lstParametros, cveVariante, cveProducto, idColectiva);
                    }
                }
                else
                {
                    DAOComparador.ActualizarEstatusFicheroDetalle(registroCompensar.idFicheroDetalle, EstatusFicheroDetalle.ERROR.ToString(), connCompensadorW, etiquetaLogueoComp, mensajeError);
                }
            }
        }

        private static void CompensarRegistro(RegistroCompensar registroCompensar, string connAutorizadorW, string connCompensadorW
                                                , string connAutorizadorR, string connCompensadorR, string etiquetaLogueoComp
                                                , Dictionary<String, Parametro> lstParametros, string cveVariante, string cveProducto
                                                , string idColectiva)
        {
            List<Evento> lstEventosEjecutar = new List<Evento>();
            string idPolizas = null;
            bool insertaRegistroCompValido = false;
            string cveEstatusDestinoOp = null;
            string mensajeErrorEventos = null;
            lstEventosEjecutar = DAOComparador.ObtenerEventos(lstParametros["@cvePertenenciaTipo"].Valor, cveVariante, connAutorizadorR, ref cveEstatusDestinoOp, ref mensajeErrorEventos, etiquetaLogueoComp);
            bool actualizarOK = false;
            List<Evento> lstEventosProcesados = new List<Evento>();
            bool errorPoliza = false;

            if (lstEventosEjecutar.Count < 1)
            {
                DAOComparador.ActualizarEstatusFicheroDetalle(registroCompensar.idFicheroDetalle, EstatusFicheroDetalle.ERROR.ToString(), connCompensadorW, etiquetaLogueoComp, mensajeErrorEventos);
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(connAutorizadorW))
                {
                    conn.Open();
                    using (SqlTransaction transaccionSQL = conn.BeginTransaction("Compensador_Registro"))
                    {
                        try
                        {
                            foreach (Evento evento in lstEventosEjecutar)
                            {
                                string observaciones = lstParametros["@cveColectiva_Emisor"].Valor.ToString() + " - " +
                                                    cveProducto + " - " + evento.CveEvento;

                                Parametro parametro = new Parametro();
                                parametro.Nombre = "@cveEvento";
                                parametro.Valor = evento.CveEvento;
                                lstParametros.Add(parametro.Nombre, parametro);

                                Poliza poliza = LNProcesaMovimiento.EjecutarContabilidad(registroCompensar.referencia, observaciones, evento.DescEvento,
                                            evento.IdEvento, lstParametros, registroCompensar.idFicheroDetalle, conn, transaccionSQL, etiquetaLogueoComp);



                                if (poliza.CodigoRespuesta != 0)
                                {
                                    transaccionSQL.Rollback();
                                    errorPoliza = true;

                                    string errorExecuter = "[{\"responseCode\":\"406\", \"responseDate\":\"" +
                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                                "\", \"responseMessage\":\"" + poliza.DescripcionRespuesta + "\", \"objectName\":\"Executer\", " +
                                                "\"objectVersion\":\"1.3.0.0\", \"objectType\":\"Componente\", \"parameters\": {" +
                                                "\"@cveEvento\":\"" + evento.CveEvento + "\"}}]";
                                    Log.Error(etiquetaLogueoComp + $"[{NombreMetodo()}] " + errorExecuter);

                                    DAOComparador.ActualizarEstatusFicheroDetalle(registroCompensar.idFicheroDetalle, EstatusFicheroDetalle.ERROR.ToString(),
                                                                    connCompensadorW, etiquetaLogueoComp, errorExecuter);

                                    throw new Exception("[ERROR AL GENERAR MOVIMIENTO CODIGO] [IdFicheroDetalle:" + registroCompensar.idFicheroDetalle + ",IdEvento:" +
                                                    evento.IdEvento + ",DescRespuesta:" + poliza.DescripcionRespuesta);

                                }
                                else
                                {
                                    lstEventosProcesados.Add(evento);
                                    idPolizas = idPolizas + poliza.ID_Poliza + ",";
                                    lstParametros.Remove("@cveEvento");
                                }
                            }


                            idPolizas = idPolizas.TrimEnd(',');
                            string respuestaErrorRegComp = null;

                            DAOAutorizador.InsertarRegistroComp(registroCompensar, lstParametros, conn, transaccionSQL
                                            , etiquetaLogueoComp, idPolizas, cveEstatusDestinoOp, cveVariante, ref insertaRegistroCompValido, ref respuestaErrorRegComp);

                            if (insertaRegistroCompValido && lstParametros["@aplicaWebhookCompensador"].Valor.Equals("1"))
                            {
                                int contadorEventos = 0;
                                string[] lstPolizas = idPolizas.Split(',');
                                foreach (string idPoliza in lstPolizas)
                                {
                                    insertaRegistroCompValido = LNWebhook.EnviarNotificacion(registroCompensar, idColectiva, idPoliza
                                                        , Convert.ToInt64(lstParametros["@ID_OperacionOriginal"].Valor), etiquetaLogueoComp
                                                        , lstEventosProcesados[contadorEventos].DescEvento, lstEventosProcesados[contadorEventos].CveEvento);
                                    if (!insertaRegistroCompValido)
                                    {
                                        transaccionSQL.Rollback();
                                        respuestaErrorRegComp = "[{{\"responseCode\":\"406\", \"responseDate\":\"" +
                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                                "\", \"responseMessage\":\"ERROR AL ENVIAR WEBHOOK\", \"objectName\":\"WebHook\", " +
                                                "\"objectVersion\":\"1.0.0.0\", \"objectType\":Componente\"}}]";
                                        break;

                                    }

                                    contadorEventos++;
                                }
                                if (insertaRegistroCompValido)
                                {
                                    actualizarOK = true;
                                }

                            }
                            else if (insertaRegistroCompValido)
                            {
                                actualizarOK = true;
                            }
                            else
                            {
                                transaccionSQL.Rollback();
                            }

                            if (actualizarOK)
                            {
                                bool respActualizaEstatus = DAOComparador.ActualizarEstatusFicheroDetalle(registroCompensar.idFicheroDetalle, EstatusFicheroDetalle.OK.ToString(),
                                                                   connCompensadorW, etiquetaLogueoComp, null);
                                if (respActualizaEstatus)
                                    transaccionSQL.Commit();
                                else
                                    transaccionSQL.Rollback();
                            }
                            else
                            {
                                DAOComparador.ActualizarEstatusFicheroDetalle(registroCompensar.idFicheroDetalle, EstatusFicheroDetalle.ERROR.ToString(),
                                                                    connCompensadorW, etiquetaLogueoComp, respuestaErrorRegComp);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(etiquetaLogueoComp + $"{NombreMetodo()}: " + ex.Message + "] [" + ex.StackTrace + "]");
                            if (!errorPoliza)
                            {
                                transaccionSQL.Rollback();
                                string error = "[{{\"responseCode\":\"406\", \"responseDate\":\"" +
                                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                                    "\", \"responseMessage\":\"" + ex.Message + "\", \"objectName\":\"CompensarRegistro\", " +
                                                    "\"objectVersion\":\"1.0.0.0\", \"objectType\":Método\"}}]";
                                DAOComparador.ActualizarEstatusFicheroDetalle(registroCompensar.idFicheroDetalle, EstatusFicheroDetalle.ERROR.ToString(),
                                                                        connCompensadorW, etiquetaLogueoComp, error);
                            }
                        }
                    }
                }
            }
        }

        private static void asignarParametrosContrao_PMA(Dictionary<String, Parametro> lstParametrosOut,
                                                        Dictionary<String, Parametro> lstParametrosIN)
        {
            foreach (var registro in lstParametrosIN)
            {
                lstParametrosOut.Add(registro.Key, registro.Value);
            }
        }

        private static void ObtenerParametros(ref Dictionary<String, Parametro> lstParametros, RegistroCompensar regCompensar)
        {

            Parametro parametro;
            string nombreParam = null, valorParam = null;

            PropertyInfo[] lstPropiedadesRegCompensar = typeof(RegistroCompensar).GetProperties();
            foreach (PropertyInfo oProperty in lstPropiedadesRegCompensar)
            {
                nombreParam = "@" + oProperty.Name;
                valorParam = oProperty.GetValue(regCompensar).ToString();
                parametro = new Parametro();
                parametro.Nombre = nombreParam;
                parametro.ID_TipoColectiva = 0;
                parametro.Valor = valorParam;
                parametro.ClaveTipoColectiva = "";
                lstParametros.Add(parametro.Nombre, parametro);
            }

            nombreParam = "@Importe";
            if (lstParametros.ContainsKey(nombreParam))
            {
                lstParametros[nombreParam].Valor = regCompensar.impCompensacion;
            }
            else
            {
                parametro = new Parametro();
                parametro.Nombre = nombreParam;
                parametro.ID_TipoColectiva = 0;
                parametro.Valor = regCompensar.impCompensacion;
                parametro.ClaveTipoColectiva = "";
                lstParametros.Add(parametro.Nombre, parametro);
            }

        }

        private static bool EjecutarVariantes(List<Variante> lstVariantes, Dictionary<String, Parametro> lstParametros, ref string mensajeErrorVariante, ref string cveVariante, string connCompensadorR, string etiquetaLogueoComp)
        {
            foreach (Variante variante in lstVariantes.OrderBy(elemento => elemento.Ejecucion))
            {

                RespuestaVarianteV6 respuestaV6 = DAOComparador.EjecutarVariante(variante, lstParametros, connCompensadorR, etiquetaLogueoComp);

                if (respuestaV6 == null)
                {
                    mensajeErrorVariante = "ERROR AL EJECUTAR VARIANTES";
                    return false;
                }

                if (!DAOComparador.EvaluarRespuestaSPS(respuestaV6))
                {
                    mensajeErrorVariante = respuestaV6.DescRespuesta;
                    return false;
                }

                if (!respuestaV6.respuestaSP)
                {
                    mensajeErrorVariante = respuestaV6.DescRespuesta;
                    cveVariante = respuestaV6.cveVariante;
                    return false;
                }

                if (variante.varianteFinal && respuestaV6.cumpleVariante)
                {
                    cveVariante = respuestaV6.cveVariante;
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// Devuelve el nombre del metodo que llamo a este metodo.
        /// Ejemplo el metodo consultaDato llama a este metodo el resultado es: consultaDato
        /// </summary>
        /// <returns>Nombre del metodo que lo invocó</returns>
        private static string NombreMetodo([CallerMemberName] string metodo = null)
        {
            return metodo;
        }
    }
}
