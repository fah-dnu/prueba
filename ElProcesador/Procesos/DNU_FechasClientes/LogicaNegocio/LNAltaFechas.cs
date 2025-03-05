using CommonProcesador;
using DNU.Cifrado.DES;
using DNU_FechasClientes.BaseDatos;
using DNU_FechasClientes.Utilidades;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;

namespace DNU_FechasClientes.LogicaNegocio
{
    public class LNAltaFechas
    {
        public string directorio;
        private static string directorioSalida;
        private static string nomArchivoProcesar;
        private static DataTable dtContenidoFileSalida;
        private Cifrador cifrador;
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "FECHASCLIENTES";
                _NombreNewRelic = PNConfig.Get(ClaveProceso, "NombreNewRelic");
                if (String.IsNullOrEmpty(_NombreNewRelic))
                {
                    _NombreNewRelic = ClaveProceso + "-SINNOMBRE";
                    Logueo.Evento("Se coloco nombre generico para instrumentacion NewRelic al no encontrar el parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                    Logueo.Error("No se encontro parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                }
                else
                {
                    Logueo.Evento("Se encontro parametro NombreNewRelic: " + _NombreNewRelic + " [" + ClaveProceso + "]");
                }
                return _NombreNewRelic;
            }
        }

        public LNAltaFechas(string directorio)
        {

            cifrador = new Cifrador(PNConfig.Get("FECHASCLIENTES", "Key"), PNConfig.Get("FECHASCLIENTES", "Vector"));
            this.directorio = cifrador.Descifrar(directorio);

        }


