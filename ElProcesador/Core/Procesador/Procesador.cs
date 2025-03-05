using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Threading;
using ProcesadorNocturno;
using CommonProcesador;
using ProcesadorNocturno.Entidades;
using ProcesadorNocturno.BaseDatos;
using System.IO;
using CommonProcesador.Utilidades;
using ProcesadorNocturno.LogicaDeNegocio;
using DNU.Monitoreo.Entidades;

namespace Procesador
{


    public partial class Procesador : ServiceBase
    {

        private System.Timers.Timer tmrEjecutor = new System.Timers.Timer();

        //delegate void EjecutarProceso(String ClaveProceso);
        //delegate void ProcesaBatch(String Accion);
        delegate bool EjecutarProceso(Proceso elProceso);
        //Alacenamos los mensajes leidos en la Base de datos.
        static Object thisLockSend = new Object();
        public static Dictionary<String, IAsyncResult> ThreadsControl = new Dictionary<string, IAsyncResult>();
        public static Dictionary<String, IAsyncResult> ThreadsControlServicios = new Dictionary<string, IAsyncResult>();

        static void Main()
        {

            try
            {

                bool EstaHabilitadaMonitoreoTabla = false;
                bool EstaHabilitadaMonitoreoArchivo = false;
                //Libreria Monitoreo
                LNLibreriaMonitoreo _libreriaMonitoreo = new LNLibreriaMonitoreo();
                LNLibreriaMonitoreo.CargarValoresFijos();
                //Validar Firma de Parametros
                RespuestaGenerica RespuestaEvaluarFirmaParametros = _libreriaMonitoreo.EvaluarFirmaParametros();
                if (RespuestaEvaluarFirmaParametros.Codigo != 0)
                {
                    Logueo.Error("[ProcesadorNocturo] No se pudo evaluar la firma del archivo de configuraciones, favor de volver a firmar el archivo con el configurador");
                    Logueo.Error("[ProcesadorNocturo] " + RespuestaEvaluarFirmaParametros.Respuesta.ToString());
                    Logueo.Evento("[ProcesadorNocturo] No se pudo Evaluar la firma del archivo de configuraciones, favor de volver a firmar el archivo con el configurador");
                    Logueo.Evento("[ProcesadorNocturo] " + RespuestaEvaluarFirmaParametros.Respuesta.ToString());
                    Logueo.EntradaSalida("[ProcesadorNocturo] No se pudo Evaluar la firma del archivo de configuraciones, favor de volver a firmar el archivo con el configurador", "", true);
                    Logueo.EntradaSalida("[ProcesadorNocturo] " + RespuestaEvaluarFirmaParametros.Respuesta.ToString(), "", true);
                    Logueo.EventoDebug("[ProcesadorNocturo] No se pudo Evaluar la firma del archivo de configuraciones, favor de volver a firmar el archivo con el configurador");
                    Logueo.EventoDebug("[ProcesadorNocturo] " + RespuestaEvaluarFirmaParametros.Respuesta.ToString());
                    //detener servicio
                    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
                    //FallarEnProduccion();
                    throw new Exception("No se pudo Evaluar la firma del archivo de configuraciones, favor de volver a firmar el archivo con el configurador");
                    return;
                }
                else
                {
                    Logueo.Evento("[ProcesadorNocturo] " + RespuestaEvaluarFirmaParametros.Respuesta.ToString());
                }

                ////////////Monitoreo Tablas            


                RespuestaGenerica RespuestaVerificarFirmasAplicativos = _libreriaMonitoreo.VerificarFirmasAplicativos();
                if (RespuestaVerificarFirmasAplicativos.Codigo != 0)
                {
                    Logueo.Error("[ProcesadorNocturo] No se pudo Iniciar la libreria Monitoreo firmas");
                    Logueo.Error("[ProcesadorNocturo] " + RespuestaVerificarFirmasAplicativos.Respuesta.ToString());
                    Logueo.Evento("[ProcesadorNocturo] No se pudo Iniciar la libreria Monitoreo firmas");
                    Logueo.Evento("[ProcesadorNocturo] " + RespuestaVerificarFirmasAplicativos.Respuesta.ToString());
                    //detener servicio
                    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
                    //FallarEnProduccion();
                    throw new Exception("No se pudo Iniciar la libreria Monitoreo firmas");
                    return;
                }
                else
                {
                    Logueo.Evento("[ProcesadorNocturo] " + (RespuestaVerificarFirmasAplicativos.Respuesta.ToString()));
                }

                RespuestaGenerica RespuestaEncenderMonitoreoTabla = _libreriaMonitoreo.EncenderMonitoreoTabla();
                if (RespuestaEncenderMonitoreoTabla.Codigo != 0)
                {
                    Logueo.Error("[ProcesadorNocturo] No se pudo Iniciar la libreria Monitoreo Tabla");
                    Logueo.Error("[ProcesadorNocturo] " + RespuestaEncenderMonitoreoTabla.Respuesta.ToString());
                    Logueo.Evento("[ProcesadorNocturo] No se pudo Iniciar la libreria Monitoreo Tabla");
                    Logueo.Evento("[ProcesadorNocturo] " + RespuestaEncenderMonitoreoTabla.Respuesta.ToString());
                    //LibreriaMonitoreo.EstaHabilitadaMonitoreoTabla = false;
                    EstaHabilitadaMonitoreoTabla = false;
                }
                else
                {
                    Logueo.Evento("[ProcesadorNocturo] " + RespuestaEncenderMonitoreoTabla.Respuesta.ToString());
                    //LibreriaMonitoreo.EstaHabilitadaMonitoreoTabla = true;
                    EstaHabilitadaMonitoreoTabla = true;
                }
                //////////////Monitoreo Archivos            
                RespuestaGenerica RespuestaEncenderMonitoreoArchivos = _libreriaMonitoreo.EncenderMonitoreoArchivo();
                if (RespuestaEncenderMonitoreoArchivos.Codigo != 0)
                {
                    Logueo.Error("[ProcesadorNocturo] No se pudo Iniciar la libreria Monitoreo Archivo");
                    Logueo.Error("[ProcesadorNocturo] " + RespuestaEncenderMonitoreoArchivos.Respuesta.ToString());
                    Logueo.Evento("[ProcesadorNocturo] No se pudo Iniciar la libreria Monitoreo Archivo");
                    Logueo.Evento("[ProcesadorNocturo] " + RespuestaEncenderMonitoreoArchivos.Respuesta.ToString());
                    //LibreriaMonitoreo.EstaHabilitadaMonitoreoArchivo = false;
                    EstaHabilitadaMonitoreoArchivo = false;
                }
                else
                {
                    Logueo.Evento("[ProcesadorNocturo] " + RespuestaEncenderMonitoreoArchivos.Respuesta.ToString());
                    //LibreriaMonitoreo.EstaHabilitadaMonitoreoArchivo = true;
                    EstaHabilitadaMonitoreoArchivo = true;
                }
                ///////////////////////
                ///

                if (EstaHabilitadaMonitoreoArchivo = false)
                {
                    //detener servicio
                    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
                    throw new Exception("Debe iniciar la libreria monitoreo");
                    return;
                }

                //DateTime mondayOfLastWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 6);

                //LNEmail.Send("jluisdtm@gmail.com", "info@dnu.mx", "El Procesador nocturno ha sido Reiniciado", "Se Reinicio el Procesador Norcturno");
                //#if (!DEBUG)

                //server para instalarlo
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                    {
                        new Procesador()
                    };
                ServiceBase.Run(ServicesToRun);

                //#else

                //  DateTime elFecha = DateTime.Parse("2015-05-19T17:33:18.167-05:00");

                //local debug
                //Procesador service1 = new Procesador();
                //String[] args = new String[] { String.Empty };
                //service1.OnStart(args);
                //System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);


            }
            catch (Exception err)
            {
                Logueo.Error("[ProcesadorNocturo] " + "Inicializando e Servicio" + err.Message + ":" + err.Source + ": " + err.InnerException);
            }


            ////#endif


        }

