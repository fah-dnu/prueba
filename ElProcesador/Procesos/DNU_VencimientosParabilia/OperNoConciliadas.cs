using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNU_VencimientoCompensacion.LogicaNegocio;

namespace DNU_VencimientoCompensacion
{
    public class OperNoConciliadas : IProcesoNocturno
    {
        public string conn;

        bool IProcesoNocturno.Procesar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                conn = PNConfig.Get("VENCIMIENTO", "DataConnectionString");
                LNOperNoConciliadas _OperNoConciliadas = new LNOperNoConciliadas(conn);
                
                return _OperNoConciliadas.validarOperaciones();
            }
            catch (Exception ex)
            {
                Logueo.Error("[Procesar] [Error al Iniciar Proceso Vencimiento] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {
        }

        void IProcesoNocturno.Detener()
        {
        }

        //METODO TEST
        public void Iniciar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                conn = PNConfig.Get("VENCIMIENTO", "DataConnectionString");
                LNOperNoConciliadas _OperNoConciliadas = new LNOperNoConciliadas(conn);

                _OperNoConciliadas.validarOperaciones();
            }
            catch (Exception ex)
            {
                Logueo.Error("[Procesar] [Error al Iniciar Proceso Vencimiento] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

    }
}
