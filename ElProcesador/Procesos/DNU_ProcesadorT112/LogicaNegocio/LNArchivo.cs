using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DNU_ProcesadorT112.Entidades;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using DNU_ProcesadorT112.BaseDatos;
using System.Data;
using System.Threading;
using DNU_ProcesadorT112.LogicaNegocio;
using Interfases.Entidades;
using Executer.Entidades;
using Executer.BaseDatos;
using DNU_ProcesadorT112.Utilidades;

namespace DNU_ProcesadorT112.LogicaNegocio
{
    class LNArchivo
    {
        private static readonly string ARCHIVOS_T112_NUEVA_VERSION_IDENTIFICADOR = "V2";

        public static String T112_FILE_VERSION { get; set; }


        delegate void EnviarDetalle(String elDetalle);
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();
        public static Boolean enProceso = false;
        private static readonly object balanceLock = new object();
#if DEBUG
        private static FileSystemWatcher elObservador = new FileSystemWatcher(@"C:\FTP_dev\cacao\T112");
#else
           private static FileSystemWatcher elObservador = new FileSystemWatcher(PNConfig.Get("PROCESAT112", "DirectorioEntrada"));
#endif

        public static RespuestaProceso ProcesaArch(Archivo unArchivo, String path)
        {
            RespuestaProceso laRespuestadelFichero = new RespuestaProceso();
            Int64 ID_Fichero = 0;
            try
            {



                char[] delimiter1 = new char[] { ',' };
                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();

                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {

                            //GUARDA EL FICHERO
                            Logueo.Evento(path + ": INICIA GUARDAR DATOS EN BD");
                            ID_Fichero = DAOArchivo.GuardarFicheroEnBD(unArchivo, conn, transaccionSQL);
                            Logueo.Evento(path + ": TERMINA GUARDAR DATOS EN BD");

                            laRespuestadelFichero.ID_Fichero = ID_Fichero;


                            if (ID_Fichero == 0)
                            {
                                transaccionSQL.Rollback();
                                Logueo.Error("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
                                Logueo.Evento(path + ": ERROR GUARDAR DATOS EN BD");
                                laRespuestadelFichero.CodigoRespuesta = 8;

                                throw new Exception("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
                            }


                            //Guarda los detalles
                            //Guarda los detallesLogueo.Evento(path + ": ERROR GUARDAR DATOS FICHERO EN BD");
                            Logueo.Evento(path + ": INICIAR GUARDAR DETALLES EN BD");
                            Boolean guardoLosDetalles = DAOArchivo.GuardarFicheroDetallesEnBD(unArchivo, ID_Fichero, ObtenerFormatoFMT(unArchivo),  conn, transaccionSQL);
                            Logueo.Evento(path + ": TERMINA GUARDAR DETALLES EN BD");

                            if (!guardoLosDetalles)
                            {
                                transaccionSQL.Rollback();
                                Logueo.Evento(path + ": ERROR GUARDAR DATOS FICHERO EN BD");
                                Logueo.Error("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
                                DAOFichero.GuardarErrorFicheroEnBD(ID_Fichero, "ERROR GUARDAR DATOS FICHERO EN BD [" + unArchivo.NombreArchivoDetectado + "]");
                                DAOFichero.ActualizaEstatusFichero(ID_Fichero, EstatusFichero.ProcesadoConErrores);
                                laRespuestadelFichero.CodigoRespuesta = 9;
                                throw new Exception("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");

                            }

                            transaccionSQL.Commit();
                            laRespuestadelFichero.CodigoRespuesta = 0;
                        }
                        catch (Exception err)
                        {
                            transaccionSQL.Rollback();
                            Logueo.Error("ProcesaArch(): ERROR AL ALMACENAR EL FICHERO: " + err.Message);

                            laRespuestadelFichero.CodigoRespuesta = 10;
                        }
                        transaccionSQL.Dispose();
                    }

                    conn.Close();
                }

                return laRespuestadelFichero;

            }
            catch (Exception err)
            {
                Logueo.Error("ProcesaArch(): " + err.Message);
                DAOFichero.GuardarErrorFicheroEnBD(ID_Fichero, "ProcesaArch():ERROR " + err.Message);
                DAOFichero.ActualizaEstatusFichero(ID_Fichero, EstatusFichero.ProcesadoConErrores);
                laRespuestadelFichero.CodigoRespuesta = 8;
                return laRespuestadelFichero;
            }
        }

        internal static string GetLongFila(Archivo elArchivo)
        {
            if (T112_FILE_VERSION.Equals(ARCHIVOS_T112_NUEVA_VERSION_IDENTIFICADOR))
            {
                Logueo.Evento("Long FIla V2 " + PNConfig.Get("PROCESAT112", "LongFila_V2"));
                return PNConfig.Get("PROCESAT112", "LongFila_V2");
            }

            return PNConfig.Get("PROCESAT112", "LongFila");
        }

        private static string ObtenerFormatoFMT(Archivo elArchivo)
        {
            if (T112_FILE_VERSION.Equals(ARCHIVOS_T112_NUEVA_VERSION_IDENTIFICADOR))
            {
                Logueo.Evento("FMT V2 FMTConfig_V2");
                return "FMTConfig_V2";
            }

            return "FMTConfig";
        }

        public static RespuestaProceso ProcesaArchivosDesdeBD(Archivo miArchivo)
        {
            RespuestaProceso laRespuestadelFichero = new RespuestaProceso();
            //  Int64 ID_Fichero = 0;
            try
            {

                char[] delimiter1 = new char[] { ',' };


                StringBuilder lasRespuesta = new StringBuilder();
                int totalProcesoOK = 0;
                int totalProcesoNOK = 0;

                //eJECUTA LOS EVENTOS

                try
                {
                    SqlConnection conn = null;
                    SqlConnection connFichero = null;

                    
                    try
                    {

                        conn = new SqlConnection(BDAutorizador.strBDEscritura);
                        conn.Open();

                        connFichero = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo);
                        connFichero.Open();

                        int totalProceso = 0;

                        laRespuestadelFichero.ID_Fichero = miArchivo.ID_Archivo;
                        Logueo.Evento("[PROCESAT112] Obteniendo Movimientos");
                        List<Movimiento> MisMovimientos = DAOComparador.ObtieneMovimientosPorProcesar(miArchivo.ID_Archivo); //ObtieneMovimientos(LosMovAProcesar, ID_Fichero);
                        if (MisMovimientos.Count == 0)
                        {

                            Logueo.Evento("ProcesaArch():NO HAY REGITROS QUE PROCESAR :" + miArchivo.ID_Archivo);
                            DAOFichero.GuardarErrorFicheroEnBD(miArchivo.ID_Archivo, "NO HAY REGITROS QUE PROCESAR :" + miArchivo.ID_Archivo);
                            DAOFichero.ActualizaEstatusFichero(miArchivo.ID_Archivo, EstatusFichero.Procesado);
                        }
                        Logueo.Evento("[PROCESAT112] Procesando movimientos");
                        foreach (Movimiento elMovimiento in MisMovimientos)
                        {
                            try
                            {
                                totalProceso++;
                                //Obtiene los datos de la Operacion Original  del Autorizador

                                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();


                                DAOUtilerias.getParametrosOperacionOriginal(elMovimiento.ClaveMA, elMovimiento.MonedaOriginal, elMovimiento.Autorizacion, elMovimiento.Importe, elMovimiento.ClaveEvento, ref losParametros, conn);

                                //asgina una cadena de default configurada en la BD
                                String LaCadenaComercial = PNConfig.Get("PROCESAT112", "CadenaComercial");
                                int laRespuesta = -1;

                                int SubEvento = 0;
                                using (SqlTransaction transaccionSQL = conn.BeginTransaction("T112_PROCESS"))
                                {


                                    // DAOUtilerias lasUtilerias = new DAOUtilerias(conn, transaccionSQL);

                                    try
                                    {

                                        String TipoOperacionCheckOut_O_Normal = "";
                                        try
                                        {

                                            TipoOperacionCheckOut_O_Normal = DAOUtilerias.ConsultaPrefijoProcesamientoOperacionPrevioCompensacion(elMovimiento.ClaveMA, elMovimiento.T112_CodigoMonedaLocal, float.Parse(elMovimiento.Importe), elMovimiento.Autorizacion, conn, transaccionSQL);

                                        }
                                        catch (Exception err)
                                        {
                                            TipoOperacionCheckOut_O_Normal = "";
                                        }




                                        string elMA = "";
                                        String elEvento = TipoOperacionCheckOut_O_Normal + "E" + elMovimiento.ClaveEvento + elMovimiento.MonedaOriginal;

                                        if (elMovimiento.ClaveMA.Length > 8)
                                            elMA = (elMovimiento.ClaveMA.Substring(elMovimiento.ClaveMA.Length - 8));
                                        if (elMovimiento.ClaveMA.Length < 8)
                                            elMA = (elMovimiento.ClaveMA);




                                        //este IF ES PARA ESPECIFICAR QUE SI SE DECLINA LA PLIZA DE UN T112 PARA ATM DE TOODS MODOS SE COPENSA SI ES QUE HAY OPERACION ORIGINAL
                                        if (PNConfig.Get("PROCESAT112", "EventosCompesarSinPoliza").Split(delimiter1, StringSplitOptions.RemoveEmptyEntries).Contains(elMovimiento.ClaveEvento))
                                        {
                                            ///asigna a la poliza el ID_Operacion de la Operacion Original
                                            ///
                                            try
                                            {

                                                float ImporteUSD = 0;

                                                DAOUtilerias.ActualizaCompensada(elMovimiento.ClaveMA, float.Parse(elMovimiento.Importe), ImporteUSD, elMovimiento.Autorizacion, elMovimiento.FechaOperacion, elMovimiento.Ticket, elMovimiento.T112_CodigoMonedaLocal, elMovimiento.T112_CuotaIntercambio, elMovimiento.T112_ImporteCompensadoDolar, elMovimiento.T112_ImporteCompensadoLocal, elMovimiento.T112_ImporteCompensadoPesos, elMovimiento.T112_IVA, Int32.Parse(losParametros["@ID_Operacion"].Valor), losParametros["@T112_ProcessingCode"].Valor, elMovimiento.T112_FechaPresentacion, elMovimiento.T112_NombreArchivo, conn, transaccionSQL);


                                            }
                                            catch (Exception ERR)
                                            {
                                                Logueo.Evento("[EventoManual] NO SE ACTUALIZO NINGUNA OPERACION: para el MEDIOACCESO:" + elMA + "," + ERR.Message);
                                                throw new Exception("[EventoManual] NO SE ACTUALIZO NINGUNA OPERACION para el MEDIOACCESO:" + elMA);
                                            }

                                        }
                                        else
                                        {


                                            string[] losEventosAEjecutar = PNConfig.Get("PROCESAT112", elEvento).Split(delimiter1, StringSplitOptions.RemoveEmptyEntries);

                                            obtieneParametros(ref losParametros, elMovimiento, TipoOperacionCheckOut_O_Normal, conn, transaccionSQL);

                                            //SI NO HAY EJECUCION DE EVENTO FIJO EN EL PN, ENTONCES TOMA LOS EVENTOS DE LA PERTNENCIA
                                            if (losEventosAEjecutar.Length == 0)
                                            {


                                                Regla laPertenencia = DAOPertenencia.getParamentrosStoredPertenencia(conn, transaccionSQL);


                                                int ID_Pertenencia = DAOPertenencia.getPertenencia(laPertenencia, ref losParametros, conn, transaccionSQL);

                                                try
                                                {
                                                    //obtiene la cadena o cliente de la tarjeta
                                                    LaCadenaComercial = losParametros["@ClaveCadenaComercial"].Valor;

                                                }
                                                catch (Exception err)
                                                {

                                                }

                                                if (ID_Pertenencia == 0)
                                                {
                                                    throw new Exception("NO HAY PERTENENCIA [" + ID_Pertenencia + "]: TARJETA:" + elMovimiento.ClaveMA.Substring(10) + "; IMPORTE: " + elMovimiento.Importe + "; Operacion" + elMovimiento.ClaveEvento);
                                                }

                                                List<String> misEventos = DAOPertenencia.getEventosAEjecutarPorPertenencia(ID_Pertenencia, conn, transaccionSQL);

                                                if (misEventos.Count == 0)
                                                {
                                                    //throw new Exception("NO HAY EVENTOS LIGADOS A LA PERTENENCIA [" + ID_Pertenencia + "]: TARJETA:" + elMovimiento.ClaveMA.Substring(10) + "; IMPORTE: " + elMovimiento.Importe + "; Operacion" + elMovimiento.ClaveEvento);
                                                    Logueo.Evento("NO HAY EVENTOS LIGADOS A LA PERTENENCIA[" + ID_Pertenencia + "]: " + "ID_Operacion:" + losParametros["@ID_Operacion"].Valor + " TARJETA: " + elMovimiento.ClaveMA.Substring(10) + "; IMPORTE: " + elMovimiento.Importe + "; Operacion" + elMovimiento.ClaveEvento + ", Se Procesará y se Tomará solo como Cambio de Estatus en caso que aplique");
                                                    try
                                                    {

                                                        float ImporteUSD = 0;

                                                        DAOUtilerias.ActualizaCompensada(elMovimiento.ClaveMA, float.Parse(elMovimiento.Importe), ImporteUSD, elMovimiento.Autorizacion, elMovimiento.FechaOperacion, elMovimiento.Ticket, elMovimiento.T112_CodigoMonedaLocal, elMovimiento.T112_CuotaIntercambio, elMovimiento.T112_ImporteCompensadoDolar, elMovimiento.T112_ImporteCompensadoLocal, elMovimiento.T112_ImporteCompensadoPesos, elMovimiento.T112_IVA, Int32.Parse(losParametros["@ID_Operacion"].Valor), losParametros["@T112_ProcessingCode"].Valor, elMovimiento.T112_FechaPresentacion, elMovimiento.T112_NombreArchivo, conn, transaccionSQL);


                                                    }
                                                    catch (Exception ERR)
                                                    {
                                                        Logueo.Evento("[EventoManual] NO SE ACTUALIZO NINGUNA OPERACION: para el MEDIOACCESO:" + elMA + "," + ERR.Message);
                                                        throw new Exception("[EventoManual] NO SE ACTUALIZO NINGUNA OPERACION para el MEDIOACCESO:" + elMA);
                                                    }
                                                }

                                                losEventosAEjecutar = new string[misEventos.Count];

                                                int Index = 0;

                                                foreach (String unEvento in misEventos)
                                                {
                                                    losEventosAEjecutar[Index] = unEvento;
                                                    Index++;
                                                }
                                            }
                                            else
                                            {
                                                throw new Exception(" NO HAY EVENTOS CONFIGURADOS PARA EJECUTAR: TARJETA:" + elMovimiento.ClaveMA.Substring(10) + "; IMPORTE: " + elMovimiento.Importe + "; Operacion" + elMovimiento.ClaveEvento);
                                            }


                                            foreach (String elEventoToEjecutar in losEventosAEjecutar)
                                            {

                                                Logueo.Evento("INICIA LA EJECUCION DEL EVENTO: " + elMovimiento.ClaveEvento + ": " + elEventoToEjecutar + ", MA: " + elMA + ", Importe:" + elMovimiento.Importe);

                                                laRespuesta = LNProcesaMovimiento.ProcesarMovimiento(elMovimiento, elEventoToEjecutar, LaCadenaComercial, elMovimiento.ClaveEvento, TipoOperacionCheckOut_O_Normal, losParametros, conn, transaccionSQL, connFichero);

                                                if (laRespuesta == 0)
                                                {
                                                    Logueo.Evento("EVENTO EJECUTADO CON EXITO: " + elMovimiento.ClaveEvento + ": " + elEventoToEjecutar + ", MA: " + elMA + ", Importe:" + elMovimiento.Importe);
                                                }
                                                else
                                                {
                                                    throw new Exception(" ERROR AL GENERAR MOVIMIENTO CODIGO[" + laRespuesta + "] " + elMA + " EVENTO:" + elEvento + ", Importe:" + elMovimiento.Importe);
                                                    //  elEvento = "";
                                                }
                                            }
                                        }

                                        totalProcesoOK++;
                                        transaccionSQL.Commit();

                                    }
                                    catch (Exception err)
                                    {
                                        totalProcesoNOK++;
                                        transaccionSQL.Rollback();
                                        Logueo.Error("ProcesaArch(): ERROR AL GENERAR MOVIMIENTO " + elMovimiento.ClaveMA + " " + err.Message);
                                    }

                                }
                            }
                            catch (Exception ERR)
                            {
                                Logueo.Error("ProcesaArch(): ERROR AL GENERAR MOVIMIENTO " + elMovimiento.ClaveMA + " " + ERR.Message);
                            }

                        }

                        MisMovimientos = null;
                        conn.Close();
                        connFichero.Close();
                        Logueo.Evento("ProcesaArch():SE PROCESARON  " + totalProceso + " REGISTROS. ARCHIVO:" + miArchivo.NombreArchivoDetectado);
                        DAOFichero.GuardarErrorFicheroEnBD(miArchivo.ID_Archivo, "SE PROCESARON  " + totalProceso + " REGISTROS. ARCHIVO:" + miArchivo.NombreArchivoDetectado);
                        DAOFichero.ActualizaEstatusFichero(miArchivo.ID_Archivo, EstatusFichero.Procesado);

                    }
                    catch (Exception ERR)
                    {
                        Logueo.Error("ProcesaArch(): " + ERR.Message);
                        DAOFichero.GuardarErrorFicheroEnBD(miArchivo.ID_Archivo, miArchivo.NombreArchivoDetectado + ": ERROR " + ERR.Message);
                        DAOFichero.ActualizaEstatusFichero(miArchivo.ID_Archivo, EstatusFichero.ProcesadoConErrores);
                    }
                    
                }
                catch (Exception ERR)
                {
                    Logueo.Error("ProcesaArch(): " + ERR.Message);
                    DAOFichero.GuardarErrorFicheroEnBD(miArchivo.ID_Archivo, miArchivo.NombreArchivoDetectado + ": ERROR " + ERR.Message);
                    DAOFichero.ActualizaEstatusFichero(miArchivo.ID_Archivo, EstatusFichero.ProcesadoConErrores);
                }

                laRespuestadelFichero.OperacionesConErrores = totalProcesoNOK;
                laRespuestadelFichero.OperacionesSinErrores = totalProcesoOK;



                laRespuestadelFichero.CodigoRespuesta = 0;
                

                return laRespuestadelFichero;

            }
            catch (Exception err)
            {
                Logueo.Error("ProcesaArch(): " + err.Message);
               // DAOFichero.GuardarErrorFicheroEnBD(miArchivo.ID_Fichero, "ProcesaArch():ERROR " + err.Message);
                //DAOFichero.ActualizaEstatusFichero(miArchivo.ID_Fichero, EstatusFichero.ProcesadoConErrores);
                laRespuestadelFichero.CodigoRespuesta = 8;
                return laRespuestadelFichero;
            }
        
        }

