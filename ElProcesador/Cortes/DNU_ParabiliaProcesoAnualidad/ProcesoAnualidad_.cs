using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonProcesador;
using Interfases;
using DNU_ParabiliaProcesoAnualidad.LogicaNegocio;

namespace DNU_ParabiliaProcesoAnualidad
{
    public class ProcesoAnualidad_ : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                Logueo.Evento("[GeneraGastosAnualidad] Inicio proceso anualidad");
                LNAnualidad _lnAnualidad = new LNAnualidad();
                return _lnAnualidad.inicio();
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
                Logueo.Evento("[GeneraGastosAnualidad] Inicio proceso anualidad");
                LNAnualidad _lnAnualidad = new LNAnualidad();
                _lnAnualidad.inicio();
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
                Logueo.Evento("[GeneraGastosAnualidad] Inicio proceso anualidad");
                LNAnualidad _lnAnualidad = new LNAnualidad();
                return _lnAnualidad.inicio(fecha);
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }
    }
}
