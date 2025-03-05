using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using QUANTUM_EnviarOperacionesCREAL.LogicaNegocio;

namespace QUANTUM_EnviarOperacionesCREAL
{
    class EnviarOperaciones : IProcesoNocturno
    {

        bool IProcesoNocturno.Procesar()
        {

            try
            {
                int resp;

                resp = -1;

                try
                {
                    LNOperaciones.EnviarOperacionesProcesadas();
                }

                catch (Exception ex)
                {
                     Logueo.Error(ex.Message);
                }


                if (resp == 0)
                {
                    return true;
                }
                else
                    return false;
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
              
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        void IProcesoNocturno.Detener()
        {

            try
            {
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }


    }
}