        internal static string GetInsertaFicheroDetalleBulkSPName()
        {
            if (T112_FILE_VERSION.Equals(ARCHIVOS_T112_NUEVA_VERSION_IDENTIFICADOR))
            {
                return "proc_InsertaFicheroDetalleBulkV2";
            }
            else
            {
                return "proc_InsertaFicheroDetalleBulk";
            }
        }

        private static void obtieneParametros(ref Dictionary<String, Parametro> losParametros, Movimiento elMovimiento, String DrafCaptureFlag, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {
            //Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();
            try
            {

                //Se consultan los parámetros del contrato
                Dictionary<String, Parametro> losParametrosContrato = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato("", elMovimiento.ClaveMA, "", "PROCNOCT", elMovimiento.MonedaOriginal, float.Parse(elMovimiento.ImporteMonedaOriginal), connOperacion, transaccionSQL);


                //almacena los parametros de contrato en el  set de parametros que llega

                foreach (Parametro elParam in losParametrosContrato.Values)
                {
                    losParametros[elParam.Nombre] = elParam;
                }

                losParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = elMovimiento.Importe, Descripcion = "Importe" };
                losParametros["@MedioAcceso"] = new Parametro() { Nombre = "@MedioAcceso", Valor = elMovimiento.ClaveMA };
                losParametros["@TipoMedioAcceso"] = new Parametro() { Nombre = "@TipoMedioAcceso", Valor = "TAR" };
                losParametros["@Tarjeta"] = new Parametro() { Nombre = "@Tarjeta", Valor = elMovimiento.ClaveMA };
                losParametros["@ReferenciaNumerica"] = new Parametro() { Nombre = "@ReferenciaNumerica", Valor = elMovimiento.ReferenciaNumerica };
                losParametros["@Autorizacion"] = new Parametro() { Nombre = "@Autorizacion", Valor = elMovimiento.Autorizacion };
                losParametros["@FechaOperacion"] = new Parametro() { Nombre = "@FechaOperacion", Valor = elMovimiento.FechaOperacion };
                losParametros["@FechaAplicacion"] = new Parametro() { Nombre = "@FechaAplicacion", Valor = elMovimiento.FechaOperacion };
                losParametros["@Ticket"] = new Parametro() { Nombre = "@Ticket", Valor = elMovimiento.Ticket };

                losParametros["@ProcessingCode"] = new Parametro() { Nombre = "@ProcessingCode", Valor = elMovimiento.ClaveEvento };
                losParametros["@T112_ProcessingCode"] = new Parametro() { Nombre = "@ProcessingCode", Valor = elMovimiento.ClaveEvento };

                if (elMovimiento.MonedaOriginal.Equals("840"))
                {
                    losParametros["@ImporteOriginal_USD"] = new Parametro() { Nombre = "@ImporteOriginal_USD", Valor = elMovimiento.ImporteMonedaOriginal };
                    losParametros["@ImporteOriginal_MXN"] = new Parametro() { Nombre = "@ImporteOriginal_MXN", Valor = "0" };
                    losParametros["@Imp_08_T112"] = new Parametro() { Nombre = "@Imp_08_T112", Valor = elMovimiento.ImporteMonedaOriginal };
                }
                else if (elMovimiento.MonedaOriginal.Equals("484")) //pesos 
                {
                    losParametros["@ImporteOriginal_USD"] = new Parametro() { Nombre = "@ImporteOriginal_USD", Valor = "0" };
                    losParametros["@ImporteOriginal_MXN"] = new Parametro() { Nombre = "@ImporteOriginal_MXN", Valor = elMovimiento.ImporteMonedaOriginal };
                    losParametros["@Imp_08_T112"] = new Parametro() { Nombre = "@Imp_08_T112", Valor = elMovimiento.ImporteMonedaOriginal };
                }

                //nuevos paramentros T112 
                losParametros["@T112_CodigoMonedaLocal"] = new Parametro() { Nombre = "@T112_CodigoMonedaLocal", Valor = elMovimiento.T112_CodigoMonedaLocal };
                losParametros["@T112_CuotaIntercambio"] = new Parametro() { Nombre = "@T112_CuotaIntercambio", Valor = elMovimiento.T112_CuotaIntercambio };
                losParametros["@T112_ImporteCompensadoDolar"] = new Parametro() { Nombre = "@T112_ImporteCompensadoDolar", Valor = elMovimiento.T112_ImporteCompensadoDolar };
                losParametros["@T112_ImporteCompensadoLocal"] = new Parametro() { Nombre = "@T112_ImporteCompensadoLocal", Valor = elMovimiento.T112_ImporteCompensadoLocal };
                losParametros["@T112_ImporteCompensadoPesos"] = new Parametro() { Nombre = "@T112_ImporteCompensadoPesos", Valor = elMovimiento.T112_ImporteCompensadoPesos };
                losParametros["@T112_IVA"] = new Parametro() { Nombre = "@T112_IVA", Valor = elMovimiento.T112_IVA };
                losParametros["@T112_NombreArchivo"] = new Parametro() { Nombre = "@T112_NombreArchivo", Valor = elMovimiento.T112_NombreArchivo };
                losParametros["@T112_FechaPresentacion"] = new Parametro() { Nombre = "@T112_FechaPresentacion", Valor = elMovimiento.T112_FechaPresentacion };

                //PARA INDICAR SI ES CHECKoUT O SOLO ES UNA COMPENSACION NORMAL  
                losParametros["@DrafCaptureFlag"] = new Parametro() { Nombre = "@DrafCaptureFlag", Valor = DrafCaptureFlag };

            }
            catch (Exception err)
            {

            }


        }

