using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_EnviaOperaciones.LogicaNegocio
{
    class DNU_Enviar : IProcesoNocturno
    {
        public void Detener()
        {
            //throw new NotImplementedException();
        }

        public void Iniciar()
        {
            //throw new NotImplementedException();
        }

        public bool Procesar()
        {

            try
            {
                return true;
            } catch (Exception err)
            {
                return false;
            }
            

        }
    }
}
