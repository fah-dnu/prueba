using CommonProcesador;
using DICONSA_SolicitaTransferencias.BaseDatos;
using DICONSA_SolicitaTransferencias.Entidades;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Data;
using System.Net;
using System.Text;

namespace DICONSA_SolicitaTransferencias
{
    public class WebService
    {
        /// <summary>
        /// Establece la llamada POST del Login al Web Service
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        /// <returns>Token de autenticación recibido</returns>
        public static string WSLogin(Int32 IdColectiva)
        {
            try
            {
                StringBuilder sbResponse = new StringBuilder();

                //Se genera el POST para el login
                var clientRS = new RestClient(PNConfig.Get("REQFUNDS", "WS_URL"));
                var request = new RestRequest(PNConfig.Get("REQFUNDS", "WS_RequestLogin"), Method.POST);

                //Body
                DataSet dsCredenciales = DAODNU.ObtenerCredencialesWS(IdColectiva);

                if (dsCredenciales.Tables[0].Rows.Count == 0)
                {
                    sbResponse.AppendFormat("La tienda con ID Colectiva ({0}) no tiene credenciales registradas para el Web Service.", IdColectiva.ToString());
                    throw new Exception(sbResponse.ToString());
                }

                string user = dsCredenciales.Tables[0].Rows[0]["Usuario"].ToString().Trim();
                string pswd = dsCredenciales.Tables[0].Rows[0]["PSP"].ToString().Trim();

                request.AddParameter("username", user);
                request.AddParameter("password", pswd);

                //Headers HTTP
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("AuthorizationToken", PNConfig.Get("REQFUNDS", "WS_Token"));

                //Ejecuta la petición
                IRestResponse response = clientRS.Execute(request);
                var content = response.Content;

                //Procesa la respuesta
                WSJsonResponses.Login wsLoginParameters = JsonConvert.DeserializeObject<WSJsonResponses.Login>(content);
                if (!wsLoginParameters.Success)
                {
                    WSJsonResponses.Error elError = JsonConvert.DeserializeObject<WSJsonResponses.Error>(wsLoginParameters.Error.ToString());
                    sbResponse.AppendFormat(
                        "No se pudo establecer conexión exitosa con el Web Service durante el Login. Mensaje de error del WS: ({0}) {1}",
                        elError.Code, elError.Message);
                    
                    throw new Exception(sbResponse.ToString());
                }

                //Se valida el token obtenido
                WSJsonResponses.LoginConnection wsConnection = JsonConvert.DeserializeObject<WSJsonResponses.LoginConnection>(wsLoginParameters.Connection.ToString());
                if (String.IsNullOrEmpty(wsConnection.Token))
                {
                    throw new Exception("La autenticación con el Web Service NO fue exitosa.");
                }

                return wsConnection.Token;
            }

            catch (Exception ex)
            {
                throw new Exception("WSLogin(): " + ex.Message);
            }
        }

        /// <summary>
        /// Establece la llamada GET de cuentas bancarias hacia el Web Service
        /// </summary>
        /// <param name="token">Token de autenticación recibido en el login.</param>
        /// <returns>Operaciones de la tienda consultada</returns>
        public static WSJsonResponses.GetBankAccounts WSGetBankAccounts(string token)
        {
            try
            {
                StringBuilder sbResponse = new StringBuilder();

                using (var clientWC = new WebClient())
                {
                    clientWC.Headers.Add("Content-Type", "application/json");
                    clientWC.Headers.Add("AuthorizationToken", "Bearer " + token);

                    StringBuilder sbUrlBankAccounts = new StringBuilder();
                    sbUrlBankAccounts.AppendFormat("{0}{1}?", PNConfig.Get("REQFUNDS", "WS_URL"), PNConfig.Get("REQFUNDS", "WS_BankAccounts"));
                    
                    var responseString = clientWC.DownloadString(sbUrlBankAccounts.ToString());

                    //Procesa la respuesta
                    WSJsonResponses.GetBankAccounts wsGetBankAccounts = JsonConvert.DeserializeObject<WSJsonResponses.GetBankAccounts>(responseString);
                    if (!wsGetBankAccounts.Success)
                    {
                        WSJsonResponses.Error elError = JsonConvert.DeserializeObject<WSJsonResponses.Error>(wsGetBankAccounts.Error.ToString());
                        sbResponse.AppendFormat(
                            "No se pudo establecer conexión exitosa con el Web Service (GetBankAccounts). Mensaje de error del WS: ({0}) {1}",
                            elError.Code, elError.Message);
                        
                        throw new Exception(sbResponse.ToString());
                    }

                    return wsGetBankAccounts;
                }
            }

            catch (Exception ex)
            {
                throw new Exception("WSGetBankAccounts(): " + ex.Message);
            }
        }

        /// <summary>
        /// Establece la llamada POST para la solicitud de la transferencia al Web Service de la tienda indicada
        /// </summary>
        /// <param name="token">Token de autenticación recibido en el login.</param>
        /// <param name="BankId">Identificador de la cuenta bancaria</param>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        /// <returns>Respuesta del Web Service a la transferencia</returns>
        public static bool WSPostTransfer(string token, Int64 BankId, Int32 IdColectiva)
        {
            try
            {
                StringBuilder sbResponse = new StringBuilder();

                //Se genera el POST para el transfer
                var clientRS = new RestClient(PNConfig.Get("REQFUNDS", "WS_URL"));
                var request = new RestRequest(PNConfig.Get("REQFUNDS", "WS_RequestTransfer"), Method.POST);

                //Body
                request.AddParameter("bank-account", BankId);

                //Headers HTTP
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("AuthorizationToken", "Bearer " + token);

                Logueo.EntradaSalida("WSTransfer|Tienda: " + IdColectiva.ToString(), "", false);

                //Ejecuta la petición
                IRestResponse response = clientRS.Execute(request);
                var content = response.Content;

                //Procesa la respuesta
                WSJsonResponses.PostTransfer wsPostTransfer = JsonConvert.DeserializeObject<WSJsonResponses.PostTransfer>(content);
                if (!wsPostTransfer.Success)
                {
                    WSJsonResponses.Error elError = JsonConvert.DeserializeObject<WSJsonResponses.Error>(wsPostTransfer.Error.ToString());

                    string JsonErrorRecibido = wsPostTransfer.Error.ToString().Replace("\r\n", "").Replace("\"", "").Replace('"', '\0').Replace(",    ", ";");
                    Logueo.EntradaSalida("WSTransferResult_ERROR: " + JsonErrorRecibido + "|Tienda: " + IdColectiva.ToString(), "", true);

                    if (elError.Code != "InvalidBalanceException")  //Este código lo manda cuando no tiene nada que transferir
                    {
                        sbResponse.AppendFormat
                        ("Solicitud de transferencia no exitosa. Mensaje de error del WS: ({0}) {1}",
                        elError.Code, elError.Message);
                        throw new Exception(sbResponse.ToString());
                    }
                }
                else
                {
                    string JsonRecibido = wsPostTransfer.Result.ToString().Replace("\r\n", "").Replace("\"", "").Replace('"', '\0').Replace(",    ", ";");
                    Logueo.EntradaSalida("WSTransferResult: " + JsonRecibido + "|Tienda: " + IdColectiva.ToString(), "", true);
                }

                return wsPostTransfer.Success;
            }

            catch (Exception ex)
            {
                throw new Exception("WSPostTransfer(): " + ex.Message);
            }
        }
    }
}
