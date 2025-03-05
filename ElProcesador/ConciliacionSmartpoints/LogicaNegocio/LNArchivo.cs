using CommonProcesador;
using ConciliacionSmartpoints.BaseDatos;
using ConciliacionSmartpoints.Entidades;
using ConciliacionSmartpoints.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.LogicaNegocio
{
     class LNArchivo
    {

        private static readonly string ARCHIVOS_T112_NUEVA_VERSION_IDENTIFICADOR = "V2";

        public static String T112_FILE_VERSION { get; set; }


        delegate void EnviarDetalle(String elDetalle);
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();
        public static Boolean enProceso = false;
        private static readonly object balanceLock = new object();
        private static FileSystemWatcher elObservador = new FileSystemWatcher(PNConfig.Get("ConciliacionSmart", "DirectorioEntrada"));

        public static Boolean ProcesaArch(Ficheros unFichero)
        {
            try
            {

                DatosBaseDatos laConfigForConmparacion = DAODatosBase.ObtenerConsulta(unFichero.IdArchivo);

                //Borrar datos de tablaDatosBD where IdArchivo
                //DAOFichero.ActializaIdEstatusFichero(IdFichero);
                DAOFichero.EliminaFicheroDatosBD(unFichero.IdFichero);

                //Insertar los datos obtenidos en la DB de Archivos
                int OperacionesBD = DAODatosBase.InsertaDatosEnTabla(laConfigForConmparacion, unFichero);

                //OBTINE LAS CONFIGURACIONES DE CAMPOS-FILAS
                List<ComparadorConfig> lasConfiguraciones = DAOComparador.ObtenerConfiguracionFila(unFichero.IdFichero);

                if (lasConfiguraciones.Count == 0)
                    return true;

                StringBuilder lasRespuesta = new StringBuilder();

                String laConsultaWhere = LNComparacion.GeneraConsultaWhereSQL(lasConfiguraciones);

                DAODatosBase.Proc_InsertaConsultaWhere(laConsultaWhere, unFichero.IdConsulta, unFichero.IdArchivo);

                //guardar laconsultaWhere FiltroSQL varchar tabla Configurarprocesamiento

                DataSet losNOArchivoSIBD = DAOComparador.ObtieneDatosNOArchivoSIBaseDatos(unFichero.IdFichero, laConsultaWhere);

                lasRespuesta.Append("Registros NO en Archivo SI en BD:");
                lasRespuesta.Append(losNOArchivoSIBD.Tables[0].Rows.Count);



                DataSet losSIArchivoSIBD = DAOComparador.ObtieneDatosSIArchivoSIBaseDatos(unFichero.IdFichero, laConsultaWhere);
                lasRespuesta.Append("\nRegistros SI en Archivo SI en BD:");
                lasRespuesta.Append(losSIArchivoSIBD.Tables[0].Rows.Count);


                DataSet losSIArchivoNOBD = DAOComparador.ObtieneDatosSIArchivoNOBaseDatos(unFichero.IdFichero, laConsultaWhere);
                lasRespuesta.Append("\nRegistros SI en Archivo NO en BD:");
                lasRespuesta.Append(losSIArchivoNOBD.Tables[0].Rows.Count);


                StringBuilder ElCuerpodelMail = new StringBuilder(File.ReadAllText(PNConfig.Get("ConciliacionSmart", "HTMLMailToSend")));
                ElCuerpodelMail = ElCuerpodelMail.Replace("[NOMBREARCHIVO]", unFichero.NombreFichero.Replace("EN_PROCESO_", ""));//QUITAMOS EL PREFIJO QUE INDICA QUE ESTA SIENDO PROCESADO
                //ElCuerpodelMail = ElCuerpodelMail.Replace("[OPERARCHIVO]", unArchivo.LosDatos.Count.ToString());
                ElCuerpodelMail = ElCuerpodelMail.Replace("[OPERBD]", OperacionesBD.ToString());
                ElCuerpodelMail = ElCuerpodelMail.Replace("[SIARCHIVOSIBD]", losSIArchivoSIBD.Tables[0].Rows.Count.ToString());
                ElCuerpodelMail = ElCuerpodelMail.Replace("[SIARCHIVONOBD]", losSIArchivoNOBD.Tables[0].Rows.Count.ToString());
                ElCuerpodelMail = ElCuerpodelMail.Replace("[NOARCHIVOSIBD]", losNOArchivoSIBD.Tables[0].Rows.Count.ToString());

    
                //LNMailing.EnviaResultadoConciliacion(ElCuerpodelMail.ToString(), unArchivo);

                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("getUsuarios(): " + err.Message);
                return false;
            }
        }

        public static RespuestaProceso ProcesaArchivo(Archivo unArchivo, String path)
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
                            ID_Fichero = DAOArchivo.GuardarFicheroEnBD(unArchivo, path, conn, transaccionSQL);
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
                            Boolean guardoLosDetalles = DAOArchivo.GuardarFicheroDetallesEnBD(unArchivo, ID_Fichero, ObtenerFormatoFMT(unArchivo), conn, transaccionSQL);
                            Logueo.Evento(path + ": TERMINA GUARDAR DETALLES EN BD");

                            if (!guardoLosDetalles)
                            {
                                transaccionSQL.Rollback();
                                Logueo.Evento(path + ": ERROR GUARDAR DATOS FICHERO EN BD");
                                Logueo.Error("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
                                //DAOFichero.GuardarErrorFicheroEnBD(ID_Fichero, "ERROR GUARDAR DATOS FICHERO EN BD [" + unArchivo.NombreArchivoDetectado + "]");
                                //DAOFichero.ActualizaEstatusFichero(ID_Fichero, EstatusFichero.ProcesadoConErrores);
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
        public static void ObtieneRegistrosDeArchivo(String elPath, ref Archivo elArchivo)
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

                    if ((noFilaDeARchivo == 0) & (elArchivo.LID_Header != 0))
                    {
                        //losDetalles.Add(DecodificaFila(sLine,elArchivo.laConfiguracionHeaderLectura));
                        if (sLine.Substring(0, 7).Contains(elArchivo.ClaveHeader))
                        {
                            elArchivo.Header = DecodificaFila(sLine, elArchivo.laConfiguracionHeaderLectura);
                        }
                    }

                    //DECODIFICA DETALLES
                    if ((noFilaDeARchivo > 0))
                    {
                        if (sLine.Substring(0, 10).Contains(elArchivo.ClaveRegistro))
                        {

                            elArchivo.LosDatos.Add(DecodificaFila(sLine, elArchivo.laConfiguracionDetalleLectura));
                        }
                    }

                    //if (()) //VALIDAR SI LA FILA ACTUAL ES EMV entonces recorrer la siguiente y tomarla como DETALLE EXTENDIDO
                    //{
                    //    sLine = objReader.ReadLine();

                    //    elArchivo.losDatos.Add(DecodificaFila(sLine, elArchivo.laConfiguracionDetalleExtraLectura));
                    //}

                    noFilaDeARchivo++; //INCREMENTA LA FILA LEIDA

                    UltimaFilaLeida = sLine;
                }

                //DECODIFICA FOOTER
                if ((noFilaDeARchivo > 0) & (elArchivo.LID_Footer != 0))
                {
                    if (UltimaFilaLeida.Substring(0, 7).Contains(elArchivo.ClaveFooter))
                    {
                        elArchivo.Footer = DecodificaFila(UltimaFilaLeida, elArchivo.laConfiguracionFooterLectura);
                    }
                }

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
                        laFila.losCampos.Add(k, losDato[0]);
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

        public static void EscucharDirectorio()
        {
            try
            {
                Logueo.Evento("Inicia Escucha de Carpeta");

                crearDirectorio();

                Logueo.Evento("Inicia la escucha de la carpeta: " + PNConfig.Get("ConciliacionSmart", "DirectorioEntrada") + " en espera de archivos. ConciliacionSmart");


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

        public static void crearDirectorio()
        {//
            string directorioEscucha = PNConfig.Get("ConciliacionSmart", "DirectorioEntrada");
            string directorioSalida = PNConfig.Get("ConciliacionSmart", "DirectorioSalida");

            if (!Directory.Exists(directorioEscucha))
                Directory.CreateDirectory(directorioEscucha);


            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);

        }

        public static void alCambiar(object source, FileSystemEventArgs el)
        {
            try
            {
                //detener el file wathcer despues de que se detecto un cambio para evitar varios disparadores
                DetenerDirectorio();

                enProceso = true;
                WatcherChangeTypes elTipoCambio = el.ChangeType;
                //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
                Logueo.Evento("Hubo un Cambio (1) [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("ConciliacionSmart", "DirectorioEntrada") + " el se recibio el archivo : " + el.FullPath);
                Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                try
                {
                    Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                    Logueo.Evento("Esperando " + (Int32.Parse(PNConfig.Get("ConciliacionSmart", "MinEsperaProceso"))).ToString() + " Minutos.");
                    Thread.Sleep(1000 * 60 * Int32.Parse(PNConfig.Get("ConciliacionSmart", "MinEsperaProceso")));



                }
                catch (Exception err)
                {
                    Logueo.Evento("Inicia espera defualt por no tener configuracion en 'MinEsperaProceso'. Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                    Thread.Sleep(1000 * 60 * 10);
                    Logueo.Evento("Esperando 10 Minutos.");
                }


                //Smartpoints sp = new Smartpoints();
                //sp.Procesar();

                Logueo.Evento("INICIO DE PROCESO DEL ARCHIVO  (1):" + el.FullPath);
                RespuestaProceso laRespuestProceso = new RespuestaProceso();

                try
                {

                    lock (balanceLock)

                    {
                        DirectoryInfo directory = new DirectoryInfo(PNConfig.Get("ConciliacionSmart", "DirectorioEntrada"));

                        //  String Prefijo = "C360";
                        //String laFecha = "2016-03-27";

                        List<Archivo> losArchivos = DAOArchivo.ObtenerArchivosConfigurados();

                        //OBTENER TODOS LOS ARCHIVOS CONFIGURADOS EN LA BASE DE DATOS PARA OBTENER LOS PREFIJOS.

                        foreach (Archivo elArchivo in losArchivos)
                        {
                            //Logueo.Evento("Procesando Archivo : " + elArchivo.Nombre);
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
                                    string filename1 = Path.GetFileName(elpathFile);

                                    elArchivo.NombreArchivoDetectado = filename1;
                                    string root1 = Path.GetDirectoryName(elpathFile);
                                    File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);
                                    elpathFile = root1 + "\\EN_PROCESO_" + filename1;


                                    elArchivo.UrlArchivo = elpathFile;
                                    Logueo.Evento("Procesar archivo desde hora programada [" + ((FileInfo)files[i]).FullName + "] ");


                                    //Abrir el Archivo
                                    //Crear los objetos Usuario por cada registro del 
                                    //iniicar el ciclo de actualizacion.
                                    laRespuestProceso = LNArchivo.ProcesaArchivo(elArchivo, elpathFile);

                                    //RENOMBRAR el ARCHIVO

                                    if (laRespuestProceso.CodigoRespuesta != 0)
                                    {
                                        string filename = Path.GetFileName(elpathFile);
                                        // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                                        string root = Path.GetDirectoryName(elpathFile);
                                        File.Move(elpathFile, PNConfig.Get("ConciliacionSmart", "Errores") + "\\PROCESADO_CON_ERROR_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + filename.Replace("EN_PROCESO_", ""));
                                    }
                                    if (laRespuestProceso.CodigoRespuesta == 0)
                                    {
                                        Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                                        //string extension = Path.GetExtension(e.FullPath);
                                        string filename = Path.GetFileName(elpathFile);
                                        // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                                        string root = Path.GetDirectoryName(elpathFile);
                                        if (File.Exists(PNConfig.Get("ConciliacionSmart", "DirectorioSalida") + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "")))
                                        {
                                            File.Move(elpathFile, PNConfig.Get("ConciliacionSmart", "DirectorioSalida") + "\\PROCESADO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + filename.Replace("EN_PROCESO_", ""));
                                        }
                                        else
                                        {
                                            File.Move(elpathFile, PNConfig.Get("ConciliacionSmart", "DirectorioSalida") + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", ""));
                                        }

                                        ////DAOFichero.GuardarErrorFicheroEnBD(laRespuestProceso.ID_Fichero, "ALMACENADO EXITOSAMENTE EN LA BASE DE DATOS, ESPERAR HORA DE PROCESO");
                                        ////DAOFichero.ActualizaEstatusFichero(laRespuestProceso.ID_Fichero, EstatusFichero.PorProcesar);


                                    }
                                }
                                catch (Exception err)
                                {
                                    if (File.Exists(PNConfig.Get("ConciliacionSmart", "DirectorioSalida") + "\\PROCESADO_CON_ERROR_" + filename2.Replace("EN_PROCESO_", "")))
                                    {
                                        File.Move(elpathFile, PNConfig.Get("ConciliacionSmart", "DirectorioSalida") + "\\PROCESADO_CON_ERROR_" + DateTime.Now.ToString("yyyyMMddHHmmss") + filename2.Replace("EN_PROCESO_", ""));
                                    }
                                    else
                                    {
                                        File.Move(elpathFile, PNConfig.Get("ConciliacionSmart", "DirectorioSalida") + "\\PROCESADO_CON_ERROR_" + filename2.Replace("EN_PROCESO_", ""));
                                    }
                                    ////Logueo.Error("alCambiar(): " + err.Message + ",  " + err.ToString());

                                    ////DAOFichero.GuardarErrorFicheroEnBD(laRespuestProceso.ID_Fichero, "PROCESADO CON ERROR:" + err.Message + ",  " + err.ToString());
                                    ////DAOFichero.ActualizaEstatusFichero(laRespuestProceso.ID_Fichero, EstatusFichero.ProcesadoConErrores);

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

        public static void DetenerDirectorio()
        {
            try
            {

                Logueo.Evento("Se Detiene a escucha del Directorio: " + PNConfig.Get("ConciliacionSmart", "DirectorioEntrada"));


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
            //if (T112_FILE_VERSION.Equals(ARCHIVOS_T112_NUEVA_VERSION_IDENTIFICADOR))
            //{
            //    Logueo.Evento("FMT V2 FMTConfig_V2");
            //    return "FMTConfig_V2";
            //}

            return "FMTConfig";
        }

        

        internal static string GetInsertaFicheroDetalleBulkSPName()
        {
            //if (T112_FILE_VERSION.Equals(ARCHIVOS_T112_NUEVA_VERSION_IDENTIFICADOR))
            //{
            //    return "proc_InsertaFicheroDetalleBulkV2";
            //}
            //else
            //{
                return "proc_InsertaFicheroDetalleBulk_SmartPoint";
            //}
        }


    }
}
