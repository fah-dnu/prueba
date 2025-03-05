using CommonProcesador;
using DNU_ParabiliumProcesoMSI.CapaDeNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI
{
    public class ProcesoMSI : IProcesoNocturno
    {

        string directorioEntrada;
        string directorioSalida;

        bool IProcesoNocturno.Procesar()
        {//
            try
            {
                Logueo.Evento("[ProcesoMSI]Inicio proceso corte debito");
                LNMSI _lnIniciocorte = new LNMSI();
                return _lnIniciocorte.inicioProcesoMSI();

            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;

            }
        }

        void IProcesoNocturno.Iniciar()
        {
            try
            {
                Logueo.Evento("[ProcesoMSI]Inicio proceso diferimiento");
                LNMSI _lnIniciocorte = new LNMSI();
             //   _lnIniciocorte.inicioProcesoMSI();

            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);


            }


        }

        void IProcesoNocturno.Detener()
        {
        }

        public bool InicioLocal(string fecha = null)
        {
            try
            {
                LNMSI _lnIniciocorte = new LNMSI();
                //
                return _lnIniciocorte.inicioProcesoMSI();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }

    }
}