        private static List<Movimiento> ObtieneMovimientos(DataTable losMovimientos, Int64 ID_Fichero)
        {
            try
            {
                List<Movimiento> losResultados = new List<Movimiento>();
                int regi = 0;

                foreach (DataRow row in losMovimientos.Rows)
                {
                    try
                    {
                        regi++;
                        Movimiento dato = new Movimiento();
                        dato.ClaveMA = row["C002"].ToString();
                        dato.IdColectiva = 0;

                        dato.TipoMA = "TAR";
                        dato.ClaveEvento = row["C001"].ToString();
                        dato.Observaciones = "PROCESAMIENTO AUTOMATICO T112";
                        dato.Autorizacion = row["C018"].ToString();
                        dato.FechaOperacion = row["C005"].ToString();

                        dato.ReferenciaNumerica = row["ID_FicheroDetalle"].ToString();
                        dato.Ticket = "";
                        dato.MonedaOriginal = row["C009"].ToString();

                        dato.T112_CodigoMonedaLocal = row["C007"].ToString();
                        dato.T112_ImporteCompensadoLocal = (float.Parse(row["C006"].ToString()) / 100).ToString();

                        dato.T112_CuotaIntercambio = (float.Parse(row["C020"].ToString()) / 100).ToString();

                        if (row["C007"].ToString().Equals("484")) //es Pesos
                        {
                            dato.T112_ImporteCompensadoDolar = "0";
                            dato.T112_ImporteCompensadoPesos = (float.Parse(row["C006"].ToString()) / 100).ToString();

                            dato.Importe = (float.Parse(row["C006"].ToString()) / 100).ToString();
                            dato.ImporteMonedaOriginal = (float.Parse(row["C006"].ToString()) / 100).ToString();
                            dato.MonedaOriginal = row["C007"].ToString();
                        }
                        else
                        {

                            if (row["C009"].ToString().Equals("840")) //es Dolar
                            {
                                dato.T112_ImporteCompensadoDolar = (float.Parse(row["C008"].ToString()) / 100).ToString();
                                dato.T112_ImporteCompensadoPesos = "0";

                            }
                            else if (row["C009"].ToString().Equals("484")) //pesos
                            {
                                dato.T112_ImporteCompensadoDolar = "0";
                                dato.T112_ImporteCompensadoPesos = (float.Parse(row["C008"].ToString()) / 100).ToString();
                            }

                            dato.Importe = (float.Parse(row["C008"].ToString()) / 100).ToString();
                            dato.ImporteMonedaOriginal = (float.Parse(row["C008"].ToString()) / 100).ToString();
                            dato.MonedaOriginal = row["C009"].ToString();
                        }

                        try
                        {
                            dato.T112_IVA = (float.Parse(row["C050"].ToString().Substring(3, 12)) / 100).ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_IVA = "0";
                            Logueo.Error("ObtieneMovimientos(): error al Obtenr el Campo IVA [" + regi + "] Valor en T112: [" + row["C050"].ToString() + "];" + err.Message);

                        }

                        try
                        {
                            dato.T112_FechaPresentacion = row["FechaPresentacion"].ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_FechaPresentacion = DateTime.Now.ToString("yyyy-MM-dd");
                            Logueo.Error("ObtieneMovimientos(): error al Obtenr el Campo Fecha Presentacion" + err.Message);

                        }

                        try
                        {
                            dato.T112_NombreArchivo = row["NombreArchivo"].ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_NombreArchivo = "";
                            Logueo.Error("ObtieneMovimientos(): error al Obtenr el Campo Nombre de Archivo" + err.Message);

                        }

                        losResultados.Add(dato);

                    }
                    catch (Exception err)
                    {
                        Logueo.Error("ObtieneMovimientos(): Formato de Registro no valido [" + regi + "] " + row.ToString() + ";" + err.Message);
                    }
                }

                return losResultados;

            }
            catch (Exception err)
            {
                Logueo.Error("ObtieneMovimientos(): " + err.Message);
                DAOFichero.GuardarErrorFicheroEnBD(ID_Fichero, " ERROR:" + err.Message);
                DAOFichero.ActualizaEstatusFichero(ID_Fichero, EstatusFichero.ProcesadoConErrores);
                throw new Exception("Error al Obtener los Movimientos");
            }
        }


