using CommonProcesador;
using MonitoreoCacaoPN.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MonitoreoCacaoPN
{
    public class MonitoreoMain : IProcesoNocturno
    {
        Timer timer = new Timer(Convert.ToDouble(PNConfig.Get("MONCACAO", "IntervaloTimer")));
        static int validaEjecucionServicio = 0;
        LNOperaciones operaciones;
        
        public void Detener()
        {
        }

        public void Iniciar()
        {
            operaciones = new LNOperaciones();
            timer.Elapsed += new ElapsedEventHandler(this.enviarPeticion);
            timer.Start();
        }

        public bool Procesar()
        {
            return true;
        }

        private void enviarPeticion(object sender, ElapsedEventArgs e)
        {
            timer.Stop();

            try
            {

                Logueo.Evento("Ejecucion enviarPeticion .... ");
                ConfiguracionContexto.InicializarContexto();

                if (validaEjecucionServicio == 0)
                {
                    validaEjecucionServicio = 1;
                    operaciones.iniciarProcesoTarjetaDebito();
                }
                else if (validaEjecucionServicio == 1)
                {
                    validaEjecucionServicio = 2;
                    operaciones.iniciarProcesoTestingMethod();
                }
                else if (validaEjecucionServicio == 2)
                {
                    validaEjecucionServicio = 0;
                    operaciones.iniciarProcesoLineaCredito();
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al Iniciar Proceso MonitoreoCacao] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }

            timer.Start();
        }
    }
}
