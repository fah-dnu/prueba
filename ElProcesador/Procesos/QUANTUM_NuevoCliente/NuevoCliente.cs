using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using QUANTUM_NuevoCliente.LogicaNegocio;

namespace QUANTUM_NuevoCliente
{
    public class NuevoCliente : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {

            try
            {
                return LNCliente.ProcesaNuevosClientes();
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
