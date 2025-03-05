using CommonProcesador;
using MOSHI_CambioNivel.LogicaNegocio;
using System;

namespace MOSHI_CambioNivel
{
    public class CambiaNivel : IProcesoNocturno
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                return LNCambiaNivel.CambiaNivelCuentahabientes();
            }

            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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
