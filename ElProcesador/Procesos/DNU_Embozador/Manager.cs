using CommonProcesador;
using DNU_Embozador.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DNU_Embozador
{
    public class Manager : IProcesoNocturno
    {



        public static void Start()
        {
            ConfiguracionContexto.InicializarContexto();
            LNEmbozamiento.Start();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                Start();
                return true;
            }

            catch (Exception err)
            {
                Log.Error(String.Format("[Procesar] {0}", err.Message));
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
                //Start();
            }
            catch (Exception err)
            {
                Log.Error(String.Format("[Iniciar] {0}", err.Message));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void IProcesoNocturno.Detener()
        {
            try
            {
                //Not action
            }
            catch (Exception err)
            {
                Log.Error(String.Format("[Detener] {0}", err.Message));
            }
        }
    }
}
