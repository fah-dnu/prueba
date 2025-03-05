using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonProcesador;
using DNU_ParabiliaProcesoCobranza.LogicaNegocio;
//using DNU_ParabiliaProcesoCortes.CapaNegocio;
//using DNU_ParabiliaProcesoCortes.Entidades;

namespace DNU_ParabiliaProcesoCobranza
{
    public class ProcesoCobranza : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {//
            try
            {
                Logueo.Evento("Inicio proceso gastos de cobranza");
                LNCobranza _lnCobranza = new LNCobranza();
                return _lnCobranza.inicio();
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
                Logueo.Evento("Inicio proceso gastos de cobranza");
                LNCobranza _lnCobranza = new LNCobranza();
                _lnCobranza.inicio();

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
               
                LNCobranza _lnCobranza = new LNCobranza();
                return _lnCobranza.inicio(fecha);
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }
    }
}
