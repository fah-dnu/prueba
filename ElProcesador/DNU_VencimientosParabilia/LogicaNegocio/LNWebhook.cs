using CommonProcesador;
using DNU_VencimientosParabilia.Entidades;
using DNU_VencimientosParabilia.Utilidades;
using Interfases.Entidades;
using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_VencimientosParabilia.LogicaNegocio
{
    public class LNWebhook
    {


        public static void enviaNotificacion(Movimiento elMovimiento,
                 Dictionary<String, Parametro> losParametros)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            try
            {
                if (losParametros.ContainsKey("@WH_EnviarVencimientos"))
                {
                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [PROCESAVENCPARABILIA, Validando si se debe enviar Notificación " + losParametros["@WH_EnviarVencimientos"].Valor + "]");
                   
                    if (!losParametros["@WH_EnviarVencimientos"].Valor.Equals("1"))
                        return;
                }
                else
                {
                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [PROCESAVENCPARABILIA] No se tiene configurada la key @WH_EnviarVencimientos , no se notificará]");
                    return;
                }

                var mensaje = GeneraMensaje(elMovimiento, losParametros);

                enviaMensajeWebhookAsync(mensaje);


            }
            catch (Exception ex)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [PROCESAVENCPARABILIA, Error general al enviar la notificacion " + ex.Message + "]");
            }

        }

        private static String GeneraMensaje(Movimiento elMovimiento, Dictionary<string, Parametro> losParametros)
        {

            var msj = new WebhookMessage
            {
                ID_Colectiva = losParametros.ContainsKey("@ID_CadenaComercial") ? Convert.ToInt32(losParametros["@ID_CadenaComercial"].Valor) : 0,
                ID_Operacion = elMovimiento.Id_Operacion,
                Prioridad = PNConfig.Get("PROCESAVENCPARABILIA", "PrioridadWebhook"),
                mensaje = new Mensaje
                {
                    Tarjeta = elMovimiento.ClaveMA,
                    TipoMedioAcceso = elMovimiento.TipoMA,
                    Moneda = elMovimiento.MonedaOriginal,
                    Importe = elMovimiento.Importe
                }
            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(msj);

        }

        private static bool enviaMensajeWebhookAsync(String elMensaje)
        {
            string log = "";//;ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            try
            {
                //Se genera el POST
                var url = PNConfig.Get("PROCESAVENCPARABILIA", "URLWebhook");
                var clientRS = new RestClient(url);
                clientRS.Timeout = Convert.ToInt32(PNConfig.Get("PROCESAVENCPARABILIA", "ClientTimeoutWebhook"));
                clientRS.ReadWriteTimeout = Convert.ToInt32(PNConfig.Get("PROCESAVENCPARABILIA", "ClientReadWriteTimeoutWebhook"));

                var request = new RestRequest("", Method.POST);
                request.Timeout = Convert.ToInt32(PNConfig.Get("PROCESAVENCPARABILIA", "RequestTimeoutWebhook"));
                request.ReadWriteTimeout = Convert.ToInt32(PNConfig.Get("PROCESAVENCPARABILIA", "RequestReadWriteTimeoutWebhook"));

                //Body
                request.AddParameter("application/json", elMensaje, ParameterType.RequestBody);

                //Headers HTTP
                request.AddHeader("Content-Type", "application/json");


                //Ejecuta la petición
                var response = clientRS.Execute(request);

                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [" + Newtonsoft.Json.JsonConvert.SerializeObject("[PROCESAVENCPARABILIA] " + response.Headers) + " | "
                                     + Newtonsoft.Json.JsonConvert.SerializeObject(response.Content) + " | "
                                     + response.StatusCode + " | "
                                     + response.StatusDescription + " | "
                                     + response.ErrorException + "]");

             

                RespuestaWebhook resp;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        resp = Newtonsoft.Json.JsonConvert.DeserializeObject<RespuestaWebhook>(response.Content);
                    }
                    catch (Exception ex)
                    {
                        LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [PROCESAVENCPARABILIA, Ocurrio un error al deserializar la respuesta  " + response.Content + "| " + ex.Message + "]");
                       return false;
                    }

                    return resp.CodRespuesta == "0000";
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [PROCESAVENCPARABILIA, Ocurrio un error al enviar la notificación " + ex.Message + "]");
               return false;
            }
        }
    }
}
