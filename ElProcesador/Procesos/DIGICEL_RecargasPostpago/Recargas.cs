using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DALCentralAplicaciones.LogicaNegocio;

namespace DIGICEL_RecargasPostpago
{
    class Recargas : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {
            ValoresInicial.InicializarContexto();

            try
            {
                try
                {
                    return LNRecargas.EnviarRegargas();
                }
                catch (Exception err)
                {
                    Logueo.Error("Error al ejecutar Recargas Operaciones PostPago()" + err.ToString());
                    return false;
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
