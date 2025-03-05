using CommonProcesador;
using DNU_CompensacionT112_API_Cacao.LogicaNegocio;
using System;

namespace DNU_CompensacionT112_API_Cacao
{
    public class GestionaCompensacion : IProcesoNocturno
    {
        public static void Start()
        {
            LNThreadManager.IniciaProcesoHilos();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                LNThreadManager.IniciaProcesoHilos();
                return true;
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
                Start();
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
                LNThreadManager.DetieneHilos();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }
    }
}
