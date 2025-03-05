//#define CACAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using DNU_BovedaDigitalCargaTarjetasAnonimas.Entidades;
using DNU_BovedaDigitalCargaTarjetasAnonimas.BaseDatos;
using System.Threading;
using DNU_BovedaDigitalCargaTarjetasAnonimas.Utilidades;
using log4net;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;

namespace DNU_BovedaDigitalCargaTarjetasAnonimas.LogicaNegocio
{
    public class LNArchivo
    {
        private static string directorioSalida;
        private string directorio;
        private string tipoColectiva;
        delegate void EnviarDetalle(String elDetalle);
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();
        string log; string ip;

        public LNArchivo(string directorio, string tipoColectiva)//, string log, string ip)
        {
            this.directorio = directorio;
            this.tipoColectiva = tipoColectiva;
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            log = ThreadContext.Properties["log"].ToString();
            ip = ThreadContext.Properties["ip"].ToString();
        }

        //public static Boolean ProcesaArch(String path, String claveColectiva, string tipoManuFactura, ref List<Archivo> lstArchvio)
        //{
        //    try
        //    {

        //        Int64 ID_Fichero = 0;

        //        ObtieneRegistrosDeArchivo(path, ref lstArchvio);

        //        if (!DAOArchivo.ColectivaValida(claveColectiva, DBProcesadorArchivo.BDLecturaAutorizador))
        //            throw new Exception("LA COLECTIVA NO EXISTE O NO CUMPLE LAS VALIDACIONES");

        //        using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
        //        {
        //            conn.Open();

        //            using (SqlTransaction transaccionSQL = conn.BeginTransaction())
        //            {
        //                try
        //                {

        //                    //GUARDA EL FICHERO
        //                    ID_Fichero = DAOArchivo.GuardarFicheroEnBD(path, conn, transaccionSQL);

        //                    if (ID_Fichero == 0)
        //                    {
        //                        transaccionSQL.Rollback();
        //                        Logueo.Error("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
        //                        throw new Exception("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
        //                    }


        //                    //Guarda los detalles
        //                    DataTable dtDetalle = DAOArchivo.GeneraDataTableDetalle(lstArchvio, ID_Fichero);

        //                    Boolean guardoLosDetalles = DAOArchivo.GuardarFicheroDetallesEnBD(dtDetalle);

        //                    if (!guardoLosDetalles)
        //                    {
        //                        transaccionSQL.Rollback();
        //                        Logueo.Error("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
        //                        throw new Exception("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
        //                    }

        //                    transaccionSQL.Commit();
        //                }
        //                catch (Exception err)
        //                {
        //                    Logueo.Error("ProcesaArch(): ERROR AL ALMACENAR EL FICHERO: " + err.Message);
        //                    transaccionSQL.Rollback();
        //                }
        //            }
        //        }




        //        if (ID_Fichero == 0)
        //        {
        //            throw new Exception("NO SE PUDO OBTENER UN ID_FICHERO VALIDO");
        //        }
        //        StringBuilder sb = new StringBuilder();

        //        var ejecucionCorrecta = ejecutaActivacion(ref lstArchvio, ref sb, claveColectiva, tipoManuFactura);

        //        renombraArchivo(path, ejecucionCorrecta, sb);

        //        sb.Clear();

        //        return true;


        //    }
        //    catch (Exception err)
        //    {
        //        Logueo.Error(String.Format("[AltaTarjetasAnonimas]ProcesaArch: {0} --- Stack : {1} ", err.Message, err.StackTrace));
        //        renombraArchivoPorError(path, lstArchvio, err.Message);
        //        return false;
        //    }
        //}

