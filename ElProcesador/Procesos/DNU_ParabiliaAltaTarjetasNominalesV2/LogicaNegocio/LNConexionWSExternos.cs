using CommonProcesador;
using DNU_ParabiliaAltaTarjetasNominales.Entidades;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    class LNConexionWSExternos
    {

        internal async Task<string> PostWithLoginDW(object data, string user,
              string password, string credenciales, string url, Response respuesta, Hashtable htHeader, LogueoAltaEmpleadoV2 logEmpelado, string urllots)
        {

            var responseLogin = await DoLoginDW(user, password, url, htHeader, logEmpelado);

            Logueo.Evento($"[Login] [RespuestaWS] [Se obtuvo Token Procesador]");//[{JsonConvert.SerializeObject(responseLogin)}]");

            htHeader.Add("Authorization", $"Bearer {responseLogin.Token}");

            var postResponse = await PostItem(data, credenciales, responseLogin.Token, urllots, respuesta, htHeader,  logEmpelado);

            Logueo.Evento($"[Post] [RespuestaWS] [Peticion al Procesador]");//[{JsonConvert.SerializeObject(postResponse)}]");


            return postResponse;

        }

        private async static Task<ResponseLogin> DoLoginDW(string user, string password, string url, Hashtable htHeader, LogueoAltaEmpleadoV2 logEmpelado)
        {
            try
            {
                var postData = new Dictionary<string, string>();
                postData.Add("user_name", user);
                postData.Add("Password", password);

                var data = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(postData));
                WebRequest request = WebRequest.Create($"{url}/api/Login");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;

                foreach (string key in htHeader.Keys)
                {
                    request.Headers.Add(key, htHeader[key].ToString());
                }

               

                using (Stream webStream = request.GetRequestStream())
                {
                    webStream.Write(data, 0, data.Length);
                }

                WebResponse webResponse = request.GetResponse();

                using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                {
                    using (StreamReader responseReader = new StreamReader(webStream))
                    {
                        string response = await responseReader.ReadToEndAsync();
                        webResponse.Close();
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseLogin>(response);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse exWebResponse = ex.Response;

                    using (Stream webStream = exWebResponse.GetResponseStream() ?? Stream.Null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            string response = await responseReader.ReadToEndAsync();
                            exWebResponse.Close();
                            return Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseLogin>(response);
                        }
                    }
                }

                return new ResponseLogin { CodRespuesta = "9999", DescRespuesta = "Error en el consumo del servicio." };

            }
            catch (Exception ex)
            {
                logEmpelado.Error($"Error en Login {ex.Message}");
                return new ResponseLogin { CodRespuesta = "9999", DescRespuesta = ex.Message };
            }


        }

        protected async static Task<string> PostItem(object postData, string credenciales, string token, string url, Response respuesta, Hashtable htHeader, LogueoAltaEmpleadoV2 logEmpelado)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                var data = Newtonsoft.Json.JsonConvert.SerializeObject(postData);//Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(postData));
                request.Method = "POST";
                request.ContentType = "application/json;charset=utf-8";//"application/json";
                request.ContentLength = data.Length;
                foreach (string key in htHeader.Keys)
                {
                    request.Headers.Add(key, htHeader[key].ToString());
                }
                // request.Headers.Add(PluginConstants.CredencialesHeader, credenciales);
                // request.Headers.Add(PluginConstants.AuthorizationHeader, $"Bearer {token}");
                request.UseDefaultCredentials = true;
                request.UserAgent = "[PIEK]";
                Stream webStream = null;
                using (webStream = request.GetRequestStream())
                using (StreamWriter requestWriter = new StreamWriter(webStream))
                {
                    requestWriter.Write(data);
                    //
                }
                //using (Stream webStream = request.GetRequestStream())
                //{
                //    webStream.Write(data, 0, data.Length);
                //}

                try
                {

                    HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();

                    using (Stream webRespStream = webResponse.GetResponseStream() ?? Stream.Null)
                    {
                        using (StreamReader responseReader = new StreamReader(webRespStream))
                        {
                            string response = await responseReader.ReadToEndAsync();
                            if (respuesta != null)
                            {
                                respuesta.CodRespuesta = webResponse.StatusCode.ToString();
                            }
                            webResponse.Close();
                            return response;
                        }
                    }


                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)ex.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                string error = reader.ReadToEnd();
                                if (respuesta != null)
                                {
                                    respuesta.CodRespuesta = errorResponse.StatusCode.ToString();
                                }
                                logEmpelado.Error("[POST digital wh] " + error + " " + ex.Message + " " + ex.StackTrace);
                                return error;
                                //Logueo.EntradaSalida("[" + idlog + "]" + "[AltaDigitalV3] [" + url + "]" + JsonConvert.SerializeObject(error), "", true);
                            }
                        }
                    }
                    else
                    {
                        logEmpelado.Error("[POST digital wh] [" + url + "]" + JsonConvert.SerializeObject(ex));
                    }
                    logEmpelado.Error($"Error en PostItem {ex.Message}");
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new Response { CodRespuesta = "9999", DescRespuesta = "Error en la solicitud" });
                }
            }
            catch (Exception ex)
            {
                respuesta.CodRespuesta = "1064";
                respuesta.DescRespuesta = "Error en la solicitud";
                logEmpelado.Error($"Error en PostItem {ex.Message}");
                return Newtonsoft.Json.JsonConvert.SerializeObject(respuesta);
            }

        }
    }
}

