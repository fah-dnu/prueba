using CommonProcesador;
using ConciliacionSmartpoints.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.LogicaNegocio
{
    class LNMailing
    {
        public static Boolean EnviaResultadoConciliacion(string htmlMail, Archivo elArchivo)
        {
            try
            {

                String losEmail = elArchivo.DistribucionEmail;


                //Emailing.Send(losEmail, "Conciliador@dnu.mx", htmlMail, "Conciliación Automatica DNU ");

                Logueo.EntradaSalida("Se envio el mail [Conciliación Automatica DNU] a los Correos: " + losEmail, "Procesador", false);

                return true;
            }
            catch (Exception err)
            {
            }

            return false;
        }
    }
}
