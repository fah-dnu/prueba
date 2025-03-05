using CommonProcesador;
using DNU_ParabiliaBloqDesbloqueoTDC.CapaDatos;
using DNU_ParabiliaBloqDesbloqueoTDC.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfases.Entidades;

namespace DNU_ParabiliaBloqDesbloqueoTDC.LogicaNegocio
{
    public class LNBloqueoDesbloqueo
    {
        private string cadenaConexion;
        private DAOBloqueoDesbloqueo DAOBloq;
        private string usuarioServicio;

        public LNBloqueoDesbloqueo()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                
                cadenaConexion = PNConfig.Get("PROCESABLOQDESBLOQ", "BDWriteAutorizador");
                usuarioServicio = PNConfig.Get("PROCESABLOQDESBLOQ", "UsuarioBloqueo");
            }
            catch (Exception es)
            {
                Logueo.Error("[GeneraBloqueoDesbloqueo] [Error al procesar el bloqueo o desbloqueo] [Mensaje: " + es.Message + " TRACE: " + es.StackTrace + "]");
            }
        }

        public bool inicio(string fecha = null)
        {
            DAOBloq = new DAOBloqueoDesbloqueo();

            string cultura = CultureInfo.CurrentCulture.Name;
            Logueo.Evento("[GeneraBloqueoDesbloqueo] culture " + cultura);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            Logueo.Evento("[GeneraBloqueoDesbloqueo] cultureMX " + cultura);

            try
            {
                Logueo.Evento("[GeneraBloqueoDesbloqueo] Inicia proceso de bloqueo de cuentas");
                List<Resultado> resultadoBloqueo = DAOBloq.BloqueoTarjetasTDC(usuarioServicio, cadenaConexion);

                foreach (Resultado resultado in resultadoBloqueo)
                {
                    if (resultado.Tipo == "correcto")
                    {
                        Logueo.Evento("[GeneraBloqueoDesbloqueo] " + resultado.Mensaje + ": " + resultado.Bitacora);
                    }
                    else
                    {
                        Logueo.Error("[GeneraBloqueoDesbloqueo] " + resultado.Mensaje + ": " + resultado.Bitacora);
                    }
                }

                Logueo.Evento("[GeneraBloqueoDesbloqueo] Inicia proceso de desbloqueo de cuentas");
                List<Resultado> resultadoDesbloqueo = DAOBloq.DesbloqueoTarjetasTDC(usuarioServicio, cadenaConexion);
                foreach (Resultado resultado in resultadoDesbloqueo)
                {
                    if (resultado.Tipo == "correcto")
                    {
                        Logueo.Evento("[GeneraBloqueoDesbloqueo] " + resultado.Mensaje + ": " + resultado.Bitacora);
                    }
                    else
                    {
                        Logueo.Error("[GeneraBloqueoDesbloqueo] " + resultado.Mensaje + ": " + resultado.Bitacora);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al iniciar coneccion:" + ex.Message + " " + ex.StackTrace);
            }

            return true;
        }


    }
}
