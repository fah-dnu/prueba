using CommonProcesador;
using DNU_ConciliadorCacao.Entidades;
using DNU_ConciliadorCacao.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DNU_ConciliadorCacao.LogicaNegocio
{
    class LNMailing
    {
        public static Boolean EnviaResultadoConciliacion(string htmlMail, Archivo elArchivo)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                String losEmail = elArchivo.DistribucionEmail;


                //Emailing.Send(losEmail, "Conciliador@dnu.mx", htmlMail, "Conciliación Automatica DNU ");

                 LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Se envio el mail Conciliación Automatica DNU a los Correos: " + losEmail, "Procesador" + "]");
                return true;
            }
            catch (Exception err)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
            }

            return false;
        }

    }
}