        public static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table>";
            //add header row
            //html += "<tr>";
            //for (int i = 0; i < dt.Columns.Count; i++)
            //    html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            //html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
        public static void ObtieneRegistrosDeArchivo(String elPath, ref Archivo elArchivo)
        {
            try
            {
                StreamReader objReader = new StreamReader(elPath);
                string sLine = "";
                //List<Fila> losDetalles = new List<Fila>();
                int noFilaDeARchivo = 0;
                String UltimaFilaLeida = "";

                try
                {
                    while (sLine != null)
                    {
                        sLine = objReader.ReadLine();

                        if (sLine == null)
                        {
                            break;
                        }



                        //DECODIFICA HEADER

                        //if ((noFilaDeARchivo == 0) & (elArchivo.LID_Header != 0))
                        //{
                        //    //losDetalles.Add(DecodificaFila(sLine,elArchivo.laConfiguracionHeaderLectura));
                        //    if (sLine.Substring(0, 7).Contains(elArchivo.ClaveHeader))
                        //    {
                        //        elArchivo.Header = DecodificaFila(sLine, elArchivo.laConfiguracionHeaderLectura);
                        //    }
                        //}

                        //DECODIFICA DETALLES
                        //if ((noFilaDeARchivo > 0))
                        //{
                        //    if (sLine.Substring(0, 5).Contains(elArchivo.ClaveRegistro))
                        //    {
                        Fila unaFilaNueva = DecodificaFila(sLine, elArchivo.laConfiguracionDetalleLectura);
                        elArchivo.LosDatos.Add(unaFilaNueva);
                        //elArchivo.FechaDatos = unaFilaNueva.losCampos[8].ToString();
                        //    }
                        //}

                        //if (()) //VALIDAR SI LA FILA ACTUAL ES EMV entonces recorrer la siguiente y tomarla como DETALLE EXTENDIDO
                        //{
                        //    sLine = objReader.ReadLine();

                        //    elArchivo.losDatos.Add(DecodificaFila(sLine, elArchivo.laConfiguracionDetalleExtraLectura));
                        //}

                        noFilaDeARchivo++; //INCREMENTA LA FILA LEIDA

                        UltimaFilaLeida = sLine;
                    }

                    //DECODIFICA FOOTER
                    //if ((noFilaDeARchivo > 0) & (elArchivo.LID_Footer != 0))
                    //{
                    //    if (UltimaFilaLeida.Substring(0, 7).Contains(elArchivo.ClaveFooter))
                    //    {
                    //        elArchivo.Footer = DecodificaFila(UltimaFilaLeida, elArchivo.laConfiguracionFooterLectura);
                    //    }
                    //}

                    elArchivo.Nombre = elPath;

                }
                catch (Exception err)
                {
                    Logueo.Error("ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source);
                }
                finally
                {
                    objReader.Close();
                }
            }
            catch (Exception err)
            {
                Logueo.Error("ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source);
            }





            return;
        }

        public static Fila DecodificaFila(String Cadena, FilaConfig laConfiguracionDelaFila)
        {
            Fila laFila = new Fila();
            try
            {
                laFila.laConfigDeFila = laConfiguracionDelaFila;
                laFila.DetalleCrudo = Cadena;

                //decodifica el Header.
                if (laConfiguracionDelaFila.PorSeparador)
                {
                    string[] losDato = Cadena.Split(laConfiguracionDelaFila.CaracterSeparacion.ToCharArray());

                    for (int k = 0; k < losDato.Length; k++)
                    {
                        laFila.losCampos.Add(k + 1, losDato[k]);
                    }

                }
                else if (laConfiguracionDelaFila.PorLongitud)
                {

                    Int32 elNumeroCampo = 1;

                    while (Cadena.Length != 0)
                    {
                        try
                        {
                            CampoConfig unaConfig = laConfiguracionDelaFila.LosCampos[elNumeroCampo];

                            laFila.losCampos.Add(elNumeroCampo, Cadena.Substring(0, unaConfig.Longitud));

                            Cadena = Cadena.Substring(unaConfig.Longitud);

                            elNumeroCampo++;
                        }
                        catch (Exception err)
                        {
                            Logueo.Error("DecodificaFila(): Campo: " + elNumeroCampo + ", " + err.Message + " " + err.StackTrace + ", " + err.Source);
                            throw new Exception("DecodificaFila(): Campo: " + elNumeroCampo + ", " + err.Message + " " + err.StackTrace + ", " + err.Source);
                        }
                    }

                }


            }
            catch (Exception err)
            {
                Logueo.Error("DecodificaFila()" + err.Message + " " + err.StackTrace + ", " + err.Source);
            }

            return laFila;
        }

        public static void crearDirectorio()
        {//
            string directorioEscucha = PNConfig.Get("PROCESAT112", "DirectorioEntrada");
            string directorioSalida = PNConfig.Get("PROCESAT112", "DirectorioSalida");

            if (!Directory.Exists(directorioEscucha))
                Directory.CreateDirectory(directorioEscucha);


            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);

        }


        public static void EscucharDirectorio()
        {
            try
            {
                Logueo.Evento("Inicia Escucha de Carpeta ");

                crearDirectorio();

                Logueo.Evento("Inicia la escucha de la carpeta: " + PNConfig.Get("PROCESAT112", "DirectorioEntrada") + " en espera de archivos. PROCESAT112");


                elObservador.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);

                elObservador.Created += alCambiar;
                elObservador.Error += alOcurrirUnError;

                elObservador.EnableRaisingEvents = true;

            }
            catch (Exception err)
            {
                Logueo.Error("EscucharDirectorio(): " + err.Message);
            }
        }

