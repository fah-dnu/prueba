#define Azure
using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_ParabiliaProcesoCortes.CapaNegocio;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes
{
    public class ProcesoCortes : IProcesoNocturno
    {
        
        bool IProcesoNocturno.Procesar()
        {//
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Inicio proceso corte debito]");
                LNInicioCorte _lnIniciocorte = new LNInicioCorte();
                return _lnIniciocorte.inicio();
            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Inicio proceso corte debito]");
                LNInicioCorte _lnIniciocorte = new LNInicioCorte();
                _lnIniciocorte.inicio();

            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
               
            }
        }

        void IProcesoNocturno.Detener()
        {
        }

        public bool InicioLocal(string fecha=null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try

            {
#if Azure
                string aplication = ConfigurationManager.AppSettings["applicationId"].ToString();
                string cliente = ConfigurationManager.AppSettings["clientKey"].ToString();
                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(aplication, cliente);
#endif
                ConfiguracionContexto.InicializarContexto();
                LNInicioCorte _lnIniciocorte = new LNInicioCorte();
                //
                return _lnIniciocorte.inicio(fecha);
            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return false;
            }
        }

        public bool GenerarEstadoDeCuenta(Int64 idCorte)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                LNInicioCorte _lnIniciocorte = new LNInicioCorte(idCorte.ToString());
                //
                return _lnIniciocorte.inicio();
            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return false;
            }
        }

        public RespuestaSolicitud GenerarEstadoDeCuentaDebito(Int64 idCorte)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            RespuestaSolicitud respuesta = new RespuestaSolicitud();
            try
            {
               //si envia el id de entrarda va aenviar el correo o generar el pdf sin hacer todo el proceso
                LNInicioCorte _lnIniciocorte = new LNInicioCorte(idCorte.ToString(), null, null, false, respuesta,true);
                //
                bool Procesado = _lnIniciocorte.inicio();
                return respuesta;
            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                 return respuesta;
            }
        }

        public RespuestaSolicitud EnviarEstadoDeCuenta(Int64 idCorte)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            RespuestaSolicitud respuesta = new RespuestaSolicitud();
            try
            {
                
                LNInicioCorte _lnIniciocorte = new LNInicioCorte(idCorte.ToString(),null,null,true, respuesta);
                //
                bool Procesado= _lnIniciocorte.inicio();
                return respuesta;
            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
               return respuesta;
            }
        }
    }
}
