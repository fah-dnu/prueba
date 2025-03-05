using CommonProcesador;
using DNU_NewRelicNotifications.BussinesLayer;
using DNU_NewRelicNotifications.Models;
using NewRelic.Api.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_NewRelicNotifications.Services.Wrappers
{
    public class ApmNoticeWrapper
    {

        public static void SetTransactionName(string transactionName)
        {
            NewRelic.Api.Agent.NewRelic.SetTransactionName("generic", transactionName);
        }

        public static void SetTransactionName(string category,string transactionName)
        {
            NewRelic.Api.Agent.NewRelic.SetTransactionName(category, transactionName);
        }

        public static void SetAplicationName(string transactionName)
        {
            NewRelic.Api.Agent.NewRelic.SetApplicationName(transactionName);
        }

        /// <summary>
        /// Sets the client identifier in the transaction.
        /// </summary>
        /// <param name="identifierId">The customer identifier.</param>
        public static void SetClientIdentifier(string identifierId)
        {
            NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("customer.client_id", identifierId);
        }

        /// <summary>
        /// Sets the client name to the transaction.
        /// </summary>
        /// <param name="name">The customer name.</param>
        public static void SetClientName(string name)
        {
            NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("customer.name", name);
        }

        /// <summary>
        ///  Sets the customer request identifier to the transaction.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        public static void SetRequestIdentifier(string requestId)
        {
            NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("customer.request_id", requestId);
        }

        /// <summary>
        /// Notices an exception within the current transaction.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void NoticeException(Exception exception, Guid? idLog = null)
        {
            try
            {
                //  throw new Exception("error spei 20200810");//
                NewRelic.Api.Agent.NewRelic.NoticeError(exception);
            }
            catch (Exception ex)
            {
                Logueo.Error("[NewRelic] " + ex.Message + " " + ex.StackTrace + "[" + (idLog is null ? "" : idLog.ToString()) + "]");
            }
        }

        public static string pruebas(String prueba) {
            //Logueo.Evento("[NewRelic] ");
            return "pruebaExitosa";

        }

        /// <summary>
        /// Notices an error message Within the current transaction
        /// </summary>
        /// <param name="message"></param>
        public static void NoticeErrorMessage(string message, Guid? idLog = null)
        {
            try
            {
                //  throw err();
                // throw new Exception("error");// throw err();
                //prueba
                Dictionary<string, object> parameters = null;
                NewRelic.Api.Agent.NewRelic.NoticeError(message, parameters);
            }
            catch (Exception ex)
            {
                // NewRelic.Api.Agent.NewRelic.NoticeError(ex);
                Logueo.Error("[NewRelic] " + ex.Message + " " + ex.StackTrace + "[" + (idLog is null ? "" : idLog.ToString()) + "]");

            }
        }

        public static void IgnoreTransaction()
        {
            NewRelic.Api.Agent.NewRelic.IgnoreTransaction();
        }

        //public static void setDataTransactionInformationNewRelic(string claveCliente, string conexion, Guid? idLog = null)
        //// public static void setDataTransactionInformationNewRelic(HttpRequestMessage httpRequest, dynamic dynamicClass, Guid? idLog = null)
        //{
        //    try
        //    {
        //        BLApmTransaction _blapm = new BLApmTransaction(conexion);
        //        ClientInfo info = _blapm.GetClientInfo(new ClientInfo { card = "", mediaAccess = "", typeMediaAccess = "" }, "", "", idLog, claveCliente);
        //        NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("customer.name", info.client);
        //        NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("customer.client_id", info.clientID);
        //        if (idLog != null)
        //            NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("customer.request_id", (idLog is null ? "" : idLog.ToString()));

        //    }
        //    catch (Exception exTask)
        //    {
        //        NoticeException(exTask);
        //    }
        //}

        public static void setDataTransactionInformationNewRelic(string nameAttribute, string valueAttribute)
        // public static void setDataTransactionInformationNewRelic(HttpRequestMessage httpRequest, dynamic dynamicClass, Guid? idLog = null)
        {
            try
            {
               
                NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute(nameAttribute, valueAttribute);
             

            }
            catch (Exception exTask)
            {
                NoticeException(exTask);
            }
        }
        public static void setDataClientInformationNewRelic(string clientName, string clientId, string requestId)
        {
            try
            {
                NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("customer.name", clientName);
                NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("customer.client_id", clientId);
                NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction.AddCustomAttribute("customer.request_id", requestId);
            }
            catch (Exception ex)
            {
                NoticeException(ex);
            }
        }

    }
}
