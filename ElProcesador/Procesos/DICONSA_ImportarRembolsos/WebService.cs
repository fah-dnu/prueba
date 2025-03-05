using CommonProcesador;
using DICONSA_ImportarRembolsos.BaseDatos;
using DICONSA_ImportarRembolsos.Entidades;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Data;
using System.Net;
using System.Text;

namespace DICONSA_ImportarRembolsos
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
                var clientRS = new RestClient(PNConfig.Get("IMPORTRMB", "WS_URL"));
                var request = new RestRequest(PNConfig.Get("IMPORTRMB", "WS_RequestLogin"), Method.POST);

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
                request.AddHeader("AuthorizationToken", PNConfig.Get("IMPORTRMB", "WS_Token"));

                //Ejecuta la petición
                IRestResponse response = clientRS.Execute(request);
                var content = response.Content;

                //Procesa la respuesta
                WSJsonResponses.Login wsLoginParameters = JsonConvert.DeserializeObject<WSJsonResponses.Login>(content);
                if (!wsLoginParameters.Success)
                {
                    WSJsonResponses.Error elError = JsonConvert.DeserializeObject<WSJsonResponses.Error>(wsLoginParameters.Error.ToString());
                    sbResponse.AppendFormat(
                        "No se pudo establecer conexión exitosa con el Web Service durante el Login. Tienda con ID Colectiva ({0}). Mensaje de error del WS: ({1}) {2}",
                        IdColectiva.ToString(), elError.Code, elError.Message);

                    throw new Exception(sbResponse.ToString());
                }

                //Se valida el token obtenido
                WSJsonResponses.LoginConnection wsConnection = JsonConvert.DeserializeObject<WSJsonResponses.LoginConnection>(wsLoginParameters.Connection.ToString());
                if (String.IsNullOrEmpty(wsConnection.Token))
                {
                    sbResponse.AppendFormat("La autenticación con el Web Service NO fue exitosa. Tienda con ID Colectiva ({0})", IdColectiva.ToString());
                    throw new Exception(sbResponse.ToString());
                }

                return wsConnection.Token;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Establece la llamada GET de los rembolsos hacia el Web Service
        /// </summary>
        /// <param name="token">Token de autenticación recibido en el login.</param>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        /// <returns>Rembolsos de la tienda consultada</returns>
        public static WSJsonResponses.GetWithdrawal WSGetWithdrawal(string token, Int32 IdColectiva)
        {
            try
            {
                StringBuilder sbResponse = new StringBuilder();

                //Se genera el GET con las fechas ingresadas
                using (var clientWC = new WebClient())
                {
                    clientWC.Headers.Add("Content-Type", "application/json");
                    clientWC.Headers.Add("AuthorizationToken", "Bearer " + token);

                    StringBuilder sbUrlWithdrawal = new StringBuilder();

                    DateTime dtIni = DateTime.Today;
                    DateTime dtFin = DateTime.Today;

                    sbUrlWithdrawal.AppendFormat("{0}{1}?", PNConfig.Get("IMPORTRMB", "WS_URL"), PNConfig.Get("IMPORTRMB", "WS_RequestWith"));
                    sbUrlWithdrawal.AppendFormat("start_date={0}", String.Format("{0:yyyy-MM-dd}", dtIni));
                    sbUrlWithdrawal.AppendFormat("&end_date={0}", String.Format("{0:yyyy-MM-dd}", dtFin));

                    var responseString = clientWC.DownloadString(sbUrlWithdrawal.ToString());

                    //Procesa la respuesta
                    WSJsonResponses.GetWithdrawal wsGetWithdrawal = JsonConvert.DeserializeObject<WSJsonResponses.GetWithdrawal>(responseString);
                    if (!wsGetWithdrawal.Success)
                    {
                        WSJsonResponses.Error elError = JsonConvert.DeserializeObject<WSJsonResponses.Error>(wsGetWithdrawal.Error.ToString());
                        sbResponse.AppendFormat(
                            "No se pudo establecer conexión exitosa con el Web Service (GetWithdrawal). Tienda con ID Colectiva ({0}). Mensaje de error del WS: ({1}) {2}",
                            IdColectiva.ToString(), elError.Code, elError.Message);

                        throw new Exception(sbResponse.ToString());
                    }

                    return wsGetWithdrawal;
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Procesa la cadena JSON con los rembolsos recibidos del Web Service
        /// </summary>
        /// <param name="operationsResult">JSON con los rembolsos</param>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        /// <returns>DataTable con los rembolsos procesados</returns>
        public static DataTable procesaRembolsos(WSJsonResponses.GetWithdrawalResult withdrawalResult, Int32 IdColectiva)
        {
            try
            {
                DataTable dt = new DataTable();

                WSJsonResponses.WithdrawalOperations[] records = JsonConvert.DeserializeObject<WSJsonResponses.WithdrawalOperations[]>(withdrawalResult.Operations.ToString());

                dt.Columns.Add("ID_Colectiva");
                dt.Columns.Add("MontoTotal");
                dt.Columns.Add("MonedaTotal");
                dt.Columns.Add("Estatus");
                dt.Columns.Add("NombreBanco");
                dt.Columns.Add("Alias");
                dt.Columns.Add("SufijoNumeroCuenta");
                dt.Columns.Add("MontoComision");
                dt.Columns.Add("MonedaComision");
                dt.Columns.Add("FechaHoraSolicitud");
                dt.Columns.Add("FechaHoraAplicacion");


                for (int rembolso = 0; rembolso < records.Length; rembolso++)
                {
                    dt.Rows.Add();

                    dt.Rows[rembolso]["ID_Colectiva"] = IdColectiva;

                    WSJsonResponses.OperationsAmmounts importe = JsonConvert.DeserializeObject<WSJsonResponses.OperationsAmmounts>(records[rembolso].Total.ToString());
                    dt.Rows[rembolso]["MontoTotal"] = importe.Amount;
                    dt.Rows[rembolso]["MonedaTotal"] = importe.Currency;

                    dt.Rows[rembolso]["Estatus"] = records[rembolso].Status;

                    WSJsonResponses.BankAccount banco = JsonConvert.DeserializeObject<WSJsonResponses.BankAccount>(records[rembolso].Bank_Account.ToString());
                    dt.Rows[rembolso]["NombreBanco"] = banco.Bank_Name;
                    dt.Rows[rembolso]["Alias"] = banco.Alias;
                    dt.Rows[rembolso]["SufijoNumeroCuenta"] = banco.Account_Number_Suffix;

                    WSJsonResponses.OperationsAmmounts comision = JsonConvert.DeserializeObject<WSJsonResponses.OperationsAmmounts>(records[rembolso].Commission.ToString());
                    dt.Rows[rembolso]["MontoComision"] = comision.Amount;
                    dt.Rows[rembolso]["MonedaComision"] = comision.Currency;

                    DateTime fechaSolic = Convert.ToDateTime(records[rembolso].Date_Request);
                    dt.Rows[rembolso]["FechaHoraSolicitud"] = fechaSolic;

                    DateTime fechaApl = Convert.ToDateTime(records[rembolso].Date_Apply);
                    dt.Rows[rembolso]["FechaHoraAplicacion"] = fechaApl;
                }

                return dt;
            }

            catch (Exception ex)
            {
                throw new Exception("procesaRembolsos(): " + ex.Message);
            }
        }
    }
}
