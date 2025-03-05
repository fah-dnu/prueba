#define Azure
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DNU_VencimientosParabilia.LogicaNegocio;
using System.IO;
using DNU_VencimientosParabilia.BaseDatos;
using DNU_VencimientosParabilia.Entidades;
using Executer.Entidades;
using System.Data.SqlClient;
using Interfases.Entidades;
using System.Threading.Tasks;
using log4net;
using DNU_VencimientosParabilia.Utilidades;
using Dnu.AutorizadorParabiliaAzure.Models;
using System.Configuration;
using Dnu.AutorizadorParabiliaAzure.Services;

namespace DNU_VencimientosParabilia
{
   public class ProcesarVencimientos : IProcesoNocturno
    {
        public void ProcesarloTest()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            try
            {
              
#if Azure
                string aplication = ConfigurationManager.AppSettings["applicationId"].ToString();
                string cliente = ConfigurationManager.AppSettings["clientKey"].ToString();
                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(aplication, cliente);
#endif
                ConfiguracionContexto.InicializarContexto();
               
                LNProcesaVencimeintos.ejecutaAplicacionDevoluciones();

                LNProcesaVencimeintos.ejecutaCambioEstatus();

                
            }
            catch (Exception err)
            {
                LogueoProcesaVencParabilia.Error("[" + direccionIP + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + idLog + "] [" + err.Message + "]");
            }
        }
        bool IProcesoNocturno.Procesar()
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            try
            {
                
                ConfiguracionContexto.InicializarContexto();


                LNProcesaVencimeintos.ejecutaAplicacionDevoluciones();


                LNProcesaVencimeintos.ejecutaCambioEstatus();


                return true;

            }
            catch (Exception err)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return false;
            }

        }

        void IProcesoNocturno.Iniciar()
        {
            string log = ""; //ThreadContext.Properties["log"].ToString();
            string ip = ""; //ThreadContext.Properties["ip"].ToString();
            try
            {

                ConfiguracionContexto.InicializarContexto();


                LNProcesaVencimeintos.ejecutaAplicacionDevoluciones();


                LNProcesaVencimeintos.ejecutaCambioEstatus();

            }
            catch (Exception err)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
            }
        }

        void IProcesoNocturno.Detener()
        {
            string log = "";//ThreadContext.Properties["log"].ToString();
            string ip = "";//ThreadContext.Properties["ip"].ToString();
            try
            {
            }
            catch (Exception err)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
            }
        }


    }
}