        public static void DetenerDirectorio()
        {
            try
            {

                Logueo.Evento("Se Detiene a escucha del Directorio: " + PNConfig.Get("PROCESAT112", "DirectorioEntrada"));


                elObservador.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);

                elObservador.Created -= alCambiar;
                elObservador.Error -= alOcurrirUnError;

                elObservador.EnableRaisingEvents = false;

            }
            catch (Exception err)
            {
                Logueo.Error("EscucharDirectorio(): " + err.Message);
            }
        }

        public static void alCambiar(object source, FileSystemEventArgs el)
        {
            try
            {
                //detener el file wathcer despues de que se detecto un cambio para evitar varios disparadores
                DetenerDirectorio();

                enProceso = true;
                ////WatcherChangeTypes elTipoCambio = el.ChangeType;
                //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
                ////Logueo.Evento("Hubo un Cambio (1) [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("PROCESAT112", "DirectorioEntrada") + " el se recibio el archivo : " + el.FullPath);
                Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                try
                {
                    Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                    Logueo.Evento("Esperando " + (Int32.Parse(PNConfig.Get("PROCESAT112", "MinEsperaProceso"))).ToString() + " Minutos.");
                    Thread.Sleep(1000 * 60 * Int32.Parse(PNConfig.Get("PROCESAT112", "MinEsperaProceso")));

                   

                }
                catch (Exception err)
                {
                    Logueo.Evento("Inicia espera defualt por no tener configuracion en 'MinEsperaProceso'. Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                    Thread.Sleep(1000 * 60 * 10);
                    Logueo.Evento("Esperando 10 Minutos.");
                }



                ///Logueo.Evento("INICIO DE PROCESO DEL ARCHIVO  (1):" + el.FullPath);
                RespuestaProceso laRespuestProceso = new RespuestaProceso();

                try
                {

                    lock (balanceLock)

                    {
                        DirectoryInfo directory = new DirectoryInfo(PNConfig.Get("PROCESAT112", "DirectorioEntrada"));

                        //  String Prefijo = "C360";
                        //String laFecha = "2016-03-27";
                        Logueo.Evento("[PROCESAT112] Obteniendo archivos configurados");
                        List<Archivo> losArchivos = DAOArchivo.ObtenerArchivosConfigurados();

                        //OBTENER TODOS LOS ARCHIVOS CONFIGURADOS EN LA BASE DE DATOS PARA OBTENER LOS PREFIJOS.
                        Logueo.Evento(String.Format("[PROCESAT112] Archivos configurados {0}", losArchivos.Count));
                        foreach (Archivo elArchivo in losArchivos)
                        {
                            Logueo.Evento("Procesando Archivo : " + elArchivo.Nombre);
                            FileInfo[] files = directory.GetFiles(elArchivo.Nombre + "*.*");

                            for (int i = 0; i < files.Length; i++)
                            {

                                String elpathFile = (((FileInfo)files[i]).FullName);
                                elArchivo.UrlArchivo = elpathFile;
                                string filename2 = Path.GetFileName(elpathFile);
                                LNArchivo.SetT112FileVersion(elArchivo);


                                try
                                {
                                    //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                                    Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                                    //string extension = Path.GetExtension(e.FullPath);
                                    string filename1 = Path.GetFileName(elpathFile);

                                    elArchivo.NombreArchivoDetectado = filename1;
                                    // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                                    string root1 = Path.GetDirectoryName(elpathFile);

                                    File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);

                                    elpathFile = root1 + "\\EN_PROCESO_" + filename1;

                                    FileFormat.PadFileLines(elpathFile, int.Parse(LNArchivo.GetLongFila(elArchivo)), ' ');




                                    elArchivo.UrlArchivo = elpathFile;

                                    Logueo.Evento("Procesar archivo desde hora programada [" + ((FileInfo)files[i]).FullName + "] ");


                                    //Abrir el Archivo
                                    //Crear los objetos Usuario por cada registro del 
                                    //iniicar el ciclo de actualizacion.
                                    laRespuestProceso = LNArchivo.ProcesaArch(elArchivo, elpathFile);

                                    //RENOMBRAR el ARCHIVO
                                    if (laRespuestProceso.CodigoRespuesta == 0)
                                    {
                                        Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                                        //string extension = Path.GetExtension(e.FullPath);
                                        string filename = Path.GetFileName(elpathFile);
                                        // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                                        string root = Path.GetDirectoryName(elpathFile);
                                        if (File.Exists(PNConfig.Get("PROCESAT112", "DirectorioSalida") + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "")))
                                        {
                                            File.Move(elpathFile, PNConfig.Get("PROCESAT112", "DirectorioSalida") + "\\PROCESADO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + filename.Replace("EN_PROCESO_", ""));
                                        }
                                        else
                                        {
                                            File.Move(elpathFile, PNConfig.Get("PROCESAT112", "DirectorioSalida") + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", ""));
                                        }

                                        DAOFichero.GuardarErrorFicheroEnBD(laRespuestProceso.ID_Fichero, "ALMACENADO EXITOSAMENTE EN LA BASE DE DATOS, ESPERAR HORA DE PROCESO");
                                        //para cuetion de pruebas de RM 2020-06-24 no se cambia el estatus  a 4
                                        DAOFichero.ActualizaEstatusFichero(laRespuestProceso.ID_Fichero, EstatusFichero.PorProcesar);


                                    }
                                }
                                catch (Exception err)
                                {
                                    if (File.Exists(PNConfig.Get("PROCESAT112", "DirectorioSalida") + "\\PROCESADO_CON_ERROR_" + filename2.Replace("EN_PROCESO_", "")))
                                    {
                                        File.Move(elpathFile, PNConfig.Get("PROCESAT112", "DirectorioSalida") + "\\PROCESADO_CON_ERROR_" + DateTime.Now.ToString("yyyyMMddHHmmss") + filename2.Replace("EN_PROCESO_", ""));
                                    }
                                    else
                                    {
                                        File.Move(elpathFile, PNConfig.Get("PROCESAT112", "DirectorioSalida") + "\\PROCESADO_CON_ERROR_" + filename2.Replace("EN_PROCESO_", ""));
                                    }
                                    Logueo.Error("alCambiar(): " + err.Message + ",  " + err.ToString());

                                    DAOFichero.GuardarErrorFicheroEnBD(laRespuestProceso.ID_Fichero, "PROCESADO CON ERROR:" + err.Message + ",  " + err.ToString());
                                    DAOFichero.ActualizaEstatusFichero(laRespuestProceso.ID_Fichero, EstatusFichero.ProcesadoConErrores);

                                    Console.WriteLine(err.ToString());
                                }
                            }

                        }
                    }

                }

                catch (Exception ex)
                {
                    Logueo.Error("alCambiar(): " + ex.Message + ",  " + ex.ToString());
                    Console.WriteLine(ex.ToString());

                }
                finally
                {
                    enProceso = false;
                }


            }
            catch (Exception err)
            {
                Logueo.Error("alCambiar(): " + err.Message + ",  " + err.ToString());
            }
            finally
            {
                //poner a escuchar otra vez el file watcher
                EscucharDirectorio();
            }
        }

        private static void SetT112FileVersion(Archivo elArchivo)
        {
            if (elArchivo.Nombre.Contains(ARCHIVOS_T112_NUEVA_VERSION_IDENTIFICADOR))
            {
                T112_FILE_VERSION = "V2";
            }
            else
            {
                T112_FILE_VERSION = "V1";
            }
        }

        public static void alOcurrirUnError(object source, ErrorEventArgs e)
        {
            try
            {
                Logueo.Error("alOcurrirUnError(): e: " + e.GetException().Message);
            }
            catch (Exception err)
            {
                Logueo.Error("alOcurrirUnError(): " + err.Message);
            }
        }


        internal static void EscibirArchivo(string elpathFile, DTOArchivo elArchivo, Archivo elArchivoFisico)
        {
            List<string> lineas = new List<string>();

            try
            {
                List<string> lineaInterna = new List<string>();
                lineaInterna.Add(elArchivoFisico.ClaveHeader);
                //lineaInterna.Add(elArchivoFisico.Nombre);//.ENC_Fecha.ToString(DTOConfiguracionArchivo.ENC_Fecha.Formato));

                if (elArchivoFisico.FechaDatos == null)
                {
                    //Path.GetFileName(elArchivoFisico.Nombre).Substring(Path.GetFileName(elArchivoFisico.Nombre).LastIndexOf('.')+1);
                    lineaInterna.Add(Path.GetFileName(elArchivoFisico.Nombre).Substring(Path.GetFileName(elArchivoFisico.Nombre).LastIndexOf('.') + 1));//
                                                                                                                                                        // elArchivo.ENC_Fecha.ToString("yymmdd");
                }
                else
                {
                    lineaInterna.Add(elArchivoFisico.FechaDatos.PadRight(6, ' '));//
                }


                lineaInterna.Add(elArchivoFisico.Prefijo.PadRight(15, ' '));//

                lineas.Add(string.Join("", lineaInterna));

                foreach (var fila in elArchivo.Filas)
                {
                    lineaInterna = new List<string>();


                    lineaInterna.Add(fila.Registro);

                    lineas.Add(string.Join("", lineaInterna));
                }

                lineaInterna = new List<string>();

                lineaInterna.Add(elArchivoFisico.ClaveFooter);
                lineaInterna.Add(elArchivo.PIE_NumeroRegistros.ToString().PadLeft(8, '0'));
                lineaInterna.Add((elArchivo.PIE_ImporteTotal * 100).ToString().PadLeft(15, '0').Replace(".0", ""));

                lineas.Add(string.Join("", lineaInterna));

                File.WriteAllLines(elpathFile, lineas);
            }
            catch (Exception err)
            {
                Logueo.Error("ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source);
            }

        }


    }
}
