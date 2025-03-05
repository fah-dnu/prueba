#define Azure
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using ProcesadorNocturno;
using System.Reflection;
using ProcesadorNocturno.Entidades;
using ProcesadorNocturno.BaseDatos;
using System.Threading;
using log4net;
using System.Configuration;
using Dnu.AutorizadorParabiliaAzure.Services;
using Dnu.AutorizadorParabiliaAzure.Models;
using ProcesadorNocturno.LogicaDeNegocio;
using DNU.Monitoreo.Entidades;

namespace Procesador
{
    public class LanzaProcesos
    {
       
        static Thread unTimer;

        public bool EjecutarProceso(Proceso elProceso)
        {

            IProcesoNocturno Proceso = null;
            Boolean Respuesta = false;

            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            try
            {
                ///libreria monitoreo
                LNLibreriaMonitoreo _libreriaMonitoreo = new LNLibreriaMonitoreo();

                Logueo.Evento("[ProcesadorNocturo] " + "INICIA ConsultarAlertamientosAbiertosXClave()");
                RespuestaGenerica RespuestaConsultarAlertamientosAbiertosXClave = _libreriaMonitoreo.ConsultarAlertamientosAbiertosXClave();
                Logueo.Evento("[ProcesadorNocturo] " + "TERMINA ConsultarAlertamientosAbiertosXClave()");
                if (RespuestaConsultarAlertamientosAbiertosXClave.Codigo != 0)
                {
                    Logueo.EventoDebug("[ProcesadorNocturo] " + "[InicializarProcesosSensores] No se pudo ConsultarAlertamientosAbiertosXClave Codigo:" + RespuestaConsultarAlertamientosAbiertosXClave.Codigo + " Respuesta: " + RespuestaConsultarAlertamientosAbiertosXClave.Respuesta.ToString());
                    Logueo.Error("[ProcesadorNocturo] " + "[InicializarProcesosSensores] No se pudo ConsultarAlertamientosAbiertosXClave Codigo:" + RespuestaConsultarAlertamientosAbiertosXClave.Codigo + " Respuesta: " + RespuestaConsultarAlertamientosAbiertosXClave.Respuesta.ToString());
                    return false;
                }

                if (EntidadAlertamientos.TieneAlertamientos)
                {
                    Logueo.EventoDebug("[ProcesadorNocturo] " + "[InicializarProcesosSensores] No puede operar hasta que resuelva los Alertamientos");
                    Logueo.Error("[ProcesadorNocturo] " + "[InicializarProcesosSensores] No puede operar hasta que resuelva los Alertamientos");
                    return false;
                }
                /////
                Logueo.Evento("TERMINA libreriMonitoreo()");
                //Fin libreria Monitoreo


#if Azure
                string app = ConfigurationManager.AppSettings["applicationId"].ToString();
                string appPass = ConfigurationManager.AppSettings["clientKey"].ToString();

                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(app, appPass);


#endif

                //obtiene los mensajes de la Base de datos a procesar.

                AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Se Inicia Proceso nocturno [" + elProceso.Nombre + "]");
                
                //valida que la accion no sea vacia y genera el objeto del ensamblado.
                if (elProceso.Nombre != null)
                {
                    // string accion = ProcesoNombre;

                    String _assemblyName = elProceso.Ensamblado;// Config.GetValor(ProcesoNombre, "Ensamblado");
                    String _className = elProceso.Clase;// Config.GetValor(ProcesoNombre, "Clase");

                    if ((!_assemblyName.Equals("")) && (!_className.Equals("")))
                    {
                        AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Cargando PlugIns de los Procesos para ejecución]");
                       //por reflexion genera una instancia del assembly asignado al comando.   
                        Proceso = (IProcesoNocturno)Assembly.Load(_assemblyName).CreateInstance(_className);
                    }
                    else
                    {
                        AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [El Assembly y la Clase estan configuradas como vacias]");
                        throw new Exception("El Assembly y la Clase estan configuradas como vacias");
                    }
                }
                else
                {
                   AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Nombre del Proceso Invalido [" + elProceso.Nombre + "]");
                    throw new Exception("Nombre del Proceso Invalido [" + elProceso.Nombre + "]");
                }

                if (Proceso == null)
                {
                    AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [No se pudo crear el Objeto para procesar el batch [" + elProceso.Nombre + "]");
                    throw new Exception("No se pudo crear el Objeto para procesar el batch [" + elProceso.Nombre + "]");
                }

                AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Se construyó el objeto satisfactoriamente]");
                

                //procesa el mensaje.
                try
                {
                    Respuesta = Proceso.Procesar();
                }
                catch (Exception err)
                {
                    Respuesta = false;
                    AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [" + err.Message + "]");
                }


                if (Respuesta)
                {
                    AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Ejecución Exitosa, El Proceso " + elProceso.Nombre + " no reportó fallas en su ejecución]");
                    DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Procesado);
                }
                else
                {
                    AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Ejecución con Errores, El Proceso " + elProceso.Nombre + " reportó fallas en su ejecución]");
                     DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Error);
                }

                return Respuesta;

                //asgina la respuesta al objeto para almacenarla en la base de datos.        

            }
            catch (Exception ex)
            {
                AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Ocurrio un Error en la Ejecucion del Proceso " + elProceso.Nombre + "]");
                DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Error);
                AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Proceso:" + elProceso.Nombre + ": " + ex.ToString() + "]");
               
                return false;
            }

        }

        public bool InicarProceso(Proceso elProceso)
        {
            IProcesoNocturno Proceso = null;
            Boolean Respuesta = false;

            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;


            try
            {
                ///libreria monitoreo
                LNLibreriaMonitoreo _libreriaMonitoreo = new LNLibreriaMonitoreo();

                Logueo.Evento("[ProcesadorNocturo] " + "INICIA ConsultarAlertamientosAbiertosXClave()");
                RespuestaGenerica RespuestaConsultarAlertamientosAbiertosXClave = _libreriaMonitoreo.ConsultarAlertamientosAbiertosXClave();
                Logueo.Evento("[ProcesadorNocturo] " + "TERMINA ConsultarAlertamientosAbiertosXClave()");
                if (RespuestaConsultarAlertamientosAbiertosXClave.Codigo != 0)
                {
                    Logueo.EventoDebug("[ProcesadorNocturo] " + "[InicializarProcesosSensores] No se pudo ConsultarAlertamientosAbiertosXClave Codigo:" + RespuestaConsultarAlertamientosAbiertosXClave.Codigo + " Respuesta: " + RespuestaConsultarAlertamientosAbiertosXClave.Respuesta.ToString());
                    Logueo.Error("[ProcesadorNocturo] " + "[InicializarProcesosSensores] No se pudo ConsultarAlertamientosAbiertosXClave Codigo:" + RespuestaConsultarAlertamientosAbiertosXClave.Codigo + " Respuesta: " + RespuestaConsultarAlertamientosAbiertosXClave.Respuesta.ToString());
                    return false;
                }

                if (EntidadAlertamientos.TieneAlertamientos)
                {
                    Logueo.EventoDebug("[ProcesadorNocturo] " + "[InicializarProcesosSensores] No puede operar hasta que resuelva los Alertamientos");
                    Logueo.Error("[ProcesadorNocturo] " + "[InicializarProcesosSensores] No puede operar hasta que resuelva los Alertamientos");
                    return false;
                }
                /////
                Logueo.Evento("TERMINA libreriMonitoreo()");
                //Fin libreria Monitoreo

#if Azure
                string app = ConfigurationManager.AppSettings["applicationId"].ToString();
                string appPass = ConfigurationManager.AppSettings["clientKey"].ToString();

                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(app, appPass);

               
#endif
                //obtiene los mensajes de la Base de datos a procesar.



                AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Se Inicia el proceso [" + elProceso.Nombre + "]");
                
                //valida que la accion no sea vacia y genera el objeto del ensamblado.
                if (elProceso.Nombre != null)
                {
                    // string accion = ProcesoNombre;

                    String _assemblyName = elProceso.Ensamblado;// Config.GetValor(ProcesoNombre, "Ensamblado");
                    String _className = elProceso.Clase;// Config.GetValor(ProcesoNombre, "Clase");

                    if ((!_assemblyName.Equals("")) && (!_className.Equals("")))
                    {
                        AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Cargando PlugIns de los Procesos para ejecución]");
                         //por reflexion genera una instancia del assembly asignado al comando.   
                        Proceso = (IProcesoNocturno)Assembly.Load(_assemblyName).CreateInstance(_className);
                    }
                    else
                    {
                        AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [El Assembly y la Clase estan configuradas como vacias]");
                        throw new Exception("El Assembly y la Clase estan configuradas como vacias");
                    }
                }
                else
                {
                      AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Nombre del Proceso Invalido [" + elProceso.Nombre + "]");
                    throw new Exception("Nombre del Proceso Invalido [" + elProceso.Nombre + "]");
                }

                if (Proceso == null)
                {
                    AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [No se pudo crear el Objeto para procesar el batch [" + elProceso.Nombre + "]");
                    throw new Exception("No se pudo crear el Objeto para procesar el batch [" + elProceso.Nombre + "]");
                }

                AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Se construyó el objeto satisfactoriamente]");
               

                //procesa el mensaje.
                try
                {
                    Proceso.Iniciar();

                    Thread.Sleep(Timeout.Infinite);
                }
                catch (Exception err)
                {
                    Respuesta = false;
                    AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [" + err.Message + "]");
                }


                if (Respuesta)
                {
                    AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Ejecución Exitosa, El Proceso " + elProceso.Nombre + " no reportó fallas en su ejecución]");
                    DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Procesado);
                }
                else
                {
                    AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Ejecución con Errores, El Proceso " + elProceso.Nombre + " reportó fallas en su ejecución]");
                    DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Error);
                }

                return Respuesta;

                //asgina la respuesta al objeto para almacenarla en la base de datos.        

            }
            catch (Exception ex)
            {
                AppLogger.Info("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Ocurrio un Error en la Ejecucion del Proceso " + elProceso.Nombre + "]");
                DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Error);
                AppLogger.Error("[" + direccionIP + "] [Procesador] [PROCESADORNOCTURNO] [" + idLog + "] [Proceso:" + elProceso.Nombre + ": " + ex.ToString() + "]");

                return false;
            }
        }
    }
}
