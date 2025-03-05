using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DALCentralAplicaciones.LogicaNegocio;
using FACTURACION_Sender.LogicaNegocio;

namespace FACTURACION_Sender
{
    class Sender : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {
            ValoresInicial.InicializarContexto();

            try
            {
                try{
                LNSender.EnviaSugerencias();
                }
                catch (Exception err) {
                    Logueo.Error( "Error al ejecutar EnviaSugerencias()" + err.ToString());
                }

                try{
                    LNSender.EnviaFacturasTimbradasXML();
                } catch (Exception err)
                {
                     Logueo.Error( "Error al ejecutar EnviaFacturasTimbradasXML()" + err.ToString());
                }
               
                return true;
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
