using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfases;
using CommonProcesador;
using DNU_ParabiliaBloqDesbloqueoTDC.LogicaNegocio;

namespace DNU_ParabiliaBloqDesbloqueoTDC
{
    public class ProcesoBloqDesbloqueo : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                Logueo.Evento("[GeneraBloqueoDesbloqueo] Inicio proceso anualidad");
                LNBloqueoDesbloqueo lnBloqueoDesbloqueo = new LNBloqueoDesbloqueo();
                return lnBloqueoDesbloqueo.inicio();
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
                Logueo.Evento("[GeneraBloqueoDesbloqueo] Inicio proceso anualidad");
                LNBloqueoDesbloqueo lnBloqueoDesbloqueo = new LNBloqueoDesbloqueo();
                lnBloqueoDesbloqueo.inicio();
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
                Logueo.Evento("[GeneraBloqueoDesbloqueo] Inicio proceso bloqueo y desbloqueo de TDC");
                LNBloqueoDesbloqueo lnBloqueoDesbloqueo = new LNBloqueoDesbloqueo();
                return lnBloqueoDesbloqueo.inicio(fecha);
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }
    }
}