        //credito
        public static Boolean ProcesaArch(String path, String claveColectiva, string tipoManuFactura, ref List<Archivo> lstArchvio, string subproducto = null, bool subproductoValido = false, bool esCredito = false, string log = "", string ip = "")
        {
            try
            {

                Int64 ID_Fichero = 0;
                Logueo.Evento("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Obteniendo registros]");

               // LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Obteniendo registros]");

                ObtieneRegistrosDeArchivo(path, ref lstArchvio, log, ip);
                if (lstArchvio.Count == 0) {
                    LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Archivo Vacio]");
                    throw new Exception("Archivo Vacio");
                }

                if (!subproductoValido)
                {
                    LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [SUBPRODUCTO INVALIDO]");
                    throw new Exception("SUBPRODUCTO INVALIDO");
                }

                //if (!DAOArchivo.ColectivaValida(claveColectiva, DBProcesadorArchivo.BDLecturaAutorizador, log, ip))
                //{
                //    LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [LA COLECTIVA NO EXISTE O NO CUMPLE LAS VALIDACIONES]");
                //    throw new Exception("LA COLECTIVA NO EXISTE O NO CUMPLE LAS VALIDACIONES");
                // }

                LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Guardando fichero en base de datos]");

                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    DataTable dtDetalle = null;
                    conn.Open();
                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {

                            //GUARDA EL FICHERO
                            ID_Fichero = DAOArchivo.GuardarFicheroEnBD(path, conn, transaccionSQL, log, ip);

                            if (ID_Fichero == 0)
                            {
                                transaccionSQL.Rollback();
                                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO]");
                                throw new Exception("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
                            }


                            //Guarda los detalles
                            Logueo.Evento("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Generando detalle]");
                           // LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Generando detalle]");

                            dtDetalle = DAOArchivo.GeneraDataTableDetalle(lstArchvio, ID_Fichero, subproducto);

                            Boolean guardoLosDetalles = DAOArchivo.GuardarFicheroDetallesEnBD(dtDetalle);

                            if (!guardoLosDetalles)
                            {
                                transaccionSQL.Rollback();
                                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO]");
                                throw new Exception("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
                            }

                            transaccionSQL.Commit();
                        }
                        catch (Exception err)
                        {
                            LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): ERROR AL ALMACENAR EL FICHERO: " + err.Message + "]");
                            transaccionSQL.Rollback();
                        }
                    }
                }




                if (ID_Fichero == 0)
                {
                    Logueo.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [NO SE PUDO OBTENER UN ID_FICHERO VALIDO]");

                   // LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [NO SE PUDO OBTENER UN ID_FICHERO VALIDO]");
                    throw new Exception("NO SE PUDO OBTENER UN ID_FICHERO VALIDO");
                }
                StringBuilder sb = new StringBuilder();

                var ejecucionCorrecta = ejecutaActivacion(ref lstArchvio, ref sb, claveColectiva, tipoManuFactura, subproducto, subproductoValido, esCredito, log, ip);

                renombraArchivo(path, ejecucionCorrecta, sb, log, ip);

                sb.Clear();

                return true;


            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("AltaTarjetasAnonimasProcesaArch: {0} --- Stack : {1} ", err.Message, err.StackTrace) + "]");
                renombraArchivoPorError(path, lstArchvio, err.Message, log, ip);
                return false;
            }
        }

        private static void renombraArchivoPorError(string path, List<Archivo> lstArchivo,
            string message, string log, string ip)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                "Emisor".PadRight(9),
                "Cuenta".PadRight(16),
                "Número Tarjeta".PadRight(29),
                "Tarjeta".PadRight(24),
                "Vencimiento".PadRight(15),
                "Clabe".PadRight(18),
                "Código Resultado".PadRight(20),
                "Mensaje Resultado"));

            sb.AppendLine("------------------------------------------------------------------------------------------------------------------------------------");
            foreach (var item in lstArchivo)
            {
                sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                    item.Emisor.PadRight(9),
                    item.Cuenta.PadRight(16),
                    item.NumeroTarjeta.PadRight(29),
                    item.Tarjeta.PadRight(24),
                    item.Vencimiento.PadRight(15),
                    "99".PadRight(18),
                    "99".PadRight(20),
                    message));
            }

            renombraArchivo(path, false, sb, log, ip);
        }

        internal void OcurrioError(object sender, ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }


        public void crearDirectorio()
        {//
            directorioSalida = PNConfig.Get("CARGABOVEDADIGITAL", "DirectorioSalida");
            if (!Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);
            if (!Directory.Exists(directorio + "\\Procesados"))
                Directory.CreateDirectory(directorio + "\\Procesados");
            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);
            if (!Directory.Exists(directorioSalida + "\\Erroneos"))
                Directory.CreateDirectory(directorioSalida + "\\Erroneos");
            if (!Directory.Exists(directorioSalida + "\\Correctos"))
                Directory.CreateDirectory(directorioSalida + "\\Correctos");
        }


        internal void NuevoArchivo(object sender, FileSystemEventArgs e)
        {

            WatcherChangeTypes elTipoCambio = e.ChangeType;
            //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
            LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Hubo un Cambio " + elTipoCambio.ToString() + " en el directorio: " + PNConfig.Get("PRALTATARJPARABILIA", "DirectorioEntrada") + " el se recibio el archivo : " + e.FullPath + "]");
            LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Espera a que se copie el archivo completo y lo libere el proceso de copiado]");
            LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [INICIO DE PROCESO DEL ARCHIVO:" + e.FullPath + "]");
            validarArchivos(true);
        }

        private static void renombraArchivo(string elpathFile, bool ejecucionCorrecta, StringBuilder sb, string log, string ip)
        {
            if (ejecucionCorrecta)
            {
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Iniciar el renombramiento del Archivo procesado: " + elpathFile + "]");
                string filename = Path.GetFileName(elpathFile);
                //string root = Path.GetDirectoryName(elpathFile);
                string finalFileName = directorioSalida + "\\Correctos\\" + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "");



                File.Delete(elpathFile);
                if (File.Exists(finalFileName))
                {
                    var tempName = Path.GetFileName(finalFileName);
                    //#if DEBUG
                    //                    var directorio = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioArchivosParabilia");
                    //#else

                    var directorio = PNConfig.Get("CARGABOVEDADIGITAL", "DirectorioEntrada");
                    //#endif
                    DirectoryInfo directory = new DirectoryInfo(directorio);
                    var consecutivo = directory.GetFiles().Where(w => w.Name.Contains(tempName.Replace("PROCESADO_", ""))).Count() + 1;
                    finalFileName = finalFileName.Replace("PROCESADO_", string.Format("PROCESADO_{0}_{1}_", consecutivo, DateTime.Now.Hour));
                }

                using (StreamWriter sw = File.CreateText(finalFileName))
                {
                    sw.WriteLine(sb.ToString());
                }
            }
            else
            {
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Iniciar el renombramiento del Archivo procesado con error: " + elpathFile + "]");
                string filename = Path.GetFileName(elpathFile);
                //string root = Path.GetDirectoryName(elpathFile);
                string finalFileName = directorioSalida + "\\Erroneos\\" + "\\PROCESADO_CON_ERROR_" + filename.Replace("EN_PROCESO_", "");
                File.Delete(elpathFile);

                if (File.Exists(finalFileName))
                {
                    var tempName = Path.GetFileName(finalFileName);

                    //#if DEBUG
                    //                    var directorio = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioArchivosParabilia");
                    //#else

                    var directorio = PNConfig.Get("CARGABOVEDADIGITAL", "DirectorioEntrada");
                    //#endif
                    DirectoryInfo directory = new DirectoryInfo(directorio);
                    var consecutivo = directory.GetFiles().Where(w => w.FullName.Contains(tempName.Replace("PROCESADO_CON_ERROR_", ""))).Count() + 1;
                    finalFileName = finalFileName.Replace("PROCESADO_CON_ERROR_", string.Format("PROCESADO_CON_ERROR_{0}_{1}_", consecutivo, DateTime.Now.Hour));
                }


                using (StreamWriter sw = File.CreateText(finalFileName))
                {
                    sw.WriteLine(sb.ToString());
                }
            }

        }


        internal bool validarArchivos(bool validaCopiadoFiles)
        {
            LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Validando archivo]");

            try
            {
                responseAzure respuesta = new responseAzure();
                string appAKV = ConfigurationManager.AppSettings["applicationId"].ToString();
                string clave = ConfigurationManager.AppSettings["clientKey"].ToString();
                bool activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);
                //if (true)
                //{
                //     respuesta = KeyVaultProvider.RegistrarProvedorCEK(appAKV, clave);
                //}
                if (validaCopiadoFiles)
                {
                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Iniciar espera de copiado de Archivo PRALTATARJPARABILIA]");
                    Thread.Sleep(1000 * 60 * 1);
                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Termina espera de copiado de Archivo PRALTATARJPARABILIA]");
                }

                int resp;

                resp = 0;

                try
                {
                    //ConfiguracionContexto.InicializarContexto();

                    List<ArchivoConfiguracion> losArchivos = DAOArchivo.ObtenerArchivosConfigurados(log, ip);

                    ArchivoConfiguracion elArchivos = losArchivos.
                        Where(m => m.ClaveArchivo.Contains("PRALTATARJPARABILIA")).FirstOrDefault();

                    if (elArchivos == null)
                        throw new Exception("Configuración del archivo no localizada");

                    //#if DEBUG
                    //                    var directorio = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioArchivosParabilia");
                    //#else

                    var directorio = PNConfig.Get("CARGABOVEDADIGITAL", "DirectorioEntrada");
                    //#endif
                    DirectoryInfo directory = new DirectoryInfo(directorio);
                    FileInfo[] files = directory.GetFiles(elArchivos.Nombre + "*.*");
                    if (files.Count() == 0) {
                        files = directory.GetFiles("PROCESADO_"+elArchivos.Nombre + "*.*");
                    }
                  

                    if (files.Length == 0)
                    {
                        LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [NO HAY ARCHIVO QUE PROCESAR: PRALTATARJPARABILIA +" + elArchivos.Nombre + "*.*]");
                    }

                    for (int i = 0; i < files.Length; i++)
                    {

                        String elpathFile = (((FileInfo)files[i]).FullName);

                        //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                        Logueo.Evento("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Inicia el renombramiento del Archivo procesado: " + elpathFile + "]");
                     //   LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Inicia el renombramiento del Archivo procesado: " + elpathFile + "]");
                        string filename1 = Path.GetFileName(elpathFile);
                        string root1 = Path.GetDirectoryName(elpathFile);
                        filename1 = filename1.Replace("PROCESADO_", "");
                        root1 = root1.Replace("PROCESADO_", "");
                        string[] componentesTitulo = filename1.Split('_');

                        String claveColectiva = filename1.Substring(filename1.IndexOf('_') + 1,
                            filename1.LastIndexOf('_') - (filename1.IndexOf('_') + 1)).Trim();
                        string subproducto = "";
                        //diferenciar entre debito y credito
                        // if (filename1.Contains("_V_")) {
                        string nuevoNombre = filename1.Replace("_V_", "_");
                        claveColectiva = nuevoNombre.Substring(nuevoNombre.IndexOf('_') + 1,
                        nuevoNombre.LastIndexOf('_') - (nuevoNombre.IndexOf('_') + 1)).Trim();
                        if (claveColectiva.Length > 9)
                        {
                            nuevoNombre = nuevoNombre.Replace("_" + componentesTitulo[componentesTitulo.Length - 1], "");
                            claveColectiva = nuevoNombre.Substring(nuevoNombre.IndexOf('_') + 1,
                            nuevoNombre.LastIndexOf('_') - (nuevoNombre.IndexOf('_') + 1)).Trim();

                            subproducto = nuevoNombre.Split('_').Last();
                            //string[] elementos = nuevoNombre.Split('_');
                            //if (elementos.Length > 3)
                            //{
                            //    subproducto = "";
                            //    for (int j = 2; j <= elementos.Length - 1; j++)
                            //    {
                            //        subproducto = subproducto + elementos[j];
                            //        if (j != elementos.Length - 1)
                            //        {
                            //            subproducto = subproducto + "_";
                            //        }
                            //    }
                            //}
                        }
                        //}
                        string tipoManufactura = null;
                        //camabiando obtencion de clave por medio del spli
                        //claveColectiva = componentesTitulo[1] + "_" + componentesTitulo[2];

                        if (filename1.ToUpper().Contains("_V_"))//claveColectiva.ToUpper().Contains("_V_"))
                        {
                            tipoManufactura = "V";
                            //       claveColectiva = claveColectiva.ToUpper().Replace("_V", "");
                            //       subproducto = componentesTitulo[3].ToString();//.Split('.')[0];
                        }


                        //if (componentesTitulo.Length >= 4)
                        //{

                        //    //si es una fecha entonces no existe subroducto
                        //    try
                        //    {
                        //        DateTime fecha = Convert.ToDateTime(subproducto.Insert(4, "-").Insert(7, "-"));
                        //        subproducto = "";
                        //    }
                        //    catch (Exception ex)
                        //    {

                        //    }
                        //}

                        //Limpiar archivo
                        var tmpNameFile =
                            LNArchivo.LimpiarArchivo(((FileInfo)files[i]).FullName, directory.FullName, log, ip);

                        File.Move(tmpNameFile, elpathFile);

                        File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);

                        elpathFile = root1 + "\\EN_PROCESO_" + filename1;

                        LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Procesar archivo desde hora programada " + ((FileInfo)files[i]).FullName + "]");

                        //obteniendo el subproducto para saber que metodo ejecutar
                        bool esCredito = false;
                        bool subproductoValido = false;
                        if (subproducto != "")
                        {
                            LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Validando subproducto]");

                            //  esCredito = true;
                            string conexionExterna = PNConfig.Get("CARGABOVEDADIGITAL", "BDWriteBovedaDigital");
                            DataTable resultadoConsultaSubproducto = DAOArchivo.validaExistenciaSubproducto(subproducto, conexionExterna, log, ip);
                            if (!(resultadoConsultaSubproducto is null))
                            {
                                if (resultadoConsultaSubproducto.Rows[0]["Respuesta"].ToString() == "correcto")
                                {

                                    subproductoValido = true;
                                    LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Subproducto validaod correctamente]");

                                    //   claveColectiva = claveColectiva.Replace("!", "_");
                                }

                            }
                        }

                        List<Archivo> lstArchvio = new List<Archivo>();
                        Boolean resp2 = false;
                        //if (esCredito)
                        // {
                        Logueo.Evento("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Procesando archivo]");

                       // LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Procesando archivo]");

                        resp2 = LNArchivo.ProcesaArch(elpathFile, claveColectiva, tipoManufactura, ref lstArchvio, subproducto, subproductoValido, esCredito, log, ip);

                        // }
                        //else
                        //{
                        //    resp2 = LNArchivo.ProcesaArch(elpathFile, claveColectiva, tipoManufactura, ref lstArchvio);
                        //}

                    }

                }

                catch (Exception ex)
                {
                    resp = 1;
                    Console.WriteLine(ex.ToString());
                    LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Erorr " + ex.Message + " , " + ex.StackTrace + "]");
                }


                if (resp == 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return false;
            }
        }

        private static bool ejecutaActivacion(ref List<Archivo> lstArchvio, ref StringBuilder sb, String claveColectiva, string tipoManufactura, string subproducto = null, bool subproductoValido = false, bool esCredito = false, string log = "", string ip = "")
        {

            var ejecucionCorrecta = true;
            string tokenTarjeta = "";

            sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",//{8}",
                "Emisor".PadRight(9),
                "Cuenta".PadRight(16),
                "Número Tarjeta".PadRight(29),
                "Tarjeta".PadRight(24),
                "Vencimiento".PadRight(15),
                "Clabe".PadRight(25),
                //"Token".PadRight(50),
                "Código Resultado".PadRight(20),
                "Mensaje Resultado"));

            sb.AppendLine("------------------------------------------------------------------------------------------------------------------------------------");
            string codigoCreacionCorte = "";
            int contador = 0;
            Int64 idLote = 0;
            int totalTarjetas = lstArchvio.Count;
            esCredito = false;//esta validacion ya la hago dentro del store
            LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [iniciando proceso]");

            foreach (var item in lstArchvio)
            {
                codigoCreacionCorte = "";
                string colectivacuentahabiente = "";
                try
                {
                    string[] resultado = new string[2];
                    if (true)//string.IsNullOrEmpty(subproducto))
                    {
                        SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraBovedaDigital);
                        try
                        {
                            conn.Open();
                            //using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                            //{
                            //try
                            //{

                            string[] datosArchivo = item.RawData.Split('|');
                            if (datosArchivo[1] != "00000")
                            {
                                LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Dando de alta tarjeta]");

                                Random rand = new Random();
                                int numero = rand.Next(25);
                                char letra = (char)(((int)'A') + numero);
                                char letraReserva = 'R';
                                var random = new Random();
                                string idSolicitud = random.Next(1, 99999999).ToString();
                                idSolicitud = letraReserva + letra + idSolicitud.PadLeft(8, '0');
                                DataTable resultadoConsulta = DAOArchivo.AltaTarjeta(item, claveColectiva, tipoManufactura, "", false, null, conn, null, subproducto, contador, idLote, idSolicitud, totalTarjetas);// DBProcesadorArchivo.strBDEscrituraAutorizador);DBProcesadorArchivo.strBDEscrituraAutorizador

                                if (resultadoConsulta != null && resultadoConsulta.Rows.Count > 0)
                                {
                                    if (resultadoConsulta.Rows[0]["ResponseCode"].ToString() == "0000")
                                    {
                                        resultado[0] = "00";
                                        resultado[1] = "Alta Correcta";
                                        if (contador == 0) {
                                            contador++;
                                            idLote = Convert.ToInt64(resultadoConsulta.Rows[0]["IdLote"]);
                                        }
                                    }
                                    else {
                                        resultado[0] = "99";
                                        resultado[1] = resultadoConsulta.Rows[0]["ResponseDescription"].ToString();
                                    }

                                }
                                else {
                                    resultado[0] = "99";
                                    resultado[1] = "Error al cargar tarjetas";
                                }
//                                if (resultado[0] == "00")
//                                {
//                                    try
//                                    {
//                                        tokenTarjeta = resultado[2];
//                                    }
//                                    catch (Exception ex) { }

//                                    //esta parte es para cacao
//#if !CACAO

//                                    string cuentaCacao = resultado[3];
//                                    string idMaCACAO = resultado[4];
//                                    string tipoMARegreso = resultado[5];
//                                    var tablaAltaCuentaCacao = DAOArchivo.AgregarMedioAcceso(cuentaCacao, idMaCACAO, tipoMARegreso, conn, null, subproducto);// DBProcesadorArchivo.strBDEscrituraAutorizador);DBProcesadorArchivo.strBDEscrituraAutorizador
//                                    if (tablaAltaCuentaCacao.Rows[0]["tipo"].ToString() == "correcto")
//                                    {
//                                        LogueoPrAltaTarjParabilia.Debug("realizando commit");
//                                        //          transaccionSQL.Commit();
//                                    }
//                                    else
//                                    {
//                                        LogueoPrAltaTarjParabilia.Debug("realizando rollback" + tablaAltaCuentaCacao.Rows[0][0].ToString());
//                                        //        transaccionSQL.Rollback();
//                                        resultado[0] = tablaAltaCuentaCacao.Rows[0][2].ToString();
//                                        resultado[1] = tablaAltaCuentaCacao.Rows[0][1].ToString();
//                                        LogueoPrAltaTarjParabilia.Debug("rollback realizado");

//                                    }
//#else
//                                             LogueoPrAltaTarjParabilia.Debug("realizando commit");
//                                             transaccionSQL.Commit();
//#endif


//                                }
//                                else
//                                {
//                                    LogueoPrAltaTarjParabilia.Debug("realizando rollback");
//                                    //               transaccionSQL.Rollback();
//                                }
                                //     }

                            }
                            //}
                            //catch (Exception tran)
                            //{
                            //    LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [transaccion] [" + log + "] [" + tran.Message + "]");
                            //    //         transaccionSQL.Rollback();
                            //}
                            // }
                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO][conexion] [" + log + "] [" + ex.Message + "]");

                         //   LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO][conexion] [" + log + "] [" + ex.Message + "]");
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }//
                   

                    var resObtenerClabe = "";  //DAOArchivo.ObtenerClabeTarjeta(item.NumeroTarjeta, "CLABE", DBProcesadorArchivo.strBDEscrituraAutorizador);

                    if (resultado[0] != "00")
                    {
                        ejecucionCorrecta = false;
                    }

                    //if (resObtenerClabe[0] != "00")
                    //{
                    //    ejecucionCorrecta = false;
                    //}

                    sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",//{8}",
                        item.Emisor.PadRight(9),
                        item.Cuenta.PadRight(16),
                        item.NumeroTarjeta.PadRight(29),
                        item.Tarjeta.PadRight(24),
                        item.Vencimiento.PadRight(15),
                        "".PadRight(25),// resObtenerClabe[1].PadRight(25),
                                        //tokenTarjeta.PadRight(50),
                        resultado[0].PadRight(20),
                        resultado[1]));
                }
                catch (Exception ex)
                {
                    sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",//{8}",
                    item.Emisor.PadRight(9),
                    item.Cuenta.PadRight(16),
                    item.NumeroTarjeta.PadRight(29),
                    item.Tarjeta.PadRight(24),
                    item.Vencimiento.PadRight(15),
                    //tokenTarjeta.PadRight(50),
                    "99".PadRight(25),
                    "99".PadRight(20),
                    ex.Message));
                    Logueo.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ERROR EN ALTA TARJETA {0} - ERROR {1}", item.RawData, ex.Message) + "]");

                    //   LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ERROR EN ALTA TARJETA {0} - ERROR {1}", item.RawData, ex.Message) + "]");
                    ejecucionCorrecta = false;
                }
            }

            return ejecucionCorrecta;
        }

        internal static string LimpiarArchivo(String elArchivo, string path, string log, string ip)
        {

            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(elArchivo, Encoding.UTF8))
            {
                String claveMov = String.Empty;
                String descMov = String.Empty;
                String account = String.Empty;
                String total = String.Empty;
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();

                    if (Regex.Match(line, @"^[0-9]{8}[\s][0-9]{5,15}[\s]").Success) //@"^[0-9]{8}[\s]{17}[0-9]{16}").Success)// @"^[0-9]{8}[\s][0-9]{13,15}[\s]").Success)
                    {
                        // if (Regex.Match(line, @"^[0-9]{8}[\s][0-9]{5,15}[\s]{11}").Success)
                        // {

                        var limiteInicial = 9;
                        var emisor = string.Empty;
                        try
                        {
                            emisor =
                                line.Substring(0, limiteInicial).Trim();

                        }
                        catch (Exception ex)
                        {
                            LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                        }

                        var Cuenta = string.Empty;
                        var segundolimite = limiteInicial + 16;
                        try
                        {
                            Cuenta = line.Substring(limiteInicial, (segundolimite - limiteInicial)).Trim();

                        }
                        catch (Exception ex)
                        {
                            LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                        }

                        var tercerlimite = segundolimite + 29;
                        var NumeroTarjeta = string.Empty;
                        try
                        {
                            NumeroTarjeta = line.Substring(segundolimite, (tercerlimite - segundolimite)).Trim();
                        }
                        catch (Exception ex)
                        {
                            LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                        }

                        var cuartolimite = tercerlimite + 24;
                        var Tarjeta = string.Empty;
                        try
                        {
                            Tarjeta = line.Substring(tercerlimite, (cuartolimite - tercerlimite)).Trim();
                        }
                        catch (Exception ex)
                        {
                            LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                        }

                        //var quintoimite = cuartolimite + 10;
                        //LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [FECHA:" + line.Substring(cuartolimite, (quintoimite - cuartolimite)).Trim() + "]");

                        //var vencimientoTmp = string.Empty;
                        //var Vencimiento = string.Empty;
                        //try
                        //{
                        //    vencimientoTmp = line.Substring(cuartolimite).Trim();


                        //    Vencimiento = String.Format("{0}-{1}-{2}", vencimientoTmp.Substring(6, 4),
                        //        vencimientoTmp.Substring(3, 2),
                        //        vencimientoTmp.Substring(0, 2));
                        //}
                        //catch (Exception ex)
                        //{
                        //    LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                        //}
                        var quintoimite = cuartolimite + 10;
                        Logueo.Evento("FECHA:" + line.Substring(cuartolimite, (quintoimite - cuartolimite)).Trim());

                        var vencimientoTmp = string.Empty;
                        var Vencimiento = string.Empty;
                        try
                        {
                            Vencimiento = line.Substring(cuartolimite, (quintoimite - cuartolimite)).Trim();


                            //Vencimiento = String.Format("{0}-{1}-{2}", vencimientoTmp.Substring(6, 4),
                            //    vencimientoTmp.Substring(3, 2),
                            //    vencimientoTmp.Substring(0, 2));
                        }
                        catch (Exception ex)
                        {

                        }


                        string nTarjeta = NumeroTarjeta.Substring(0, 6) + "******" + NumeroTarjeta.Substring(12, 4);
                        string tar = "";// Tarjeta.Substring(0, 6) + "******" + Tarjeta.Substring(12, 4);
                        LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Registro: " + String.Format("{0}|{1}|{2}|{3}|{4}", emisor, Cuenta, nTarjeta, tar, Vencimiento) + "]");

                        sb.AppendLine(String.Format("{0}|{1}|{2}|{3}|{4}", emisor, Cuenta, NumeroTarjeta, Tarjeta,
                             Vencimiento));
                    }
                }
            }
            File.Delete(elArchivo);
            using (StreamWriter sw = File.CreateText(elArchivo))
            {
                sw.WriteLine(sb.ToString());

            }

            return elArchivo;
        }

        public static void ObtieneRegistrosDeArchivo(String elPath, ref List<Archivo> elArchivo, string log, string ip)
        {

            string sLine = "";
            elArchivo.Clear();
            try
            {
                //
                using (StreamReader objReader = new StreamReader(elPath))
                {
                    while (objReader.Peek() > -1)
                    {
                        sLine = objReader.ReadLine();

                        if (string.IsNullOrEmpty(sLine))
                        {
                            break;
                        }

                        var splitted = sLine.Split('|');
                        //LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Procesando archivo "+ sLine + "]");

                        elArchivo.Add(new Archivo
                        {
                            Emisor = splitted[0],
                            Cuenta = splitted[1],//String.Format("{0}{1}", splitted[0], splitted[1]),
                            NumeroTarjeta = splitted[2],
                            Tarjeta = splitted[3],
                            Vencimiento = splitted[4],
                            RawData = sLine
                        });
                    }
                }
            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source + "]");
            }
        }



    }
}
