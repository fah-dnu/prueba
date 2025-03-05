using CommonProcesador;
using SMARTTKT_GeneradorPedidos.LogicaNegocio;
using System;

namespace SMARTTKT_GeneradorPedidos
{
    public class GeneraPedidos : IProcesoNocturno
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                return LNGeneraPedidos.GeneraPedidosFaltantes();
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
