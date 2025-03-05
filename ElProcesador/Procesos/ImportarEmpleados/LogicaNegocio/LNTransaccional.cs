using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImportarEmpleados.WsTransaction;
using ImportarEmpleados.Utilidades;
using ImportarEmpleados.Entidades;
using CommonProcesador;
namespace ImportarEmpleados.LogicaNegocio
{
    class LNTransaccional
    {
        public static RespuestaTransaccional ProcesaOperacion(TrxOperacion laOperacion, String ISO_ID)
        {
            try
            {
                ImportarEmpleados.WsTransaction.TransactionProcess webService = new TransactionProcess();
                StringBuilder envio = new StringBuilder();
                
                //Header
                envio.Append("TipoOperacion: [" + laOperacion.elTipoOperacion.ToString().ToUpper() + "]; ");
                string pTransactionTyper = "01";
                envio.Append("pTransactionTyper: [" + pTransactionTyper + "]; ");
                string pVersionNumber = "50";
                envio.Append("pVersionNumber: [" + pVersionNumber + "]; ");
                string pFieldInError = "000";
                envio.Append("pFieldInError: [" + pFieldInError + "]; ");
                string pOriginalEntity = "0";
                envio.Append("pOriginalEntity: [" + pOriginalEntity + "]; ");
                string pDestinationEntity = "5";
                envio.Append("pDestinationEntity: [" + pDestinationEntity + "]; ");

                //Definicion de Variables y Asignacion de Valores
                string pTransactionTypeCode = laOperacion.ProccesingCode.Substring(0, 2);
                envio.Append("pTransactionTypeCode: [" + pTransactionTypeCode + "]; ");
                string pAccountTypeCode1 = laOperacion.ProccesingCode.Substring(2, 2);
                envio.Append("pAccountTypeCode1: [" + pAccountTypeCode1 + "]; ");
                string pAccountTypeCode2 = laOperacion.ProccesingCode.Substring(4, 2);
                envio.Append("pAccountTypeCode2: [" + pAccountTypeCode2 + "]; ");
                // P-4 Transaction Amount
                string pCurrencyCode = laOperacion.CodigoMoneda;
                envio.Append("pCurrencyCode: [" + pCurrencyCode + "]; ");
                string pDecimalPlace = "2";
                envio.Append("pDecimalPlace: [" + pDecimalPlace + "]; ");
                string pTransactionAmount = laOperacion.Monto;
                envio.Append("pTransactionAmount: [" + pTransactionAmount + "]; ");
                //P-7
                string pTPVDate = laOperacion.FechaTransaccion.ToString("yyyyMMdd"); // "20110614";
                envio.Append("pTPVDate: [" + pTPVDate + "]; ");
                string pTPVTime = laOperacion.HoraTransaccion.ToString("yyyyMMddHHmmss"); //"20110614212617";
                envio.Append("pTPVTime: [" + pTPVTime + "]; ");
                //P-22
                string pAccessCard = "1234";
                envio.Append("pAccessCard: [" + pAccessCard + "]; ");
                string pVerficationCard = "1234";
                envio.Append("pVerficationCard: [" + pVerficationCard + "]; ");
                string pPOSEnviroment = "123";
                envio.Append("pPOSEnviroment: [" + pPOSEnviroment + "]; ");
                string pSecurityCard = "1234";
                envio.Append("pSecurityCard: [" + pSecurityCard + "]; ");
                //P-26
                string pMerchantType = "0001";
                envio.Append("pMerchantType: [" + pMerchantType + "]; ");
                //p-32
                string pAcquiringInstitutionIdentification = laOperacion.Adquirente;
                envio.Append("pAcquiringInstitutionIdentification: [" + pAcquiringInstitutionIdentification + "]; ");
                //p-35
                string pTrack2 = laOperacion.Track2;
                envio.Append("pTrack2: [" + pTrack2 + "]; ");
                //p-37
                string pRetrievalReferenceNumber = laOperacion.Ticket;// Guid.NewGuid().ToString().Replace('-', '0').Substring(0, 12);
                envio.Append("pRetrievalReferenceNumber: [" + pRetrievalReferenceNumber + "]; ");
                //P-41
                string pCardAcceptorTerminal = laOperacion.Terminal.PadLeft(16, ' ').Substring(0, 16);
                envio.Append("pCardAcceptorTerminal: [" + pCardAcceptorTerminal + "]; ");
                //P-42

                string pCardAcceptorIdentification = laOperacion.Operador.PadLeft(35, ' ').Substring(0, 35);

                envio.Append("pCardAcceptorIdentification: [" + pCardAcceptorIdentification + "]; ");

                //p-48
                string pAditionalDataPrivate = laOperacion.Sucursal.PadLeft(8, ' ').Substring(0, 8);
                envio.Append("pAditionalDataPrivate: [" + pAditionalDataPrivate + "]; ");
                //P-52
                string pPINData = laOperacion.NIP;
                envio.Append("pPINData: [" + pPINData + "]; ");
                //P-54
                string pAdditionalAmounts = "";
                envio.Append("pAdditionalAmounts: [" + pAdditionalAmounts + "]; ");
                //P-56
                string pOriginalTransactionTypeCode = "0000";
                envio.Append("pOriginalTransactionTypeCode: [" + pOriginalTransactionTypeCode + "]; ");
                string pOriginalTraceAuditNumber = "000000000000";
                envio.Append("pOriginalTraceAuditNumber: [" + pOriginalTraceAuditNumber + "]; ");
                string pOriginalDateTime = "00000000000000";
                envio.Append("pOriginalDateTime: [" + pOriginalDateTime + "]; ");
                string pOriginalAcquiringInstitutionIdentification = "00000000";
                envio.Append("pOriginalAcquiringInstitutionIdentification: [" + pOriginalAcquiringInstitutionIdentification + "]; ");
                //P-59
                string pTransportData = ISO_ID;
                envio.Append("pTransportData: [" + pTransportData + "]; ");
                //P-63
                string pAccessAcountType = laOperacion.TipoMedioAcceso;
                envio.Append("pAccessAcountType: [" + pAccessAcountType + "]; ");
                string pAccessAcountIdentifier = laOperacion.MedioAcceso;
                envio.Append("pAccessAcountIdentifier: [" + pAccessAcountIdentifier + "]; ");
                string pServiceReferenceNumber = laOperacion.Referencia;
                envio.Append("pServiceReferenceNumber: [" + pServiceReferenceNumber + "]; ");
                //P-94
                string pInstitucionCode = laOperacion.Afiliacion;
                envio.Append("pInstitucionCode: [" + pInstitucionCode + "]; ");
                //P-98
                string pPayee = laOperacion.Beneficiario;
                envio.Append("pPayee: [" + pPayee + "]; ");
                //P-111
                string pAdditionalSKUS = "000";
                envio.Append("pAdditionalSKUS: [" + pAdditionalSKUS + "]; ");


                Logueo.EntradaSalida(envio.ToString(), "ProcesadorNocturno", false);

                respuesta resultado = new respuesta();

                resultado = webService.SendTransaction(((int)laOperacion.elTipoOperacion).ToString(), pTransactionTyper, pVersionNumber, pFieldInError, pOriginalEntity,
                    pDestinationEntity, pTransactionTypeCode, pAccountTypeCode1, pAccountTypeCode2, pCurrencyCode, pDecimalPlace,
                    pTransactionAmount, pTPVDate, pTPVTime, pAccessCard, pVerficationCard, pPOSEnviroment, pSecurityCard,
                    pMerchantType, pAcquiringInstitutionIdentification, pTrack2, pRetrievalReferenceNumber, pCardAcceptorTerminal,
                    pCardAcceptorIdentification, pAditionalDataPrivate, pPINData, pAdditionalAmounts, pOriginalTransactionTypeCode,
                    pOriginalTraceAuditNumber, pOriginalDateTime, pOriginalAcquiringInstitutionIdentification,
                    pTransportData, pAccessAcountType, pAccessAcountIdentifier, pServiceReferenceNumber,
                    pInstitucionCode, pPayee, pAdditionalSKUS);

                Logueo.EntradaSalida(getRespuestaLog(resultado), "ProcesadorNocturno", true);

                return GetRespuestaTransaccionalFromRespuesta(resultado);
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                throw err;
            }
        }