        public void NuevoArchivo(Object sender, FileSystemEventArgs e)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            WatcherChangeTypes elTipoCambio = e.ChangeType;
            //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
            LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Hubo un Cambio " + elTipoCambio.ToString() + " en el directorio: " + PNConfig.Get("ALTAEMPLEADOCACAO", "DirectorioEntrada") + " el se recibio el archivo : " + e.FullPath + "]");
            LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES] Espera a que se copie el archivo completo y lo libere el proceso de copiado]");
            LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES] INICIO DE PROCESO DEL ARCHIVO:" + e.FullPath + "]");
           validarArchivos(true);
        }

        [Transaction]
        public void OcurrioError(Object sender, ErrorEventArgs e)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, OcurrioError, El evento de fileWatcher no inicio correctamente, Mensaje: " + e.GetException().Message + " TRACE: " + e.GetException().StackTrace + "]");
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("OcurrioError");
            ApmNoticeWrapper.NoticeException(e.GetException());
        }

        [Transaction]
        public bool validarArchivos(bool validaCopiadoFiles)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("validarArchivos");
            if (validaCopiadoFiles)
            {
                Thread.Sleep(1000 * 60 * 1);
            }
            FileInfo archivo = null;
            List<string> existeArchivo = Directory.GetFiles(directorio, "*.txt").ToList();
            if (existeArchivo.Count > 0)
            {
                try
                {
                    foreach (var dato in existeArchivo)
                    {
                        archivo = new FileInfo(dato);
                        nomArchivoProcesar = archivo.FullName;
                        string pathInicial = directorio + "\\EN_PROCESO_" + archivo.Name;
                        moverArchivo(archivo.FullName, pathInicial);
                        decodificarArchivo(pathInicial, Path.GetFileNameWithoutExtension(archivo.FullName));
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, validarArchivos, error al obtener archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    moverArchivo(archivo.FullName, pathArchivosInvalidos);
                    LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneo]");
                    ApmNoticeWrapper.NoticeException(ex);
                    return false;
                }
            }
            return false;
        }


        private void decodificarArchivo(string pPath, string pNomArchivo)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            FileInfo InfoArchivo = new FileInfo(pPath);
            int tipoFile = 0;
            try
            {
                if (pNomArchivo.ToLower().Contains(PNConfig.Get("FECHASCLIENTES", "IDentificadorFileVD2").ToLower()))
                    tipoFile = 1;
                else if (pNomArchivo.ToLower().Contains(PNConfig.Get("FECHASCLIENTES", "IDentificadorFileVD3").ToLower()))
                    tipoFile = 2;
                else if (pNomArchivo.ToLower().Contains(PNConfig.Get("FECHASCLIENTES", "IDentificadorFileDig").ToLower()))
                    tipoFile = 3;
                LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES Inicia Validacion del Archivo: " + pPath + "]");
                
                if (tipoFile == 1 || tipoFile == 2)
                {
                    if (procesarContenido(InfoArchivo, tipoFile, pNomArchivo))
                    {
                        crearArchivoSalida(pNomArchivo);
                        string rutaFinal = directorio + "\\Procesados\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                        moverArchivo(InfoArchivo.FullName, rutaFinal);
                        LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Procesados]");
                     }
                    else
                    {
                        crearArchivoSalida(pNomArchivo);
                        string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                        moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                        LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos]");
                    }
                }
                else
                {
                    if (tipoFile == 3)
                    {
                        string emisor = pNomArchivo.Substring(pNomArchivo.IndexOf('_') + 1,
                            pNomArchivo.LastIndexOf('_') - (pNomArchivo.IndexOf('_') + 1)).Trim();
                        if (!string.IsNullOrEmpty(emisor))
                        {
                            if (procesarContenidoDigital(InfoArchivo, tipoFile, pNomArchivo, emisor))
                            {
                                crearArchivoSalida(pNomArchivo);
                                string rutaFinal = directorio + "\\Procesados\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                                moverArchivo(InfoArchivo.FullName, rutaFinal);
                                LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Procesados]");
                            }
                            else
                            {
                                crearArchivoSalida(pNomArchivo);
                                string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                                moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                                LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneo]");
                            }
                        }
                        else
                        {
                            LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, decodificarArchivo, El archivo no trae Emisor:" + pNomArchivo + "]");
                            crearArchivoSalida(pNomArchivo);
                            string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                            moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                            LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos]");
                        }
                    }
                    else
                    {
                        string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                        moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                        LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos]");
                   }
                }
            }
            catch (Exception ex)
            {
                string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, decodificarArchivo, El proceso de validación del archivo:" + pNomArchivo + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [ECHASCLIENTES] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos]");
             }
        }


        private bool procesarContenido(FileInfo pInfoArchivo, int tipoFile, string pNomFile)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            int countLine = 1;
            string line, idFile, fechaExp = string.Empty, idValor;
            Hashtable ht;
            string connAut = cifrador.Descifrar(PNConfig.Get("FECHASCLIENTES", "BDDatos"));
            string connFechas = cifrador.Descifrar(PNConfig.Get("FECHASCLIENTES", "BDFechasClientes"));

            bool resultDetalle = false, resultVal = true;
            dtContenidoFileSalida = crearDataTableSalida();

            StreamReader file = new StreamReader(pInfoArchivo.FullName);
            ht = new Hashtable();
            ht.Add("@nombre", pNomFile);

            DataTable dtInsertFile = DAOAltaFechas.EjecutarSP("procnoc_travel_InsertarDatosFile", ht, connFechas).Tables["Table"];

            idFile = dtInsertFile.Rows[0]["ID_File"].ToString();

            if (idFile == null)
                return false;
            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    string valorD1;
                    string valorD2 = null, valorD3 = null;
                    if (tipoFile == 1)
                    {
                        valorD1 = line.Substring(7, 19).Replace(" ", "");
                        fechaExp = line.Substring(27, 5);
                        valorD2 = line.Substring(55, 3);
                    }
                    else
                    {
                        valorD1 = line.Substring(10, 16).Replace(" ", "");
                        valorD3 = line.Substring(26, 4);
                    }

                    resultDetalle = validarDetalle(line, countLine, valorD1, fechaExp
                                                    , valorD2, valorD3);

                    if (resultDetalle)
                    {
                        ht = new Hashtable();
                        ht.Add("@valorD1", valorD1);

                        //DataTable dtValidaExitencia = DAOAltaFechas.EjecutarSP("procnoc_travel_ValidarExitenciaValorD1", ht, connAut).Tables["Table"];
                        //if (dtValidaExitencia.Rows.Count > 0)
                        //{
                        if (tipoFile == 1)
                        {
                            string[] fechaTemp = fechaExp.Split('/');
                            fechaExp = "20" + fechaTemp[1] + "-" + fechaTemp[0] + "-01";
                        }
                        //idValor = cifrador.Cifrar(dtValidaExitencia.Rows[0]["idValor"].ToString());
                        idValor = null;
                        valorD1 = cifrador.Cifrar(valorD1);
                        valorD2 = string.IsNullOrEmpty(valorD2)
                                   ? null
                                   : cifrador.Cifrar(valorD2);
                        valorD3 = string.IsNullOrEmpty(valorD3)
                                    ? null
                                    : cifrador.Cifrar(valorD3);
                        bool respInsert = DAOAltaFechas.insertaDatos(idFile, fechaExp, idValor, valorD1, valorD2, valorD3
                                            , connFechas);
                        if (respInsert)
                        {
                            dtContenidoFileSalida.Rows.Add(new Object[] { "", "",cifrador.Descifrar(valorD1)
                                                   , cifrador.Descifrar(valorD2), cifrador.Descifrar(valorD3)
                                                   , "Autorizada"});
                        }
                        else
                        {
                            dtContenidoFileSalida.Rows.Add(new Object[] { "", "",valorD1
                                                   , valorD2, valorD3, "Ocurrio error al insertar Valores"});

                            resultVal = false;
                        }
                        //}
                        //else
                        //{
                        //    dtContenidoFileSalida.Rows.Add(new Object[] { "", "",valorD1
                        //                           , valorD2, valorD3, "ValorD1 no existe en paycard"});
                        //    resultVal = false;
                        //}
                    }
                    else
                    {
                        dtContenidoFileSalida.Rows.Add(new Object[] { "", "",valorD1
                                                   , valorD2, valorD3, "Formato incorrecto de datos en la línea"});
                        resultVal = false;
                    }
                }
                catch (Exception ex)
                {
                    LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, procesarContenido " + ex.Message + " " + ex.StackTrace + "]");
                    dtContenidoFileSalida.Rows.Add(new Object[] { "", "",""
                                                   , "", "", "Formato incorrecto de datos en la línea"});
                    resultVal = false;
                }
                countLine++;
            }
            file.Close();


            return resultVal;
        }

        private bool procesarContenidoDigital(FileInfo pInfoArchivo, int tipoFile, string pNomFile
                                                , string pEmisor)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            int countLine = 1;
            string line, idFile, fechaExp = null;
            Hashtable ht;
            string connFechas = cifrador.Descifrar(PNConfig.Get("FECHASCLIENTES", "BDFechasClientes"));

            bool resultDetalle = false, resultVal = true;
            dtContenidoFileSalida = crearDataTableSalida();

            StreamReader file = new StreamReader(pInfoArchivo.FullName);
            ht = new Hashtable();
            ht.Add("@nombre", pNomFile);

            DataTable dtInsertFile = DAOAltaFechas.EjecutarSP("procnoc_travel_InsertarDatosFile", ht, connFechas).Tables["Table"];

            idFile = dtInsertFile.Rows[0]["ID_File"].ToString();

            if (idFile == null)
                return false;
            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    string valorD1 = line.Substring(7, 19).Replace(" ", "");
                    string valorD2 = line.Substring(55, 3);
                    fechaExp = line.Substring(27, 5);

                    resultDetalle = validarDetalle(line, countLine, valorD1, fechaExp
                                                    , valorD2);

                    if (resultDetalle)
                    {
                        valorD1 = cifrador.Cifrar(valorD1);
                        valorD2 = string.IsNullOrEmpty(valorD2)
                                   ? null
                                   : cifrador.Cifrar(valorD2);
                        fechaExp = string.IsNullOrEmpty(fechaExp)
                                        ? null
                                        : cifrador.Cifrar(fechaExp);

                        bool respInsert = DAOAltaFechas.insertaDatosDigitales(idFile, fechaExp, valorD1, valorD2
                                            , pEmisor, connFechas);
                        if (respInsert)
                        {
                            dtContenidoFileSalida.Rows.Add(new Object[] { "", "",cifrador.Descifrar(valorD1)
                                                   , cifrador.Descifrar(valorD2), cifrador.Descifrar(fechaExp)
                                                   , "Autorizada"});
                        }
                        else
                        {
                            dtContenidoFileSalida.Rows.Add(new Object[] { "", "",valorD1
                                                   , valorD2, fechaExp, "Ocurrio error al insertar Valores"});

                            resultVal = false;
                        }
                    }
                    else
                    {
                        dtContenidoFileSalida.Rows.Add(new Object[] { "", "",valorD1
                                                   , valorD2, fechaExp, "Formato incorrecto de datos en la línea"});
                        resultVal = false;
                    }
                }
                catch (Exception ex)
                {
                    LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, procesarContenido " + ex.Message + " " + ex.StackTrace + "]");
                    dtContenidoFileSalida.Rows.Add(new Object[] { "", "",""
                                                   , "", "", "Formato incorrecto de datos en la línea"});
                    resultVal = false;
                }
                countLine++;
            }
            file.Close();


            return resultVal;
        }

        private bool validarDetalle(string pLine, int numLine, string valorD1, string fechaExp
                                        , string valorD2 = null, string valorD3 = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            Regex rgxValorD1 = new Regex(@"\d{16}");
            Regex rgxValorD2 = new Regex(@"\d{3}");
            Regex rgxValorD3 = new Regex(@"\d{4}");
            Regex rgxFecha = new Regex(@"[0-3][0-9]/[0-9][0-9]");


            if (!rgxValorD1.IsMatch(valorD1))
            {
                LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, validarDetalle, hay un error en la línea no. " + numLine + " el valor D1 no cumple el formato]");
                return false;
            }

            if (!string.IsNullOrEmpty(valorD2) && !rgxValorD2.IsMatch(valorD2))
            {
                LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, validarDetalle, hay un error en la línea no. " + numLine + " el valor D2 no cumple el formato]");
                return false;
            }

            if (!string.IsNullOrEmpty(valorD3) && !rgxValorD3.IsMatch(valorD3))
            {
                LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, validarDetalle, hay un error en la línea no. " + numLine + " el valor D3 no cumple el formato]");
               return false;
            }

            if (!string.IsNullOrEmpty(fechaExp) && !rgxFecha.IsMatch(fechaExp))
            {
                LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, validarDetalle, hay un error en la línea no. " + numLine + " la fecha no cumple el formato]");
                return false;
            }

            return true;
        }

        private void crearArchivoSalida(string nomFile)
        {
            string nombreArchivoSalida = directorioSalida + "\\PROCESADO_" + nomFile + ".txt";
            StreamWriter streamSalida = File.CreateText(nombreArchivoSalida);

            foreach (DataRow row in dtContenidoFileSalida.Rows)
            {
                List<string> items = new List<string>();
                int numCol = 1;
                foreach (DataColumn col in dtContenidoFileSalida.Columns)
                {
                    //if (numCol == 1)
                    //    items.Add(row[col].ToString().PadRight(10));
                    //if (numCol == 2)
                    //    items.Add(row[col].ToString().PadRight(10));
                    if (numCol == 3)
                        items.Add(row[col].ToString().PadRight(16));
                    if (numCol == 4)
                        items.Add(row[col].ToString().PadRight(3));
                    if (numCol == 5)
                        items.Add(row[col].ToString().PadRight(4));
                    if (numCol == 6)
                        items.Add(row[col].ToString().PadRight(50));
                    numCol++;
                }
                string linea = string.Join("", items.ToArray());
                streamSalida.WriteLine(linea);
            }

            streamSalida.Close();
        }

        private DataTable crearDataTableSalida()
        {
            DataTable dtDatosnew = new DataTable("DetalleFechasClientesSalida");
            var dc = new DataColumn("ID_Valor", Type.GetType("System.String"));
            var dc1 = new DataColumn("Fecha", Type.GetType("System.String"));
            var dc2 = new DataColumn("ValorD1", Type.GetType("System.String"));
            var dc3 = new DataColumn("ValorD2", Type.GetType("System.String"));
            var dc4 = new DataColumn("ValorD3", Type.GetType("System.String"));
            var dc5 = new DataColumn("Resultado", Type.GetType("System.String"));

            dtDatosnew.Columns.Add(dc);
            dtDatosnew.Columns.Add(dc1);
            dtDatosnew.Columns.Add(dc2);
            dtDatosnew.Columns.Add(dc3);
            dtDatosnew.Columns.Add(dc4);
            dtDatosnew.Columns.Add(dc5);

            return dtDatosnew;
        }

        #region OperacionesFile
        public void crearDirectorio()
        {
            directorioSalida = cifrador.Descifrar(PNConfig.Get("FECHASCLIENTES", "DirectorioSalida"));
            if (!Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);
            if (!Directory.Exists(directorio + "\\Procesados"))
                Directory.CreateDirectory(directorio + "\\Procesados");
            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);
            if (!Directory.Exists(directorio + "\\Erroneos"))
                Directory.CreateDirectory(directorio + "\\Erroneos");
        }

        private void moverArchivo(string pathOrigen, string pathDestino)
        {

            if (!File.Exists(pathDestino))
            {
                File.Move(pathOrigen, pathDestino);
            }
            else
            {
                int numero = 0;
                bool validador = true;
                while (validador)
                {
                    if (numero == 0)
                    {
                        string[] list = pathDestino.Split('\\');
                        string nombreEliminar = list.Last();
                        string[] nom = nombreEliminar.Split('.');
                        string nombre = nom.First() + " - copia." + nom.Last();

                        string nvoPath = pathDestino.Replace(nombreEliminar, nombre);
                        validador = !renombrarArchivo(pathOrigen, nvoPath);
                        numero = numero + 1;
                    }
                    else
                    {
                        numero = numero + 1;
                        string[] list = pathDestino.Split('\\');
                        string nombreEliminar = list.Last();
                        string[] nom = nombreEliminar.Split('.');
                        string nombre = nom.First() + " - copia (" + numero + ")." + nom.Last();

                        string nvoPath = pathDestino.Replace(nombreEliminar, nombre);
                        validador = !renombrarArchivo(pathOrigen, nvoPath);
                    }
                }
            }
        }

        private bool renombrarArchivo(string pathOrigen, string pathDestino)
        {
            if (!File.Exists(pathDestino))
            {
                File.Move(pathOrigen, pathDestino);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }

    enum tipoFile
    {
        typeValorD2 = 0,
        typeValorD3 = 1,
        typeDigital = 2
    }
}
