
using CommonProcesador;
using DNU.Cifrado.DES.HSM;
using DNU_Embozador.BaseDatos;
using DNU_Embozador.Model;
using DNU_Embozador.WS_CLAI_Embozamiento;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.LogicaNegocio
{
    public static class LNEmbozamiento
    {
        private static string tokenWSPB = "";

        public static void Start()
        {
            String completeFileName = string.Empty;
            String fileName = string.Empty;
            String ftpTo = string.Empty;
            List<Dictionary<string, Parametro>> lstParametrosValores = new List<Dictionary<string, Parametro>>();
            List<EMBOZO_Parametros> lstEMBOZO_Parametros;
            string extEncriptado = ".pgp";

            Log.Evento("Iniciando proceso de Embozo  " + Constants.INSTANCIA + " - " + Constants.ObtenerConfiguracion("daysback") + " - " + Constants.ObtenerConfiguracion("clavesEstatus") + " - " + Constants.ObtenerConfiguracion("key"));


            //Obteniendo Origenes de Informacion
            List<EMBOZO_OrigenDatos> lstEMBOZO_OrigenDatos = new List<EMBOZO_OrigenDatos>();
            lstEMBOZO_OrigenDatos.AddRange(DAOAutorizador.ObtenerOrigenDatos());

            if (lstEMBOZO_OrigenDatos.Count == 0)
            {
                Log.Error("No se localizaron Origenes de datos para obtener la información de las Tarjetas a Embozar");
                throw new Exception("No se localizaron Origenes de datos para obtener la información de las Tarjetas a Embozar");
            }


            foreach (var item in lstEMBOZO_OrigenDatos)
            {
                try
                {
                    lstParametrosValores = new List<Dictionary<string, Parametro>>();

                    Dictionary<string, string> paramTs = new Dictionary<string, string>();
                    paramTs.Add("daysback", Constants.ObtenerConfiguracion("daysback"));
                    paramTs.Add("clavesEstatus", Constants.ObtenerConfiguracion("clavesEstatus"));
                    paramTs.Add("instanciaEmbozador", item.ID_EMBOZO_Instancia);
                    paramTs.Add("ID_Colectiva", item.ID_Colectiva);

                    if (item.q_type == "SP")
                        DAOAutorizador.EjecutarOrigenDatosSP(item, paramTs, ref lstParametrosValores);


                    if (lstParametrosValores.Count == 0)
                    {
                        Log.EventoInfo("No se localizó la información necesaria para el embozamiento de las tarjetas ID_Colectiva: " + item.ID_Colectiva);
                        continue;
                    }

                    lstEMBOZO_Parametros = new List<EMBOZO_Parametros>();
                    //Obteniendo definicion del archivo
                    lstEMBOZO_Parametros.AddRange(DAOAutorizador.ObtenerParametros(item.ID_EMBOZO_Instancia, ParameterTypeKeys.fileContent.ToString()));
                    lstEMBOZO_Parametros.AddRange(DAOAutorizador.ObtenerParametros(item.ID_EMBOZO_Instancia, ParameterTypeKeys.fileName.ToString()));


                    lstParametrosValores.FirstOrDefault().Add("CANT_REGISTROS_ARCHIVO", new Parametro
                    {
                        value = lstParametrosValores.Count.ToString(),
                        key = "CANT_REGISTROS_ARCHIVO",
                        type = ""

                    });

                    fileName = ObtenerArchivoAGenerar(lstEMBOZO_Parametros, lstParametrosValores);

                    if (String.IsNullOrEmpty(fileName))
                    {
                        throw new Exception("Ocurrio un error al generar el nombre del archivo");
                    }

                    DAOAutorizador.ActualizaGeneracionArchivo(fileName, item.ID_EMBOZO_Instancia, EstatusGeneracionArchivo.NUEVO);

                    var encoding = Constants.ObtenerEncoding("Encoding");
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (StreamWriter sw = new StreamWriter(ms, encoding))
                        {

                            DAOAutorizador.ActualizaGeneracionArchivo(fileName, item.ID_EMBOZO_Instancia, EstatusGeneracionArchivo.GENERANDO);

                            try
                            {
                                Log.Evento("Registros a procesar en archivo " + fileName + ": " + lstParametrosValores.Count + " con ID_Colectiva " + item.ID_Colectiva);
                                foreach (var lstPV in lstParametrosValores)
                                {
                                    try
                                    {
                                        //Get dates HSM CLAI
                                        if (Constants.VERSION.Equals("1"))
                                            ObtieneOrigenDatosWSRest(lstPV, lstEMBOZO_Parametros);

                                        //Get dates HSM DNU
                                        if (Constants.VERSION.Equals("3") || Constants.VERSION.Equals("4"))
                                            ObtieneOrigenDatosWSRestHSM(lstPV, lstEMBOZO_Parametros, item.GeneraNIP);

                                        //Get delivery address data
                                        if (item.ObtieneDireccionEntregaAuto.Equals("False"))
                                            ObtieneDireccionEntrega(lstPV, lstEMBOZO_Parametros);

                                        try
                                        {

                                            buildContent(lstPV, lstEMBOZO_Parametros, sw, ParameterTypeKeys.fileContent);
                                            sw.Write(Environment.NewLine);
                                            sw.Flush();

                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error(lstPV["ID_MA"] + " - " + ex.Message);
                                            continue;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error(String.Format("Error durante la generacion del archivo {0} ", fileName) + "; ID_MA:" + lstPV["ID_MA"].value + " - " + ex.Message + "] [" + ex.StackTrace + "]");
                                        throw ex;
                                    }
                                }

                                ms.Seek(0, SeekOrigin.Begin);

                            }
                            catch (Exception ex)
                            {
                                DAOAutorizador.ActualizaGeneracionArchivo(fileName, item.ID_EMBOZO_Instancia, EstatusGeneracionArchivo.GENERANDO_ERR);
                                throw ex;
                            }


                            try
                            {

                                var tempPath = "";
                                tempPath = Path.Combine(Environment.CurrentDirectory, "tempFiles");
                                if (!Directory.Exists(tempPath))
                                {
                                    Directory.CreateDirectory(tempPath);
                                }

                                completeFileName = Path.Combine(tempPath, fileName);

                                using (FileStream outputFileStream = new FileStream(completeFileName, FileMode.Create))
                                {
                                    ms.CopyTo(outputFileStream);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(String.Format("Error durante la generacion del archivo {0} ", fileName));
                                DAOAutorizador.ActualizaGeneracionArchivo(fileName, item.ID_EMBOZO_Instancia, EstatusGeneracionArchivo.GENERANDO_ERR);
                                throw ex;
                            }

                            try
                            {
                                extEncriptado = Constants.ObtenerConfiguracion("extArchivoEncrypt");
                            }
                            catch (Exception ex)
                            {
                                extEncriptado = ".pgp";
                            }


                            try
                            {
                                string rutaSalida = Constants.ObtenerConfiguracion("rutaSalida");
                                if (string.IsNullOrEmpty(rutaSalida))
                                {
                                    ftpTo = String.Format(@"sftp://{0}/{1}/", Constants.ObtenerConfiguracion("ftp_host"), Constants.ObtenerConfiguracion("ftp_ruta"));
                                    Log.Evento(String.Format("Enviando archivo {0} a {1}", completeFileName, ftpTo));
                                    LNFtp lnFtp = new LNFtp(Constants.ObtenerConfiguracion("ftp_host"),
                                        Constants.ObtenerConfiguracion("ftp_ruta"),
                                        Constants.ObtenerConfiguracion("ftp_user"),
                                        Constants.ObtenerConfiguracion("ftp_pwd"),
                                        Constants.ObtenerConfiguracion("ftp_port"));

                                    DAOAutorizador.ActualizaGeneracionArchivo(fileName, item.ID_EMBOZO_Instancia, EstatusGeneracionArchivo.ENVIANDO);

                                    encryptFile(completeFileName, completeFileName + extEncriptado);
                                    if (Constants.ObtenerConfiguracion("GenerarArchivo").Equals("1"))
                                        lnFtp.UploadFileSFTP(completeFileName);//para generar el archihvo en claro

                                    if (Constants.VERSION.Equals("1") || Constants.VERSION.Equals("3"))
                                        lnFtp.UploadFileSFTP(completeFileName + extEncriptado);
                                    else if (Constants.VERSION.Equals("4"))
                                        lnFtp.UploadFileSFTPKeyFile(completeFileName + extEncriptado, Constants.ObtenerConfiguracion("sftp_pathKeyFile"));
                                    else
                                        lnFtp.UploadFileSFTPKeyEvent(completeFileName + extEncriptado);


                                }
                                else
                                {
                                    Log.Evento(String.Format("Enviando archivo {0} a {1}", completeFileName, rutaSalida));

                                    DAOAutorizador.ActualizaGeneracionArchivo(fileName, item.ID_EMBOZO_Instancia, EstatusGeneracionArchivo.ENVIANDO);

                                    Directory.Move(completeFileName, rutaSalida + "\\" + fileName);

                                    encryptFile(rutaSalida + "\\" + fileName, rutaSalida + "\\" + fileName + extEncriptado);
                                }
                                DAOAutorizador.ActualizaGeneracionArchivo(fileName, item.ID_EMBOZO_Instancia, EstatusGeneracionArchivo.ENVIADO);
                            }
                            catch (Exception ex)
                            {
                                eliminaArchivoTemp(completeFileName);
                                eliminaArchivoTemp(completeFileName + extEncriptado);
                                DAOAutorizador.ActualizaGeneracionArchivo(fileName, item.ID_EMBOZO_Instancia, EstatusGeneracionArchivo.ENVIANDO_ERR);
                                Log.Error(ex);
                                throw new Exception("Ocurrio un error durante el proceso de envio del archivo por SFTP " + ex.Message);
                            }

                            try
                            {
                                DAOAutorizador.ActualizaProceoMediosAccesoGenerados(lstParametrosValores, fileName, item.ID_EMBOZO_Instancia);

                                DAOAutorizador.ActualizaGeneracionArchivo(fileName, item.ID_EMBOZO_Instancia, EstatusGeneracionArchivo.TERMINADO);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Ocurrio un error durante el proceso de actualizacion de MA's procesados del archivo" + ex.Message);
                            }

                        }
                    }

                    eliminaArchivoTemp(completeFileName);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private static void encryptFile(string inputFile, string outPutFile)
        {
            try
            {
                EncryptionService encrypService = new EncryptionService();

                string pathPublicKey = Constants.ObtenerConfiguracion("pathPublicKey");

                PgpPublicKeyRing pubKeyRing = encrypService.asciiPublicKeyToRing(pathPublicKey);

                PgpPublicKey publicKey = encrypService.getFirstPublicEncryptionKeyFromRing(pubKeyRing);

                EncryptionService.EncryptFile(inputFile, outPutFile, publicKey, false, false);
            }
            catch (Exception ex)
            {
                Log.Error("[Ocurrio error al encriptar File] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }

        private static void eliminaArchivoTemp(string completeFileName)
        {
            try
            {
                if (File.Exists(completeFileName))
                {
                    File.Delete(completeFileName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error al intentar eliminar archivo en temp {0} - ex {1}", completeFileName, ex.Message));
            }
        }

        private static string ObtenerArchivoAGenerar(List<EMBOZO_Parametros> lstEMBOZO_Parametros, List<Dictionary<string, Parametro>> lstParametrosValores)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.ASCII))
                {

                    try
                    {

                        buildContent(lstParametrosValores.FirstOrDefault(), lstEMBOZO_Parametros, sw, ParameterTypeKeys.fileName);
                        sw.Flush();

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                        throw ex;
                    }


                    ms.Seek(0, SeekOrigin.Begin);
                    string val = Encoding.ASCII.GetString(ms.ToArray());
                    return val;

                }
            }

        }

        private static void ObtieneOrigenDatosWSRest(Dictionary<string, Parametro> item, List<EMBOZO_Parametros> lstEMBOZO_Parametros)
        {
            string URLEmbozado = Constants.ObtenerConfiguracion("urlEmbozado");

            var cifrador = new Cifrador(Constants.ObtenerConfiguracion("key"));


            var tarjeta = GetValue(lstEMBOZO_Parametros.Where(w => w.f_key.Equals("N_TARJETA")).FirstOrDefault(), item, DateTime.Now);

            RequestEmbozado reqEmbozo = new RequestEmbozado()
            {
                IDSOLICITUD = "1",
                TARJETA = cifrador.Cifrar(tarjeta),
                FECEXPIRACION = GetValue(lstEMBOZO_Parametros.Where(w => w.f_key.Equals("FEC_VIGENCIA_HASTA")).FirstOrDefault(), item, DateTime.Now),
                FORPINBLOCK = "ISO-0"
            };

            Log.EntradaSalida("[ObtieneOrigenDatosWSRest] [" + URLEmbozado + "] " + JsonConvert.SerializeObject(reqEmbozo), false);

            var client = new RestClient(URLEmbozado);
            var request = new RestRequest(Method.POST);
            Parameter idSol = new Parameter("IDSOLICITUD", "1", ParameterType.QueryString);
            Parameter tar = new Parameter("TARJETA", cifrador.Cifrar(tarjeta), ParameterType.QueryString);
            Parameter exp = new Parameter("FECEXPIRACION", GetValue(lstEMBOZO_Parametros.Where(w => w.f_key.Equals("FEC_VIGENCIA_HASTA")).FirstOrDefault(), item, DateTime.Now), ParameterType.QueryString);
            Parameter pin = new Parameter("FORPINBLOCK", "ISO-0", ParameterType.QueryString);

            request.AddHeader("Content-Type", "application/xml");
            //request.AddJsonBody(reqEmbozo);

            request.AddParameter(idSol);
            request.AddParameter(tar);
            request.AddParameter(exp);
            request.AddParameter(pin);

            IRestResponse response = client.Execute(request);

            ResponseEmbozado respEmbozado = JsonConvert.DeserializeObject<ResponseEmbozado>(response.Content);


            Log.EntradaSalida("[ObtieneOrigenDatosWSRest] [" + URLEmbozado + "] [CodRespuesta : " + respEmbozado.CODRESPUESTA +
                                    ", DesRespuesta : " + respEmbozado.DESCRESPUESTA, true);


            if (!respEmbozado.CODRESPUESTA.Equals(Constants.OK))
            {
                throw new Exception(String.Format("Error al obtener los criptogramas con el caso {0} - {1} - {2}", item["N_CORRELATIVO"].value, respEmbozado.CODRESPUESTA, respEmbozado.DESCRESPUESTA));
            }

            if (String.IsNullOrEmpty(respEmbozado.CVV))
                throw new Exception(String.Format("Error al obtener CVV con el caso {0} ", item["N_CORRELATIVO"].value));

            item.Add("CVC", new Parametro
            {
                key = "CVC",
                value = respEmbozado.CVV
            });

            if (String.IsNullOrEmpty(respEmbozado.CVV2))
                throw new Exception(String.Format("Error al obtener CVC2 con el caso {0} ", item["N_CORRELATIVO"].value));

            item.Add("CVC2", new Parametro
            {
                key = "CVC2",
                value = respEmbozado.CVV2
            });


            if (String.IsNullOrEmpty(respEmbozado.PVV))
                throw new Exception(String.Format("Error al obtener PVV con el caso {0} ", item["N_CORRELATIVO"].value));

            item.Add("ICVV", new Parametro
            {
                key = "ICVV",
                value = respEmbozado.PVV
            });

            if (String.IsNullOrEmpty(respEmbozado.CRIPTOPIN))
                throw new Exception(String.Format("Error al obtener CRIPTOPIN con el caso {0} ", item["N_CORRELATIVO"].value));

            item.Add("NIP_ENCRIPTADO", new Parametro
            {
                key = "NIP_ENCRIPTADO",
                value = respEmbozado.CRIPTOPIN
            });
        }

        private static void buildContent(Dictionary<string, Parametro> maValues, List<EMBOZO_Parametros> lstEMBOZO_Parametros, StreamWriter sw, ParameterTypeKeys ParameterEvaluation)
        {

            StringBuilder sb = new StringBuilder();

            foreach (var item in lstEMBOZO_Parametros.Where(w => w.ID_EMBOZO_ParameterType == Convert.ToInt32(ParameterEvaluation)).
                OrderBy(o => o.initial_index))
            {

                string value = GetValue(item, maValues, DateTime.Now);
                sb.Append(value);

            }

            var res = sb.ToString();

            sw.Write(sb.ToString());



        }


        private static string GetValue(EMBOZO_Parametros item, Dictionary<string, Parametro> maValues, DateTime dateNow)
        {
            switch (item.origin)
            {
                case ORIGIN.PL:
                    if (!maValues.ContainsKey(item.f_key))
                    {
                        throw new Exception("Valor no encontrado para la llave " + item.f_key);
                    }
                    if (Constants.VERSION.Equals("2") && item.f_key.Equals("DATO_ADICIONAL_1"))
                        maValues[item.f_key].value = maValues[item.f_key].value.Substring(12, 4);
                    return maValues[item.f_key].value.Truncate(item.longitud, item.form == "N" ? '0' : ' ');
                case ORIGIN.PLF:
                    if (!maValues.ContainsKey(item.f_key))
                    {
                        throw new Exception("Valor no encontrado para la llave " + item.f_key);
                    }
                    return DateTime.Parse(maValues[item.f_key].value).ToString(item.formato);
                case ORIGIN.CHR:
                    int unicode = Convert.ToInt32(item.formato);
                    char character = (char)unicode;
                    return character.ToString().Truncate(item.longitud, character);
                case ORIGIN.F:
                    return item.formato.Truncate(item.longitud, item.form == "N" ? '0' : ' ');
                case ORIGIN.C:
                    switch (item.f_key)
                    {
                        case "FECHA_ACTUAL":
                            return dateNow.ToString("yyyyMMdd");
                        case "HORA_ACTUAL":
                            return dateNow.ToString("HHmm");
                        case "HORA_ACTUAL_S":
                            return dateNow.ToString("HHmmss");
                        default:
                            return item.formato;
                    }
                case ORIGIN.TS:
                    string val1 = maValues[item.f_key].value.Substring(0, 4);
                    string val2 = maValues[item.f_key].value.Substring(4, 4);
                    string val3 = maValues[item.f_key].value.Substring(8, 4);
                    string val4 = maValues[item.f_key].value.Substring(12, 4);

                    maValues[item.f_key].value = val1 + " " + val2 + " " + val3 + " " + val4;

                    return maValues[item.f_key].value;
                case ORIGIN.PLT:
                    return maValues[item.f_key].value.Truncate(item.longitud, item.form == "N" ? '0' : ' ').Trim();
                default:
                    throw new Exception(String.Format("Valor de la llave {0}  no localizado", item.f_key));
            }
        }

        private static void ObtieneOrigenDatosWSRestHSM(Dictionary<string, Parametro> item, List<EMBOZO_Parametros> lstEMBOZO_Parametros, string generaNIP)
        {
            string tarjeta, serviceCode;
            try
            {
                tarjeta = GetValue(lstEMBOZO_Parametros.Where(w => w.f_key.Equals("N_TARJETA")).FirstOrDefault(), item, DateTime.Now);
            }
            catch (Exception ex)
            {
                tarjeta = item["N_TARJETA_SEC"].value;
            }
            string fechaVigencia = GetValue(lstEMBOZO_Parametros.Where(w => w.f_key.Equals("FEC_VIGENCIA_HASTA")).LastOrDefault(), item, DateTime.Now);
            try
            {
                serviceCode = GetValue(lstEMBOZO_Parametros.Where(w => w.f_key.Equals("SERVICE_CODE")).FirstOrDefault(), item, DateTime.Now);
            }
            catch (Exception ex)
            {
                serviceCode = "201";
            }

            string pinblockBD = null;

            if (Constants.VERSION.Equals("4") && generaNIP.Equals("False"))
            {
                pinblockBD = GetValue(lstEMBOZO_Parametros.Where(w => w.f_key.Equals("PIN_BLOCK")).FirstOrDefault(), item, DateTime.Now);
                if (string.IsNullOrEmpty(pinblockBD))
                    throw new Exception("La tarjeta **********" + tarjeta.Substring(12, 4) + " no tiene un pin");
                item.Remove("PIN_BLOCK");
            }
            ResponseLoginToken respLogin = GetLoginToken();
            ResponsePinBlock respGenerarPin = new ResponsePinBlock();

            if (generaNIP.Equals("True"))
                respGenerarPin = GenerarPinBlock(respLogin, tarjeta);
            else
                respGenerarPin.pinBlock = pinblockBD;

            item.Add("PINBLOCKBD", new Parametro
            {
                key = "PINBLOCKBD",
                value = respGenerarPin.pinBlock
            });

            ResponsePinBlock respTraducirPin = TraducirPinBlock(respLogin, respGenerarPin);

            item.Add("PIN_BLOCK", new Parametro
            {
                key = "PIN_BLOCK",
                value = respTraducirPin.pinBlock
            });

            try
            {
                ResponseCVV respCvv = GetCVV(tarjeta, fechaVigencia, serviceCode);

                item.Add("CVV2", new Parametro
                {
                    key = "CVV2",
                    value = respCvv.CVV2
                });

                item.Add("CVV", new Parametro
                {
                    key = "CVV",
                    value = respCvv.CVV
                });

                item.Add("ICVV", new Parametro
                {
                    key = "ICVV",
                    value = respCvv.iCVV2
                });
            }
            catch (Exception ex)
            {
                Logueo.Error("[ObtieneOrigenDatosWSRestHSM] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }

        private static ResponseLoginToken GetLoginToken()
        {
            string urlLogin = Constants.ObtenerConfiguracion("urlLogin_Token");
            string clientID = Constants.ObtenerConfiguracion("clientId_Token");
            string clientSecret = Constants.ObtenerConfiguracion("clientSecret_Token");
            string grantType = Constants.ObtenerConfiguracion("grantType_Token");
            string username = Constants.ObtenerConfiguracion("username_Token");
            string pwd = Constants.ObtenerConfiguracion("pwd_Token");

            Log.EntradaSalida("[GetLoginToken] [" + urlLogin + "]", false);

            var clientLogin = new RestClient(urlLogin);
            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("client_id", clientID);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("grant_type", grantType);
            request.AddParameter("username", username);
            request.AddParameter("password", pwd);

            IRestResponse responseLogin = clientLogin.Execute(request);



            if (!responseLogin.StatusCode.Equals(HttpStatusCode.OK))
            {
                throw new Exception("Error al realizar el login para obtener el token: " + JsonConvert.SerializeObject(responseLogin.Content));
            }

            Log.EntradaSalida("[GetLoginToken] [Aprobada]", true);

            return JsonConvert.DeserializeObject<ResponseLoginToken>(responseLogin.Content);
        }

        private static ResponsePinBlock GenerarPinBlock(ResponseLoginToken respLogin, string tar)
        {
            string urlGeneratePin = Constants.ObtenerConfiguracion("urlGeneratePin");
            string keyPinBlock = Constants.ObtenerConfiguracion("keyPinBlock");

            int min = 1000;
            int max = 9999;

            Random rnd = new Random();

            RequestGenerarPinBlock reqGenerar = new RequestGenerarPinBlock()
            {
                cardPAN = tar,
                clearPIN = rnd.Next(min, max + 1).ToString(),
                keyPINBlock = keyPinBlock
            };

            Log.EntradaSalida("[GenerarPinBlock] [" + urlGeneratePin + "]", false);

            var clientGenerate = new RestClient(urlGeneratePin);
            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + respLogin.access_token);
            request.AddJsonBody(reqGenerar);

            IRestResponse responseGenerate = clientGenerate.Execute(request);


            if (!responseGenerate.StatusCode.Equals(HttpStatusCode.OK))
            {
                throw new Exception("Error al realizar la generación del pin block: " + JsonConvert.SerializeObject(responseGenerate));
            }

            ResponsePinBlock response = JsonConvert.DeserializeObject<ResponsePinBlock>(responseGenerate.Content);
            Log.EntradaSalida("[GenerarPinBlock] [result: " + response.result + "]", true);

            return response;
        }

        private static ResponsePinBlock TraducirPinBlock(ResponseLoginToken respLogin, ResponsePinBlock respGenerate)
        {
            string urlTranslatePin = Constants.ObtenerConfiguracion("urlTranslatePin");
            string keySourceVal = Constants.ObtenerConfiguracion("keySource");
            string keyTargetVal = Constants.ObtenerConfiguracion("keyTarget");


            RequestTraducirPinBlock reqTraducir = new RequestTraducirPinBlock()
            {
                keySource = keySourceVal,
                keyTarget = keyTargetVal,
                pinBlockSource = respGenerate.pinBlock
            };

            Log.EntradaSalida("[TraducirPinBlock] [" + urlTranslatePin + "]", false);

            var clientTraducir = new RestClient(urlTranslatePin);
            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + respLogin.access_token);
            request.AddJsonBody(reqTraducir);

            IRestResponse responseTraducir = clientTraducir.Execute(request);

            if (!responseTraducir.StatusCode.Equals(HttpStatusCode.OK))
            {
                throw new Exception("Error al realizar el translate del pin block: " + JsonConvert.SerializeObject(responseTraducir));
            }

            ResponsePinBlock response = JsonConvert.DeserializeObject<ResponsePinBlock>(responseTraducir.Content);
            Log.EntradaSalida("[TraducirPinBlock] [Aprobada]", true);

            return response;
        }

        private static ResponseCVV GetCVV(string tar, string fechaVen, string serviceCode)
        {
            string urlGenerateCVV = Constants.ObtenerConfiguracion("urlGenCVV");
            string cvka = Constants.ObtenerConfiguracion("cvka");
            string cvkb = Constants.ObtenerConfiguracion("cvkb");
            string cred = Constants.ObtenerConfiguracion("credenciales");


            RequestCVV reqCvv = new RequestCVV()
            {
                CVKA = cvka,
                CVKB = cvkb,
                PAN = tar,
                ExpirationDate = fechaVen,
                ServiceCode = serviceCode
            };

            Log.EntradaSalida("[GetCVV] [" + urlGenerateCVV + "]", false);

            var clientCVV = new RestClient(urlGenerateCVV);
            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Credenciales", cred);
            request.AddJsonBody(reqCvv);

            IRestResponse responseCVV = clientCVV.Execute(request);



            ResponseCVV responseCVVTemp = JsonConvert.DeserializeObject<ResponseCVV>(responseCVV.Content);

            Log.EntradaSalida("[GetCVV] [" + responseCVVTemp.DescRespuesta + "]", true);

            if (!responseCVVTemp.CodRespuesta.Equals("0000"))
            {
                throw new Exception("Error al realizar la obtención de datos cvv");
            }

            return responseCVVTemp;
        }

        private static void ObtieneDireccionEntrega(Dictionary<string, Parametro> item, List<EMBOZO_Parametros> lstEMBOZO_Parametros)
        {
            string credenciales = Constants.ObtenerConfiguracion("credLoginWS");
            string connApp = Constants.ObtenerConfiguracion("BDReadAppConect");
            string urlQR = Constants.ObtenerConfiguracion("urlQR");
            string folio = GetValue(lstEMBOZO_Parametros.Where(w => w.f_key.Equals("CODIGO_QR")).FirstOrDefault(), item, DateTime.Now).Trim();
            string cliente = GetValue(lstEMBOZO_Parametros.Where(w => w.f_key.Equals("CLIENTE")).FirstOrDefault(), item, DateTime.Now);

            ResponseConsultarTarPorFolio respConsulta = ConsultarTarjetaPorFolioParabilium(credenciales, folio, tokenWSPB, cliente);

            if (respConsulta.CodRespuesta.Equals("0072") || respConsulta.CodRespuesta.Equals("0070"))
            {
                ResponseLoginWsPB respLoginWs = GetLoginTokenWSParabilium(credenciales);
                tokenWSPB = respLoginWs.Token;
                respConsulta = ConsultarTarjetaPorFolioParabilium(credenciales, folio, tokenWSPB, cliente);
            }

            if (!respConsulta.CodRespuesta.Equals("0000"))
            {
                throw new Exception("Error al realizar la Consulta Tarjeta Por Folio: " + respConsulta.DescRespuesta);
            }
            DAOAutorizador.EjecutarOrigenDatosDireccionEntrega(connApp, respConsulta.IDUsuario, item);
            //DAOAutorizador.EjecutarOrigenDatosDireccionEntrega(connApp, "ECC2FDC9-3305-47C0-A650-54D519DB3448", item);//Para pruebas

            Parametro param = item["CODIGO_QR"];
            param.value = urlQR + item["CODIGO_QR"].value;
        }


        private static ResponseLoginWsPB GetLoginTokenWSParabilium(string credenciales)
        {
            string urlLogin = Constants.ObtenerConfiguracion("urlLoginWS");
            string user = Constants.ObtenerConfiguracion("userLoginWS");
            string pwd = Constants.ObtenerConfiguracion("pwdLoginWS");

            RequestLoginWsPB reqLogin = new RequestLoginWsPB()
            {
                NombreUsuario = user,
                Password = pwd
            };

            Log.EntradaSalida("[GetLoginTokenWSParabilium] [" + urlLogin + "]", false);

            var clientLogin = new RestClient(urlLogin);
            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Credenciales", credenciales);
            request.AddJsonBody(reqLogin);

            IRestResponse responseLoginTemp = clientLogin.Execute(request);


            ResponseLoginWsPB responseLogin = JsonConvert.DeserializeObject<ResponseLoginWsPB>(responseLoginTemp.Content);

            if (!responseLogin.CodRespuesta.Equals("0000"))
            {
                throw new Exception("Error al realizar el login para obtener el token: " + JsonConvert.SerializeObject(responseLogin));
            }

            Log.EntradaSalida("[GetLoginTokenWSParabilium] [Aprobada]", true);

            return responseLogin;
        }

        private static ResponseConsultarTarPorFolio ConsultarTarjetaPorFolioParabilium(string credenciales, string folio, string token, string cliente)
        {
            string urlConsulta = Constants.ObtenerConfiguracion("urlConsultaFolioWS");


            RequestConsultarTarPorFolio reqConsulta = new RequestConsultarTarPorFolio()
            {
                Folio = folio,
                ClaveEmpresa = cliente.Trim()
            };

            Log.EntradaSalida("[ConsultarTarjetaPorFolioParabilium] [" + urlConsulta + "]", false);

            var clientConsulta = new RestClient(urlConsulta);
            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Credenciales", credenciales);
            request.AddHeader("Authorization", string.IsNullOrEmpty(token)
                                                ? token
                                                : "Bearer " + token);
            request.AddJsonBody(reqConsulta);

            IRestResponse responseConsultaTemp = clientConsulta.Execute(request);

            ResponseConsultarTarPorFolio responseConsulta = JsonConvert.DeserializeObject<ResponseConsultarTarPorFolio>(responseConsultaTemp.Content);

            if (!responseConsulta.CodRespuesta.Equals("0000"))
            {
                if (responseConsulta.CodRespuesta.Equals("0072") || responseConsulta.CodRespuesta.Equals("0070"))
                    return responseConsulta;
                throw new Exception("Error al realizar la Consulta Tarjeta Por Folio: " + JsonConvert.SerializeObject(responseConsulta));
            }

            Log.EntradaSalida("[ConsultarTarjetaPorFolioParabilium] [Aprobada]", true);

            return responseConsulta;
        }
    }
}
