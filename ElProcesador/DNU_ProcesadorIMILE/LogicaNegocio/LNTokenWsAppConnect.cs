using CommonProcesador;
using DNU_ProcesadorIMILE.Entidades.Request;
using DNU_ProcesadorIMILE.Entidades.Response;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorIMILE.LogicaNegocio
{
    public class LNTokenWsAppConnect
    {

        public static string tokenWsAppConnect = "";

        public string WsAppConnectLogIn(RequestLogIn _requestLogIn)
        {
            ResponseLogIn respuesta = new ResponseLogIn();

            Logueo.Evento("Comienza envio peticion a LogIn.");

            try
            {
                string encoded = PNConfig.Get("DNU_ProcesadorIMILE", "CredencialesWsAppConnect");                
                //var cliente = new RestClient(PNConfig.Get("DNU_ProcesadorIMILE", "WsLogInWsAppConnect"));
                string uri = "http://45.32.4.114/wsAppConnect_ClubPremium/api/LogIn/";
                var cliente = new RestClient(uri);
                var request = new RestRequest(Method.POST);

                request.AddHeader("Credenciales", encoded);
                request.AddJsonBody(JsonConvert.SerializeObject(_requestLogIn));

                IRestResponse response = cliente.Execute(request);

                respuesta = JsonConvert.DeserializeObject<ResponseLogIn>(response.Content);

                Logueo.Evento("Termina envio peticion a LogIn.");

                tokenWsAppConnect = respuesta.Token;

                return tokenWsAppConnect;
            }
            catch (Exception ex)
            {
                Logueo.Error("WsAppConnectLogIn(): " + ex.Message);

                return null;
            }
        }
    }
}
