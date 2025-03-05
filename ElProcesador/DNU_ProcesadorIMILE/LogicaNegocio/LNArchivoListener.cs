using CommonProcesador;
using DNU_ProcesadorIMILE.BaseDatos;
using DNU_ProcesadorIMILE.Constants;
using DNU_ProcesadorIMILE.Entidades.Request;
using DNU_ProcesadorIMILE.Entidades.Response;
using Newtonsoft.Json;
using RestSharp;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ProcesadorIMILE.LogicaNegocio
{
    class LNArchivoListener
    {

        public static Boolean enProceso = false;
        private static readonly object balanceLock = new object();

        private static readonly string archivoEnProceso = "_PROCESANDO.";
        private static readonly string archivoConError = "_PROCESADO_ERROR.";
        private static readonly string archivoProcesado = "_PROCESADO_OK.";

        private static string token;

        private static FileSystemWatcher watcher = new FileSystemWatcher(Path.Combine("@", PNConfig.Get("DNU_ProcesadorIMILE", "DirectorioEntrada")));

        public static void EscucharDirectorios()
        {
            try
            {
                token = LNTokenWsAppConnect.tokenWsAppConnect;

                Logueo.Evento("Inicia la escucha de la carpeta: " + PNConfig.Get("DNU_ProcesadorIMILE", "DirectorioEntrada"));                

                watcher.NotifyFilter = NotifyFilters.DirectoryName
                                     | NotifyFilters.FileName
                                     | NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite;

                watcher.Created += alCambiar;
                watcher.Error += alOcurrirUnError;

                watcher.EnableRaisingEvents = true;

                //Console.ReadLine();
            }
            catch (Exception err)
            {
                Logueo.Error("EscucharDirectorios(): " + err.Message);
            }
        }

        private static void alCambiar(object source, FileSystemEventArgs el)
        {
            try
            {
                enProceso = true;

                //watcher.EnableRaisingEvents = false;

                Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                try
                {
                    Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                    Logueo.Evento("Esperando " + (Int32.Parse(PNConfig.Get("DNU_ProcesadorIMILE", "MinEsperaProceso"))).ToString() + " Minutos.");
                    Thread.Sleep(1 * 60 * Int32.Parse(PNConfig.Get("DNU_ProcesadorIMILE", "MinEsperaProceso")));
                }
                catch (Exception)
                {
                    Logueo.Evento("Inicia espera defualt por no tener configuracion en 'MinEsperaProceso'. Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                    Thread.Sleep(100 * 60);
                    Logueo.Evento("Esperando 10 Minutos.");
                }

                ResponseProceso respuestaProceso = new ResponseProceso();                

                try
                {
                    lock (balanceLock)
                    {                       
                        FileInfo[] files;

                        DirectoryInfo directory = new DirectoryInfo(Path.Combine("@", PNConfig.Get("DNU_ProcesadorIMILE", "DirectorioEntrada")));

                        files = directory.GetFiles("*.*");

                        for (int i = 0; i < files.Length; i++)
                        {
                            String elpathFile;

                            elpathFile = (((FileInfo)files[i]).FullName);

                            Logueo.Evento("Comienza Procesando Archivo: " + elpathFile);

                            ProcesarDatosArchivo(elpathFile);

                            Logueo.Evento("Termina Procesando Archivo: " + elpathFile);
                        }                        
                    }
                }
                catch (Exception err)
                {
                    Logueo.Error("alCmbiar(): " + err.Message + ", " + err.ToString());
                    Console.WriteLine(err.ToString());
                }
                finally
                {
                    enProceso = false;
                }
            }
            catch (Exception err)
            {
                Logueo.Error("alCambiar(): " + err.Message);
            }
            finally
            {
                EscucharDirectorios();
            }
        }

        private static bool ProcesarDatosArchivo(string path)
        {
            try
            {                
                RequestConsultarTarjetaOnb requestConsultarTarjetaOnb = new RequestConsultarTarjetaOnb();
                ResponseConsultarTarjetaOnb respuestaConsultaTarjetaOnb = new ResponseConsultarTarjetaOnb();

                Logueo.Evento("Comienza lectura Excel.");
                SLDocument sl = new SLDocument(path);
                Logueo.Evento("Termina lectura Excel.");

                string nombreArchivo = Path.GetFileName(path);
                string url = PNConfig.Get("DNU_ProcesadorIMILE", "DirectorioEntrada");
                string urlError = PNConfig.Get("DNU_ProcesadorIMILE", "DirectorioSalidaError");
                string urlProcesado = PNConfig.Get("DNU_ProcesadorIMILE", "DirectorioSalidaProcesados");

                if (!Path.GetExtension(path).ToString().ToLower().Equals(".xlsx"))
                {
                    Logueo.Evento("La extension del Archivo no es correcta.");

                    throw new Exception("ProcesarDatosArchivo(): EXTENSION DE ARCHIVO INCORRECTA.");
                }

                Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + nombreArchivo + archivoEnProceso);

                string[] narchivo = nombreArchivo.Split('.');
                nombreArchivo = "\\" + narchivo[0] + archivoEnProceso + narchivo[1];

                url = url + nombreArchivo;

                File.Move(path, url);

                Logueo.Evento("Termina el renombramiento del Archivo procesado: " + nombreArchivo + archivoEnProceso);

                int iRow = 2;

                Logueo.Evento("Comienza lectura del archivo y armado de tabla.");

                //string cadenaConexion = "data source=45.32.4.114;Initial Catalog=AppConnect_UNO;User Id=ealbarran;Password=maLVGu57WaU8";

                using (SqlConnection conn = new SqlConnection(DBProcesadorIMILE.strBDEscritura))
                //using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    conn.Open();

                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {
                            Boolean laRespu = false;

                            while (!String.IsNullOrEmpty(sl.GetCellValueAsString(iRow, 1)))
                            {
                                string consecutivo = sl.GetCellValueAsString(iRow, 1);
                                string cuenta = sl.GetCellValueAsString(iRow, 2);
                                string[] codigoQR = sl.GetCellValueAsString(iRow, 3).Split('/');
                                string noGuia = sl.GetCellValueAsString(iRow, 4);

                                string folio = codigoQR[2];

                                requestConsultarTarjetaOnb.Folio = folio;

                                respuestaConsultaTarjetaOnb = WsConsultarTarjetaOnb(requestConsultarTarjetaOnb);

                                if (respuestaConsultaTarjetaOnb.CodRespuesta.Equals("0072"))
                                {
                                    TokenWsAppConnnect();

                                    token = LNTokenWsAppConnect.tokenWsAppConnect;

                                    respuestaConsultaTarjetaOnb = WsConsultarTarjetaOnb(requestConsultarTarjetaOnb);
                                }

                                string userID = respuestaConsultaTarjetaOnb.IDUsuario;

                                laRespu = DAOTrackOrder.InsertTrackOrder(consecutivo, cuenta, folio, noGuia, userID, conn, transaccionSQL);

                                if (!laRespu)
                                {
                                    transaccionSQL.Rollback();
                                    Logueo.Evento("ERROR AL GUARDAR EN BD.");
                                    Logueo.Error("ProcesarDatosArchivo(): NO SE PUDO INSERTAR LOS REGISTROS EN LA BD.");

                                    Logueo.Evento("Iniciar el renombramiento y cambio directorio del Archivo procesado: " + nombreArchivo + archivoConError);

                                    nombreArchivo = "\\" + narchivo[0] + archivoConError + narchivo[1];

                                    urlError = urlError + nombreArchivo;

                                    File.Move(path + archivoEnProceso, urlError);

                                    Logueo.Evento("Termina el renombramiento y cambio directorio del Archivo procesado: " + nombreArchivo + archivoConError);

                                    throw new Exception("ProcesarDatosArchivo(): NO SE PUDIERON INSERTAR LOS REGISTROS EN LA BD.");
                                }
                                else
                                {                            
                                    iRow++;
                                }
                            }

                            transaccionSQL.Commit();

                            Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + nombreArchivo + archivoProcesado);

                            nombreArchivo = "\\" + narchivo[0] + archivoProcesado + narchivo[1];

                            urlProcesado = urlProcesado + nombreArchivo;

                            File.Move(path, url + urlProcesado);

                            Logueo.Evento("Termina el renombramiento del Archivo procesado: " + nombreArchivo + archivoProcesado);
                        }
                        catch (Exception err)
                        {
                            transaccionSQL.Rollback();
                            Logueo.Error("ProcesarDatosArchivo(): " + err.Message);

                            nombreArchivo = "\\" + narchivo[0] + archivoConError + narchivo[1];

                            urlError = urlError + nombreArchivo;

                            File.Move(path + archivoEnProceso, urlError);

                            Console.WriteLine("Procesado con ERROR: " + path + archivoConError);
                        }

                        transaccionSQL.Dispose();
                    }

                    conn.Close();
                }

                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("ProcesarDatosArchivo(): " + err.Message);

                return false;
            }
        }

        public static void alOcurrirUnError(object source, ErrorEventArgs e)
        {
            try
            {
                Logueo.Error("alOcurrirUnError(): " + e.GetException().Message);
            }
            catch (Exception err)
            {
                Logueo.Error("alOcurrirUnError(): " + err.Message);
            }
        }

        public static void TokenWsAppConnnect()
        {
            try
            {
                LNTokenWsAppConnect lnTokenWsAppConnect = new LNTokenWsAppConnect();
                RequestLogIn requestLogIn = new RequestLogIn();

                Logueo.Evento("Comienza Generacion de Token para wsAppConnect.");

                Logueo.Evento("Credenciales: " + PNConfig.Get("DNU_ProcesadorIMILE", "CredencialesWsAppConnect"));

                string credenciales = Encoding.UTF8.GetString(Convert.FromBase64String(PNConfig.Get("DNU_ProcesadorIMILE", "CredencialesWsAppConnect")));
                
                requestLogIn.NombreUsuario = credenciales.Split(':')[0];
                requestLogIn.Password = credenciales.Split(':')[1];

                lnTokenWsAppConnect.WsAppConnectLogIn(requestLogIn);

                Logueo.Evento("Termina Generacion de Token para wsAppConnect.");
            }
            catch (Exception ex)
            {
                Logueo.Error("TokenWsAppConnnect(): " + ex.Message);
            }
        }

        private static ResponseConsultarTarjetaOnb WsConsultarTarjetaOnb(RequestConsultarTarjetaOnb _requestConsultaTarjetaOnb)
        {
            ResponseConsultarTarjetaOnb respuesta = new ResponseConsultarTarjetaOnb();

            Logueo.Evento("Comienza envio peticion a ConsultarTarjetaOnb.");

            try
            {
                string encoded = PNConfig.Get("DNU_ProcesadorIMILE", "CredencialesWsAppConnect");
                //string uri = "http://45.32.4.114/wsAppConnect_ClubPremium/api/ConsultarTarejtaOnb/";
                var cliente = new RestClient(PNConfig.Get("DNU_ProcesadorIMILE", "WsConsultarTarjetaOnbAppConnect"));
                //var cliente = new RestClient(uri);
                var request = new RestRequest(Method.POST);

                request.AddHeader("Authorization", token);
                request.AddJsonBody(JsonConvert.SerializeObject(_requestConsultaTarjetaOnb));

                IRestResponse response = cliente.Execute(request);

                respuesta = JsonConvert.DeserializeObject<ResponseConsultarTarjetaOnb>(response.Content);

                Logueo.Evento("Termina envio peticion a WsConsultarTarjetaOnb.");

                return respuesta;
            }
            catch (Exception ex)
            {
                Logueo.Error("WsConsultarTarjetaOnb(): " + ex.Message);

                return null;
            }
        }
    }
}
