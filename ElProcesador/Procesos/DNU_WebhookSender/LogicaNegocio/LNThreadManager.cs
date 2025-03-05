using CommonProcesador;
using DNU_WebhookSender.DataBase;
using DNU_WebhookSender.Entities;
using DNU_WebhookSender.LogicaNegocio;
using DNU_WebhookSender.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;


namespace DNU_WebhookSender.LogicaNegocio
{
    public class LNThreadManager
    {
        public static List<Thread> lstThreads  = new List<Thread>();
        public static List<Thread> lstThreadsToRemove;
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "WEBHOOK";
                _NombreNewRelic = PNConfig.Get(ClaveProceso, "NombreNewRelic");
                if (String.IsNullOrEmpty(_NombreNewRelic))
                {
                    _NombreNewRelic = ClaveProceso + "-SINNOMBRE";
                    //Logueo.Evento("Se coloco nombre generico para instrumentacion NewRelic al no encontrar el parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                    //Logueo.Error("No se encontro parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                }
                //else
                //{
                //    //Logueo.Evento("Se encontro parametro NombreNewRelic: " + _NombreNewRelic + " [" + ClaveProceso + "]");
                //}
                return _NombreNewRelic;
            }
        }

        [Transaction]
        public static void startThreadingProcess(string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("startThreadingProcess");
            try
            {
                int threads = 1;
                lstThreadsToRemove = new List<Thread>();
                List<Mensaje> messages;
                var top = Convert.ToInt32(PNConfig.Get("WEBHOOK", "TOP_COUNT_MESSAGES").ToString());
                var tiempoEspera = (PNConfig.Get("WEBHOOK", "TiempoEsperaParaEnviar").ToString());

                messages = new List<Mensaje>();
                messages = DAOMensajes.ListaMensajesSinProcesar(top, ip, log, tiempoEspera);


                foreach (var tr in lstThreads)
                {
                    if (tr.ThreadState == ThreadState.Stopped)
                    {
                        lstThreadsToRemove.Add(tr);
                        continue;
                    }

                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER  Hilos registrados {0} - {1}", tr.Name, tr.ThreadState) + "]");
                }

                foreach (var item in lstThreadsToRemove)
                {
                    item.Abort();
                    lstThreads.Remove(item);
                }

                if (messages.Count > 0)
                {
                    threads = Convert.ToInt32(PNConfig.Get("WEBHOOK", "NumeroHilos").ToString());
                    //threads = 1;

                    if (messages.Count == 0)
                    {
                        throw new Exception("[WEBHOOK] No se localizaron mensajes por enviar");
                    }

                    int messagesPerThread = messages.Count / threads;

                    if (messagesPerThread <= 1)
                    {
                        messagesPerThread = messages.Count;
                    }

                    //DAOMensajes.CambiaMensajesEnProceso(messages);
                    callInThreads(groupedMessagesPerThread(messagesPerThread, messages,ip,log),ip,log);
                }
            }
            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER,  Error en la ejecución del proceso en hilos - {0}", ex.Message) + "]");
                ApmNoticeWrapper.NoticeException(ex);
                //return false;
            }
        }

        private static void callInThreads(List<List<Mensaje>> messages, string ip, string log)
        {
            Thread t;
            int i = 0;
            string Name = "";
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            foreach (var item in messages)
            {
                try
                {
                    BDMensajesWebhook dbWebhookConnections = new BDMensajesWebhook();
                    dbWebhookConnections.GetEscrituraConnection(i, ip,  log);
                    dbWebhookConnections.GetLecturaConnection(i,  ip, log);

                    t = new Thread(() =>
                    {
                        LNEnvio.LanzaEnvioDeMensajes(item, dbWebhookConnections, i, ip,  log);
                        LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER,  Termina Hilo MessagesID {0} - {1}", item.FirstOrDefault().ID_Mensaje, item.LastOrDefault().ID_Mensaje) + "]");
                    });
                    Name = String.Format("Hilo Con MessageID {0} - {1}", item.FirstOrDefault().ID_Mensaje, item.LastOrDefault().ID_Mensaje);
                    t.Name = Name;
                    lstThreads.Add(t);
                    t.Start();
                    i++;


                }
                catch (Exception ex)
                {
                    LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER,  Error en el lanzamiento del hilo - {0} - {1}", ex.Message, Name) + "]");
                }
            }
        }

        private static List<List<Mensaje>> groupedMessagesPerThread(int messagesPerThread, List<Mensaje> messages, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            List<List<Mensaje>> lstMessages = new List<List<Mensaje>>();
            try
            {
                int count = 1;
                int countAdded = 0;

                for (int i = 0; i < messages.Count; i++)
                {
                    if (count == messagesPerThread || i == messages.Count - 1)
                    {
                        var tmpLst = messages.Skip(countAdded).Take(messagesPerThread).ToList();
                        countAdded += tmpLst.Count;

                        lstMessages.Add(tmpLst);

                        count = 0;
                    }

                    count++;
                }
            }
            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER,  Error en la agrupacion de mensajes- {0}", ex.Message) + "]");
            }

            return lstMessages;
        }


        public static void Stop(string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                if (lstThreads == null)
                {
                    return;
                }
                LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER, Deteniendo hilos {0}", lstThreads.Count) + "]");
                
                foreach (var item in lstThreads)
                {

                    try
                    {
                        LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER, Deteniendo hilos {0}", item.Name) + "]");
                        //if (item.ThreadState == ThreadState.Running)
                        //{
                        item.Abort();
                        //}
                    }
                    catch (Exception ex)
                    {
                        LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER, Error al detener el hilo {0} - {0}", item.Name, ex.Message) + "]");
                   }
                }
                LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER] Hilos detenidos {0}", lstThreads.Count) + "]");
             }
            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER] Error al detener los hilos {0}", ex.Message) + "]");
            }
        }
    }
}
