using CommonProcesador;
using Newtonsoft.Json;
using RestSharp;
using SMARTTKT_GeneradorPedidos.Entidades;
using System;
using System.Text;

namespace SMARTTKT_GeneradorPedidos
{
    public class WebService
    {
        /// <summary>
        /// Establece la solicitud de códigos al Web Service
        /// </summary>
        /// <param name="IdPedido">Identificador del pedido</param>
        /// <returns>TRUE si la solicitud fue exitosa</returns>
        public static void WS_Promociones(String IdPedido)
        {
            try
            {
                StringBuilder sbResponse = new StringBuilder();
                StringBuilder sbUrl = new StringBuilder();

                string user = PNConfig.Get("GENERAPEDIDOS", "WS_Usuario");
                string pswd = PNConfig.Get("GENERAPEDIDOS", "WS_Pswd");

                sbUrl.AppendFormat("{0}?", PNConfig.Get("GENERAPEDIDOS", "WS_RequestPromo"));
                sbUrl.AppendFormat("wsusuario={0}", user);
                sbUrl.AppendFormat("&wspassword={0}", pswd);
                sbUrl.AppendFormat("&idPedido={0}", IdPedido);

                //Se establece la URL
                var clientRS = new RestClient(PNConfig.Get("GENERAPEDIDOS", "WS_URL"));
                //Se establece la solicitud y el método
                var request = new RestRequest(sbUrl.ToString(), Method.POST);

                //Ejecuta la petición
                IRestResponse response = clientRS.Execute(request);
                var content = response.Content;

                //Procesa la respuesta
                WSJsonResponses.Promociones wsParameters = JsonConvert.DeserializeObject<WSJsonResponses.Promociones>(content);
                if (wsParameters.CodigoRespuesta != "00")
                {
                    WSJsonResponses.Error elError = JsonConvert.DeserializeObject<WSJsonResponses.Error>(wsParameters.Error.ToString());
                    sbResponse.AppendFormat(
                        "Error al solicitar las promociones al Web Service. ID Pedido ({0}). Mensaje de error del WS: ({1}) {2}",
                        IdPedido, elError.CodigoRespuesta, elError.Descripcion);

                    Logueo.Error(sbResponse.ToString());
                }
            }

            catch (Exception ex)
            {
                throw new Exception("WS_Promociones(): " + ex.Message);
            }
        }
    }
}
