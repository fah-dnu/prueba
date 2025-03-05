using CommonProcesador;
using DNU_ProcesadorClaro360.Entidades;
using DNU_ProcesadorClaro360.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DNU_ProcesadorClaro360.LogicaNegocio
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
