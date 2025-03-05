using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TELEVIP_ImportarTipoCuenta.LogicaNegocio;

namespace TELEVIP_ImportarTipoCuenta
{
   public class ActualizarTipoCuenta : IProcesoNocturno
    {

        bool IProcesoNocturno.Procesar()
        {

            try
            {
                //PRIMREO IMPORTA TODO LO DEL BD DE TELEVIA
                return LNTipoCuenta.ActualizaTipoCuentaPrepagoPostpago();


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