        public Procesador()
        {
          

            try
            {


                InitializeComponent();

            }
            catch (Exception Err)
            {
                Logueo.Error("[Procesador Nocturno] " + "Ocurrio un Error al Inicialir el Servicio " + Err.ToString());

                throw Err;
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {

               

                //PROCESO NORMAL
                Logueo.Evento("[ProcesadorNocturo] " + "Iniciando Consultor de Procesos a Ejecutar");
                this.tmrEjecutor.Enabled = true;
                this.tmrEjecutor.Interval = 8000;//Double.Parse(ConfigurationManager.AppSettings["PeriodoEjecucionLectura"]);
                this.tmrEjecutor.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_ElapsedProcesaBatch);
                Logueo.Evento("[ProcesadorNocturo] " + "Termina la inicializacion");

                Logueo.Evento("[ProcesadorNocturo] " + "Iniciando Lectura de Configuraciones de Procesos");
                ConfiguracionContexto.InicializarContexto();
                Logueo.Evento("[ProcesadorNocturo] " + "Lectura de Configuraciones de Procesos Exitosa");

                Logueo.Evento("[ProcesadorNocturo] " + "Iniciando Lectura de Configuraciones de Cubos");
              //  Framework.ContextoInicial.InicializarContexto();
                Logueo.Evento("[ProcesadorNocturo] " + "Lectura de Configuraciones de Cubos Exitosa");


                Logueo.Evento("[ProcesadorNocturo] " + "Iniciando Procesos del tipo servicio");
                InicializarProcesosSensores();
                Logueo.Evento("[ProcesadorNocturo] " + "Terminada el inicio de procesos de tipo Servicio");
              
                
            }
            catch (Exception err)
            {
                Logueo.Error("[Procesador Nocturno] " + "Ocurrio un Error al Inicialir el Servicio " + err.ToString());
            }


        }

