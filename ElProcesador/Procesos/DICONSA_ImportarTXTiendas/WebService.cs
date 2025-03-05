using CommonProcesador;
using DICONSA_ImportarTXTiendas.BaseDatos;
using DICONSA_ImportarTXTiendas.Entidades;
using DICONSA_ImportarTXTiendas.LogicaNegocio;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Data;
using System.Net;
using System.Text;

namespace DICONSA_ImportarTXTiendas
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
                var clientRS = new RestClient(PNConfig.Get("IMPORTATX", "WS_URL"));
                var request = new RestRequest(PNConfig.Get("IMPORTATX", "WS_RequestLogin"), Method.POST);

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
                request.AddHeader("AuthorizationToken", PNConfig.Get("IMPORTATX", "WS_Token"));

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
                throw new Exception("WSLogin(): " + ex.Message);
            }
        }

        /// <summary>
        /// Establece la llamada GET de las operaciones hacia el Web Service
        /// </summary>
        /// <param name="token">Token de autenticación recibido en el login.</param>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        /// <returns>Operaciones de la tienda consultada</returns>
        public static WSJsonResponses.GetOperations WSGetOperations(string token, Int32 IdColectiva)
        {
            try
            {
                StringBuilder sbResponse = new StringBuilder();

                using (var clientWC = new WebClient())
                {
                    clientWC.Headers.Add("Content-Type", "application/json");
                    clientWC.Headers.Add("AuthorizationToken", "Bearer " + token);
                    
                    StringBuilder sbUrlOperations = new StringBuilder();

                    DateTime dtIni = DateTime.Today;
                    DateTime dtFin = DateTime.Today;

                    sbUrlOperations.AppendFormat("{0}{1}?", PNConfig.Get("IMPORTATX", "WS_URL"), PNConfig.Get("IMPORTATX", "WS_RequestOper"));
                    sbUrlOperations.AppendFormat("start_date={0}", String.Format("{0:yyyy-MM-dd}", dtIni));
                    sbUrlOperations.AppendFormat("&end_date={0}", String.Format("{0:yyyy-MM-dd}", dtFin));
                    sbUrlOperations.AppendFormat("&payment_method={0}", "POS");

                    var responseString = clientWC.DownloadString(sbUrlOperations.ToString());

                    //Procesa la respuesta
                    WSJsonResponses.GetOperations wsGetOperations = JsonConvert.DeserializeObject<WSJsonResponses.GetOperations>(responseString);
                    if (!wsGetOperations.Success)
                    {
                        WSJsonResponses.Error elError = JsonConvert.DeserializeObject<WSJsonResponses.Error>(wsGetOperations.Error.ToString());
                        sbResponse.AppendFormat(
                            "No se pudo establecer conexión exitosa con el Web Service (GetOperations). Tienda con ID Colectiva ({0}). Mensaje de error del WS: ({1}) {2}",
                            IdColectiva.ToString(), elError.Code, elError.Message);

                        throw new Exception(sbResponse.ToString());
                    }

                    return wsGetOperations;
                }
            }

            catch (Exception ex)
            {
                throw new Exception("WSGetOperations(): " + ex.Message);
            }
        }


        /// <summary>
        /// Procesa la cadena JSON con las operaciones recibidas del Web Service
        /// </summary>
        /// <param name="IdResumenOperacion">Identificador del resumen de operacion</param>
        /// <param name="operationsResult">JSON con las operaciones</param>
        /// <returns>DataTable con el detalle de operaciones procesado</returns>
        public static DataTable procesaOperaciones(Int32 IdColectiva, WSJsonResponses.GetOperationsResult operationsResult)
        {
            try
            {
                DataTable dt = new DataTable();
                int operacion;

                WSJsonResponses.Operations[] records = JsonConvert.DeserializeObject<WSJsonResponses.Operations[]>(operationsResult.Operations.ToString());

                dt.TableName = "detalle";

                dt.Columns.Add("ID_Colectiva");
                dt.Columns.Add("ID_Transaction");
                dt.Columns.Add("FechaHora");
                dt.Columns.Add("MetodoPago");
                dt.Columns.Add("CodigoAutorizacion");
                dt.Columns.Add("Estatus");
                dt.Columns.Add("Referencia");
                dt.Columns.Add("NombreTarjetahabiente");
                dt.Columns.Add("TipoTarjeta");
                dt.Columns.Add("NumeroTarjeta");
                dt.Columns.Add("EtiquetaTarjeta");
                dt.Columns.Add("MontoTotal");
                dt.Columns.Add("MonedaTotal");
                dt.Columns.Add("MontoPropina");
                dt.Columns.Add("MonedaPropina");
                dt.Columns.Add("MontoComision");
                dt.Columns.Add("MonedaComision");
                dt.Columns.Add("LatitudOrigen");
                dt.Columns.Add("LongitudOrigen");
                dt.Columns.Add("Afiliacion");
                dt.Columns.Add("ARQC");
                dt.Columns.Add("TipoCriptogrma");
                dt.Columns.Add("AID");
                dt.Columns.Add("TipoTransaccion");
                dt.Columns.Add("URL");
                dt.Columns.Add("TieneDevolucion");

                for (operacion = 0; operacion < records.Length; operacion++)
                {
                    dt.Rows.Add();

                    dt.Rows[operacion]["ID_Colectiva"] = IdColectiva;
                    dt.Rows[operacion]["ID_Transaction"] = records[operacion].Transaction;
                    dt.Rows[operacion]["FechaHora"] = Convert.ToDateTime(records[operacion].Timestamp);
                    dt.Rows[operacion]["MetodoPago"] = records[operacion].Payment_Method;
                    dt.Rows[operacion]["CodigoAutorizacion"] = records[operacion].Authorization_Code;
                    dt.Rows[operacion]["Estatus"] = records[operacion].Status;

                    WSJsonResponses.OperationsReference refer = JsonConvert.DeserializeObject<WSJsonResponses.OperationsReference>(records[operacion].Reference.ToString());
                    dt.Rows[operacion]["Referencia"] = refer.Description;

                    WSJsonResponses.OperationsCard card = JsonConvert.DeserializeObject<WSJsonResponses.OperationsCard>(records[operacion].Card.ToString());
                    dt.Rows[operacion]["NombreTarjetahabiente"] = card.Holder_Name;
                    dt.Rows[operacion]["TipoTarjeta"] = card.Type;
                    dt.Rows[operacion]["NumeroTarjeta"] = card.Number;
                    dt.Rows[operacion]["EtiquetaTarjeta"] = card.Label;

                    WSJsonResponses.OperationsAmmounts total = JsonConvert.DeserializeObject<WSJsonResponses.OperationsAmmounts>(records[operacion].Total.ToString());
                    dt.Rows[operacion]["MontoTotal"] = total.Amount;
                    dt.Rows[operacion]["MonedaTotal"] = total.Currency;

                    WSJsonResponses.OperationsAmmounts propina = JsonConvert.DeserializeObject<WSJsonResponses.OperationsAmmounts>(records[operacion].Tip.ToString());
                    dt.Rows[operacion]["MontoPropina"] = propina.Amount;
                    dt.Rows[operacion]["MonedaPropina"] = propina.Currency;

                    WSJsonResponses.OperationsAmmounts comision = JsonConvert.DeserializeObject<WSJsonResponses.OperationsAmmounts>(records[operacion].Commission.ToString());
                    dt.Rows[operacion]["MontoComision"] = comision.Amount;
                    dt.Rows[operacion]["MonedaComision"] = comision.Currency;

                    WSJsonResponses.OperationsOrigin origen = JsonConvert.DeserializeObject<WSJsonResponses.OperationsOrigin>(records[operacion].Origin.ToString());
                    WSJsonResponses.OperationsOriginLocation locacion = JsonConvert.DeserializeObject<WSJsonResponses.OperationsOriginLocation>(origen.Location.ToString());
                    dt.Rows[operacion]["LatitudOrigen"] = locacion.Latitude;
                    dt.Rows[operacion]["LongitudOrigen"] = locacion.Longitude;

                    dt.Rows[operacion]["Afiliacion"] = records[operacion].Affiliation;
                    dt.Rows[operacion]["ARQC"] = records[operacion].ARQC;
                    dt.Rows[operacion]["TipoCriptogrma"] = records[operacion].Cryptogram_Type;
                    dt.Rows[operacion]["AID"] = records[operacion].AID;
                    dt.Rows[operacion]["TipoTransaccion"] = records[operacion].Transaction_Type;
                    dt.Rows[operacion]["URL"] = records[operacion].URL;
                    dt.Rows[operacion]["TieneDevolucion"] = records[operacion].HasDevolution;
                }

                return dt;
            }

            catch (Exception ex)
            {
                throw new Exception("procesaOperaciones(): " +  ex.Message);
            }
        }
    }
}
