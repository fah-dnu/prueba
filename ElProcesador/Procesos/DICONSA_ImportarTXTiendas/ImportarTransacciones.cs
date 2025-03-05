using CommonProcesador;
using DICONSA_ImportarTXTiendas.LogicaNegocio;
using System;


namespace DICONSA_ImportarTXTiendas
{
    class ImportarTransacciones : IProcesoNocturno
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                return LNImportarTransacciones.ImportaOperaciones();
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