        private void InicializarProcesosSensores()
        {
            try
            {
                String Proceso = "";
                List<Proceso> losProcesos = new List<Proceso>();
                losProcesos = DAOProcesos.ObtieneTodosProcesos();

                for (int k = 0; k < losProcesos.Count; k++)
                {
                    Proceso = losProcesos[k].Clave;// Config.GetValor("PlugIns", String.Format("{0}", k));
                    Logueo.Evento("[ProcesadorNocturo] INICIA EL SENSOR: " + Proceso);
                    try
                    {
                        LanzaProcesos elProceso = new LanzaProcesos();
                        //Inicia los delegados para Ejecutar los batch de las diferentes acciones.
                        EjecutarProceso ProcesoAEjecutar = elProceso.InicarProceso;

                        //Valida que no se este ejecutando el bacth de la accion en el momento que se ejecuto otro timmer.
                        if (ThreadsControlServicios.ContainsKey(Proceso))
                        {
                            //si existe valida que no este ejecutandose para no duplicar procesos de mensajes.
                            if (!ThreadsControlServicios[Proceso].IsCompleted)
                            {
                                //levanta la excepcion para cancelar el inicio del nuevo procesamiento
                                Logueo.Evento(" " + "Se continúa ejecutando un proceso con el mismo nombre... [" + Proceso + "] se cancela la ejecucion actual");

                                continue;
                            }

                        }

                        //Si llego aki quiere decir que no esta ejecutandose el hilo y que ya puede iniciar otro proceos batch ocn el mismo nombre
                        //si contine un elemlento en el Dictionari lo elimina para poner otro.
                        if (ThreadsControlServicios.ContainsKey(Proceso))
                        {
                            ThreadsControlServicios.Remove(Proceso);
                        }                        

                        //Ejecuta el metodo asincrono
                        ThreadsControlServicios.Add(Proceso,
                                    ProcesoAEjecutar.BeginInvoke(losProcesos[k],
                                    delegate(IAsyncResult ar1)
                                    {
                                        try
                                        {
                                            ProcesoAEjecutar.EndInvoke(ar1);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, null));




                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[TimmerBatch] " + "Procesador Nocturno.ProcesaBatchTimmer" + ex.ToString());
                    }

                }
            }
            catch (Exception err)
            {
                Logueo.Error("[TimmerBatch] " + "Procesador Nocturno.ProcesaBatchTimmer" + err.ToString());
            }
        }

        protected override void OnStop()
        {
            LNLibreriaMonitoreo _libreriaMonitoreo = new LNLibreriaMonitoreo();
            RespuestaGenerica RespuestaInsertarAlertamientoReinicio = _libreriaMonitoreo.InsertarAlertamientoReinicio();
            if (RespuestaInsertarAlertamientoReinicio.Codigo != 0)
            {
                Logueo.Error("No se pudo InsertarAlertamientoReinicio");
                Logueo.Error(RespuestaInsertarAlertamientoReinicio.Respuesta.ToString());
                Logueo.Evento("No se pudo InsertarAlertamientoReinicio");
                Logueo.Evento(RespuestaInsertarAlertamientoReinicio.Respuesta.ToString());
            }
            else
            {
                Logueo.Evento("Se inserto alerta por que se apagao el servicio");
                Logueo.Evento(RespuestaInsertarAlertamientoReinicio.Respuesta.ToString());
            }
        }

        private void timer_ElapsedProcesaBatch(object sender, System.Timers.ElapsedEventArgs e)
        {

            String Proceso = "";
            List<Proceso> losProcesos = new List<Proceso>();
            losProcesos = DAOProcesos.ObtieneProcesosAEjecutar();

            for (int k = 0; k < losProcesos.Count; k++)
            {
                Proceso = losProcesos[k].Clave;// Config.GetValor("PlugIns", String.Format("{0}", k));

                try
                {
                    LanzaProcesos elProceso = new LanzaProcesos();
                    //Inicia los delegados para Ejecutar los batch de las diferentes acciones.
                    EjecutarProceso ProcesoAEjecutar = elProceso.EjecutarProceso;

                    //Valida que no se este ejecutando el bacth de la accion en el momento que se ejecuto otro timmer.
                    if (ThreadsControl.ContainsKey(Proceso))
                    {
                        //si existe valida que no este ejecutandose para no duplicar procesos de mensajes.
                        if (!ThreadsControl[Proceso].IsCompleted)
                        {
                            //levanta la excepcion para cancelar el inicio del nuevo procesamiento
                            Logueo.Evento(" " + "Se continúa ejecutando un proceso con el mismo nombre... [" + Proceso + "] se cancela la ejecucion actual");

                            continue;
                        }

                    }

                    //Si llego aki quiere decir que no esta ejecutandose el hilo y que ya puede iniciar otro proceos batch ocn el mismo nombre
                    //si contine un elemlento en el Dictionari lo elimina para poner otro.
                    if (ThreadsControl.ContainsKey(Proceso))
                    {
                        ThreadsControl.Remove(Proceso);
                    }

                    //Ejecuta el metodo asincrono
                    ThreadsControl.Add(Proceso,
                                ProcesoAEjecutar.BeginInvoke(losProcesos[k],
                                delegate(IAsyncResult ar1)
                                {
                                    try
                                    {
                                        ProcesoAEjecutar.EndInvoke(ar1);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }, null));




                }
                catch (Exception ex)
                {
                    Logueo.Error("[TimmerBatch] [" + ConfigurationManager.AppSettings["Instancia"].ToString() + "] Procesador Nocturno.ProcesaBatchTimmer" + ex.ToString());
                }

            }


        }


    }
}
