using CommonProcesador;
using DNU_ProcesadorT112.BaseDatos;
using DNU_ProcesadorT112.Entidades;
using Interfases.Entidades;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorT112.LogicaNegocio
{
    public class LNWebhook
    {


        public static  void enviaNotificacion(Movimiento elMovimiento,
                Dictionary<String, Parametro> losParametros,
                int IdPoliza,
                int IdFicheroDetalle,
                SqlConnection conn)
        {
            try
            {
                if(losParametros.ContainsKey("@WH_EnviarT112"))
                {
                    Logueo.Evento("[PROCESAT112] Validando si se debe enviar Notificación " + losParametros["@WH_EnviarT112"].Valor);

                    if (!losParametros["@WH_EnviarT112"].Valor.Equals("1"))
                        return;
                }
                else
                {
                    Logueo.Evento("[PROCESAT112] No se tiene configurada la key @WH_EnviarT112 , no se notificará");
                    return;
                }


                Logueo.Evento("[PROCESAT112] Enviando notificacion para ficherodetalle " + IdFicheroDetalle);

                var mensaje = GeneraMensaje(elMovimiento, losParametros, IdPoliza);

                if (enviaMensajeWebhookAsync(mensaje))
                {
                    DAOWebhook.actualizaEstatusEnvioNotificacion(IdFicheroDetalle);
                }
            }
            catch(Exception ex)
            {
                Logueo.Error("[PROCESAT112] Error general al enviar la notificacion " + ex.Message);
            }

        }

        private static String GeneraMensaje(Movimiento elMovimiento, Dictionary<string, Parametro> losParametros, int IdPoliza)
        {

            var msj = new WebhookMessage
            {
                ID_Colectiva = losParametros.ContainsKey("@ID_CadenaComercial") ? Convert.ToInt32(losParametros["@ID_CadenaComercial"].Valor) : 0,
                ID_Operacion = 0,
                Prioridad = PNConfig.Get("PROCESAT112", "PrioridadWebhook"),
                mensaje = new Mensaje
                {
                    IdMovimiento = IdPoliza.ToString(),
                    Tarjeta = elMovimiento.ClaveMA,
                    TipoMedioAcceso = elMovimiento.TipoMA,
                    Moneda = elMovimiento.MonedaOriginal,
                    Importe = elMovimiento.Importe
                }
            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(msj);

        }

        private  static bool enviaMensajeWebhookAsync(String elMensaje)
        {
            try
            {
                //Se genera el POST
                var url = PNConfig.Get("PROCESAT112", "URLWebhook");
                var clientRS = new RestClient(url);
                clientRS.Timeout = Convert.ToInt32(PNConfig.Get("PROCESAT112", "ClientTimeoutWebhook"));
                clientRS.ReadWriteTimeout = Convert.ToInt32(PNConfig.Get("PROCESAT112", "ClientReadWriteTimeoutWebhook"));

                var request = new RestRequest("", Method.POST);
                request.Timeout = Convert.ToInt32(PNConfig.Get("PROCESAT112", "RequestTimeoutWebhook"));
                request.ReadWriteTimeout = Convert.ToInt32(PNConfig.Get("PROCESAT112", "RequestReadWriteTimeoutWebhook"));

                //Body
                request.AddParameter("application/json", elMensaje, ParameterType.RequestBody);

                //Headers HTTP
                request.AddHeader("Content-Type", "application/json");


                //Ejecuta la petición
                var response =  clientRS.Execute(request);

                Logueo.Evento(Newtonsoft.Json.JsonConvert.SerializeObject("[PROCESAT112] " + response.Headers) + " | "
                                     + Newtonsoft.Json.JsonConvert.SerializeObject(response.Content) + " | "
                                     + response.StatusCode + " | "
                                     + response.StatusDescription + " | "
                                     + response.ErrorException
                                     );


                RespuestaWebhook resp;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        resp = Newtonsoft.Json.JsonConvert.DeserializeObject<RespuestaWebhook>(response.Content);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[PROCESAT112] Ocurrio un error al deserializar la respuesta  " + response.Content + "| " + ex.Message);
                        return false;
                    }

                    return resp.CodRespuesta == "0000";
                }
                else
                {
                    return false;
                }

            }
            catch(Exception ex)
            {
                Logueo.Error("[PROCESAT112] Ocurrio un error al enviar la notificación "+ex.Message);
                return false;
            }
        }
    }
}
