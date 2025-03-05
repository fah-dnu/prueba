using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using QUANTUM_BloquearDesbloquearCliente.LogicaNegocio;

namespace QUANTUM_BloquearDesbloquearCliente
{
    class BloquearDesbloquear : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {

            try
            {


                LNCliente.Modificar();

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