        private static RespuestaTransaccional GetRespuestaTransaccionalFromRespuesta(respuesta laRespuesta)
        {

            RespuestaTransaccional unaRespTransaccional = new RespuestaTransaccional
            {
                Autorizacion = laRespuesta._rAuthorizationResponse,
                CodigoRespuesta = laRespuesta._rResponseCode,
                DescripcionRespuesta = laRespuesta._rDescResponseCode,
                FechaHoraOperacion = DateTime.Now,
                Saldos = laRespuesta._rAdditionalAmounts,
                ResultadoOperacion = laRespuesta._rResult,
                IsTimeOut = laRespuesta._rTimeOut
            };

            return unaRespTransaccional;
        }

        private static String getRespuestaLog(respuesta laRespuesta)
        {
            StringBuilder resp = new StringBuilder();

            resp.Append("_rAdditionalAmounts: " + laRespuesta._rAdditionalAmounts + ";");
            resp.Append("_rAuthorizationResponse: " + laRespuesta._rAuthorizationResponse + ";");
            resp.Append("_rCardIssuerTelephoneNumber: " + laRespuesta._rCardIssuerTelephoneNumber + ";");
            resp.Append("_rDescResponseCode: " + laRespuesta._rDescResponseCode + ";");
            resp.Append("_rResponseArrived: " + laRespuesta._rResponseArrived + ";");
            resp.Append("_rMessageAcceptorPrint: " + laRespuesta._rMessageAcceptorPrint + ";");
            resp.Append("_rMessageAcceptorScreen: " + laRespuesta._rMessageAcceptorScreen + ";");
            resp.Append("_rMessageClientScreen: " + laRespuesta._rMessageClientScreen + ";");
            resp.Append("_rMessageClientTicket: " + laRespuesta._rMessageClientTicket + ";");
            resp.Append("_rResponseCode: " + laRespuesta._rResponseCode + ";");
            resp.Append("_rResult: " + laRespuesta._rResult + ";");
            resp.Append("_rTimeOut: " + laRespuesta._rTimeOut + ";");
            resp.Append("tipo: " + laRespuesta.tipo + ";");


            return resp.ToString();
        }
    }
}
