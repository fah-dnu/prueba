using CommonProcesador;
using System;
using DNU_Limpieza.LogicaNegocio;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using DNU_Limpieza.Utilidades;

namespace DNU_Limpieza
{
    public class Limpiar : IProcesoNocturno
    {
        void IProcesoNocturno.Detener()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            try
            {
                //LNUsuario.EscucharDirectorio();
            }
            catch (Exception err)
            {
                LogueoLimpieza.Error("[" + direccionIP + "] [Limpieza] [PROCESADORNOCTURNO] [" + idLog + "] [" + err.Message + "]");
            }
        }

        void IProcesoNocturno.Iniciar()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                //LNArchivo.EscucharDirectorio();
            }
            catch (Exception err)
            {
                LogueoLimpieza.Error("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
            }
        }

        //bool IProcesoNocturno.Procesar()
        public bool Procesar()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                try
                {
                    
                    ConfiguracionContexto.InicializarContexto();

                    LogueoLimpieza.Info("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [Inicia metodo ProcesarArchivosPapelera]");
                    LNArchivo.ProcesarArchivosPapelera();
                    LogueoLimpieza.Info("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [Termina metodo ProcesarArchivosPapelera]");
                    return true;
                }

                catch (Exception ex)
                {
                    LogueoLimpieza.Error("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogueoLimpieza.Error("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                return false;
            }
        }
        
    }
}
