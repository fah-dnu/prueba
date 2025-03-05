using CommonProcesador;
using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Utilidades;
using Interfases.Entidades;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.LogicaNegocio
{
    public class LNWebhook
    {

        public static bool EnviarNotificacion(RegistroCompensar registro, string idColectiva, string idPoliza, Int64 idOperacion
                                , string etiquetaLogueo, string descEvento, string cveEvento)
        {
                var mensaje = GeneraMensaje(registro, idColectiva, idOperacion, idPoliza, descEvento, cveEvento);

                if (!EnviarMensajeWebhook(mensaje, etiquetaLogueo))
                    return false;
            return true;
        }

        private static String GeneraMensaje(RegistroCompensar reg, string idColectiva, Int64 idOperacion, string idPoliza
                                        , string descEvento, string claveEvento)
        {

            var msj = new WebhookMessage
            {
                ID_Colectiva = Convert.ToInt64(idColectiva),
                ID_Operacion = idOperacion,
                Prioridad = PNConfig.Get("PROCESAT112", "PrioridadWebhook"),
                mensaje = new Mensaje
                {
                    IdMovimiento = idPoliza,
                    Tarjeta = reg.tarjeta,
                    Importe = reg.impCompensacion,
                    Moneda = reg.cveDivisaCompensacion,
                    FechaHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    NumeroAutorizacion = reg.autorizacion,
                    ClaveMovimiento = descEvento,
                    DescripcionMovimiento = claveEvento, 
                    CodigoProceso = reg.codigoProceso,
                    NombreComercio = reg.comercio,
                    MCC = reg.comercioMCC,
                    ClaveRastreo = reg.referencia2,
                    ReferenciaNumerica = reg.referencia,
                    NombreFichero = reg.fichero,
                    FechaPresentacion = reg.fechaPresentacion,
                    ImporteOrigen = reg.impOrigen,
                    DivisaOrigen = reg.cveDivisaOrigen,
                    ImporteDestino = reg.impDestino,
                    DivisaDestino = reg.cveDivisaDestino
                }
            };

            return JsonConvert.SerializeObject(msj);

        }

        private static bool EnviarMensajeWebhook(String elMensaje, string etiquetaLogueo)
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
                var response = clientRS.Execute(request);

                Log.Evento(JsonConvert.SerializeObject(" " + response.Headers) + " | "
                                     + JsonConvert.SerializeObject(response.Content) + " | "
                                     + response.StatusCode + " | "
                                     + response.StatusDescription + " | "
                                     + response.ErrorException
                                     );


                RespuestaWebhook resp;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        resp = JsonConvert.DeserializeObject<RespuestaWebhook>(response.Content);

                        if (resp.CodRespuesta != "0000")
                        {
                            Log.Error(etiquetaLogueo + JsonConvert.SerializeObject(resp));
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        string msj = " Ocurrio un error al deserializar la respuesta  " + response.Content + "| " + ex.Message;
                        Log.Error(etiquetaLogueo + msj);
                        return false;
                    }

                    return true;
                }
                else
                {
                    Log.Error(etiquetaLogueo + " Error response: " + response.StatusCode);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Log.Error(etiquetaLogueo + " Ocurrio un error al enviar la notificación " + ex.Message);

                return false;
            }
        }

        private static decimal ObtenerImporte(string impCompensacion, string impMCCR, string impMarkUp) 
        {
            decimal dImpCompensacion = string.IsNullOrEmpty(impCompensacion)
                                        ? 0
                                        : Convert.ToDecimal(impCompensacion);

            decimal dImpMCCR = string.IsNullOrEmpty(impMCCR)
                                        ? 0
                                        : Convert.ToDecimal(impMCCR);

            decimal dImpMarkUp = string.IsNullOrEmpty(impMarkUp)
                                        ? 0
                                        : Convert.ToDecimal(impMarkUp);

            return dImpCompensacion + dImpMCCR + dImpMarkUp;

        }

        private static string ObtenerCodigoProceso(string codigoProceso, string reverso, string representacion)
        {
            if (reverso.Equals("1"))
                codigoProceso = "R" + codigoProceso;
            if (representacion.Equals("1"))
                codigoProceso = "P" + codigoProceso;

            return codigoProceso;
        }
    }
}
