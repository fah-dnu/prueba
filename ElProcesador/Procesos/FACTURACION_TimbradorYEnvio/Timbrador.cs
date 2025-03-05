using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;

namespace FACTURACION_Timbrador
{
    class Timbrador : IProcesoNocturno
    {

        bool IProcesoNocturno.Procesar()
        {

            try
            {
                return LogicaNegocio.LNTimbrados.TimbrarFacturasCreadas();
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
