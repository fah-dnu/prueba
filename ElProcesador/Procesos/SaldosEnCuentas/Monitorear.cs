using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using CommonProcesador;
using MonitoreaCuentas.LogicaNegocio;
using System.IO;
using CommonProcesador.Utilidades;


namespace MonitoreaCuentas
{
    public class Monitorear : IProcesoNocturno
    {


        bool IProcesoNocturno.Procesar()
        {

            try
            {
                int resp;

                resp = LNMonitor.EnviaEmailCuentasSaldoBajo();

                if (resp == 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
           
            
        }

        void IProcesoNocturno.Iniciar()
        {
        }

        void IProcesoNocturno.Detener()
        {
        }


       
    }
}
