using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using QUANTUM_ModificarLineasCredito.LogicaNegocio;

namespace QUANTUM_ModificarLineasCredito
{
    public class ModificarLineaCredito : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {
                try
                {

                    return LNModificarLC.ModificarLineas();

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
