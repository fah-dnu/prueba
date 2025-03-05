using CommonProcesador;
using Dnu.Batch.DataBase;
using Dnu.Batch.DataContracts;
using Dnu.Batch.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace Dnu.Batch.Bonificacion
{
    public class BonificacionProcess : IProcesoNocturno
    {
        public void Detener()
        {
            
        }

        public bool Procesar()
        {
            try
            {
                var section = ConfigurationManager.GetSection("BatchBonificacion") as NameValueCollection;
                
                DataBaseAccess dataBase = new DataBaseAccess();
#if !DEBUG
                Logueo.Evento("Se Inicia Proceso nocturno [BonificacionProcess]");
#endif
                var tickets = dataBase.GetTicketsByStatus(3);
                if (tickets.Count()==0)
                {
#if !DEBUG
                    Logueo.Evento("Finaliza proceso Tickets totales: 0" + tickets);
#endif
                    return true;
                }
#if !DEBUG
                Logueo.Evento("Tickets totales:" + tickets.Count());
#endif
                int numeroMaxIntentos =int.Parse( section["NumeroMaxIntentos"].ToString());
#if !DEBUG
                Logueo.Evento("Numero Maximo de Intentos:" + numeroMaxIntentos);
#endif
                //string usuario = ConfigurationManager.AppSettings["Usuario"].ToString();
                //string password = ConfigurationManager.AppSettings["Password"].ToString();
                //string tipoMedioPago = ConfigurationManager.AppSettings["TipoMedioPago"].ToString();
                //string sku_Beneficio = ConfigurationManager.AppSettings["SKU_Beneficio"].ToString();
                //string terminal = ConfigurationManager.AppSettings["Terminal"].ToString();
                string usuario = section["Usuario"].ToString();
                string password = section["Password"].ToString();
                string tipoMedioPago = section["TipoMedioPago"].ToString();
                string sku_Beneficio = section["SKU_Beneficio"].ToString();
                string terminal = section["Terminal"].ToString();
                ServicePaymentProcess.PaymentProcessClient client = new ServicePaymentProcess.PaymentProcessClient();
                foreach (var ticket in tickets)
                {
                    try
                    {
#if !DEBUG
                        Logueo.Evento("Envio de ticket: " + ticket.NumeroTicket);
#endif
                        var resp = client.Reward(usuario, password, ticket.Email, tipoMedioPago, sku_Beneficio,
                            (float)(ticket.Importe - ticket.Propina), ticket.FechaTicket.ToString("dd/MM/yyyy"), ticket.FechaTicket.ToString("HH:mm:ss"),
                            ticket.NumeroTicket, ticket.ClaveSucursal, ticket.Afiliacion, terminal, ticket.Email, string.Empty);
#if !DEBUG
                        Logueo.Evento("Respuesta, Codigo de Respuesta : " + resp.codigoRespuesta + " ,Autorizacion: " + resp.autorizacion);
#endif
                        dataBase.RegistrarBonificacionTicket(ticket.IdTicketSucursal, ticket.NumeroTicket, resp.codigoRespuesta, resp.autorizacion, numeroMaxIntentos);
#if !DEBUG
                        Logueo.Evento("Registro en BD");
#endif
                        if (resp.codigoRespuesta.Equals("0000"))
                        {

                            var nombre = dataBase.ObtenerInfoMail(ticket.Email);
                            MailUtils mail = new MailUtils();
                            //string subject = ConfigurationManager.AppSettings["SubjectEmail"].ToString();
                            //StringBuilder body = new StringBuilder(File.ReadAllText(ConfigurationManager.AppSettings["PathMailTemplate"], Encoding.UTF8));
                            string subject = section["SubjectEmail"].ToString();
                            StringBuilder body = new StringBuilder(File.ReadAllText(section["PathMailTemplate"].ToString(), Encoding.UTF8));
                            var bodyToFormat = body.ToString().Replace("[Nombre]", nombre);
                            bodyToFormat = bodyToFormat.Replace("[Ticket]", ticket.NumeroTicket);
                            bodyToFormat = bodyToFormat.Replace("[MontoBonificado]", "");

                            mail.SendMail(ticket.Email, subject, bodyToFormat, true);
#if !DEBUG
                            Logueo.Evento("Envio de correo electronico");
#endif
                        }
                    }
                    catch (Exception ex)
                    {
#if !DEBUG
                        Logueo.Evento("Proceso con error:" + ex.ToString());
#endif
                    }
                }
            }
            catch (Exception ex)
            {
#if !DEBUG
                Logueo.Error("Proceso con error:" + ex.ToString());
#endif
            }
            return true;
         
        }

        public void Iniciar()
        {
            
        }

      
    }
}
