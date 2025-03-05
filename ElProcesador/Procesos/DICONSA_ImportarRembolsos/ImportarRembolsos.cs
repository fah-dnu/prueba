using CommonProcesador;
using DICONSA_ImportarRembolsos.LogicaNegocio;
using System;

namespace DICONSA_ImportarRembolsos
{
    public class ImportarRembolsos : IProcesoNocturno
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                return LNImportarRembolsos.ImportaRembolsos();
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
