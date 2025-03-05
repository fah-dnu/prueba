using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using QUANTUM_AplicarMovimientos.LogicaNegocio;
using QUANTUM_AplicarMovimientos.Entidades;

namespace QUANTUM_AplicarMovimientos
{
    public class AplicarMovimiento : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {

            try
            {

                return LNAplicarMovimientos.AplicarMovimiento();

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
