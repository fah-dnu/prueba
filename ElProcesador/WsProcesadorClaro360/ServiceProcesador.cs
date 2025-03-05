using CommonProcesador;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WsProcesadorClaro360
{
    public partial class ServiceElProcesador : ServiceBase
    {
        public ServiceElProcesador()
        {
            InitializeComponent();
        }

        private System.Timers.Timer tmrEjecutor = new System.Timers.Timer();

        delegate bool EjecutarProceso();
        //Alacenamos los mensajes leidos en la Base de datos.
        static Object thisLockSend = new Object();
        public static Dictionary<String, IAsyncResult> ThreadsControl = new Dictionary<string, IAsyncResult>();
        public static Dictionary<String, IAsyncResult> ThreadsControlServicios = new Dictionary<string, IAsyncResult>();


        protected override void OnStart(string[] args)
        {
            try
            {
                Logueo.Evento("[ProcesadorNocturo] " + "Iniciando Consultor de Procesos a Ejecutar");
                this.tmrEjecutor.Enabled = true;
                this.tmrEjecutor.Interval = 5000;//Double.Parse(ConfigurationManager.AppSettings["PeriodoEjecucionLectura"]);
                this.tmrEjecutor.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_ElapsedProcesaBatch);
                Logueo.Evento("[ProcesadorNocturo] " + "Termina la inicializacion");

                //Logueo.Evento("[ProcesadorNocturo] " + "Iniciando Lectura de Configuraciones de Procesos");
                //ConfiguracionContexto.InicializarContexto();
                //Logueo.Evento("[ProcesadorNocturo] " + "Lectura de Configuraciones de Procesos Exitosa");

                //Logueo.Evento("[ProcesadorNocturo] " + "Iniciando Lectura de Configuraciones de Cubos");
                //Framework.ContextoInicial.InicializarContexto();
                //Logueo.Evento("[ProcesadorNocturo] " + "Lectura de Configuraciones de Cubos Exitosa");


                Logueo.Evento("[ProcesadorNocturo] " + "Iniciando Procesos del tipo servicio");
                InicializarProcesoGenerico();
                Logueo.Evento("[ProcesadorNocturo] " + "Terminada el inicio de procesos de tipo Servicio");
              
                
            }
            catch (Exception err)
            {
                Logueo.Error("[Procesador Nocturno] " + "Ocurrio un Error al Inicialir el Servicio " + err.ToString());
            }


        }

        private void InicializarProcesoGenerico()
        {
            try
            {
                String Proceso = "Genérico";
                Logueo.Evento("[ProcesadorNocturo] " + "Consulta para ejecutar Procesos");
                //List<Proceso> losProcesos = new List<Proceso>();
                //losProcesos = DAOProcesos.ObtieneTodosProcesos();

                //for (int k = 0; k < losProcesos.Count; k++)
                //{
                //    Proceso = losProcesos[k].Clave;// Config.GetValor("PlugIns", String.Format("{0}", k));

                try
                {
                    LanzadorProcesos lanzador = new LanzadorProcesos();
                    //Inicia los delegados para Ejecutar los batch de las diferentes acciones.
                    EjecutarProceso ProcesoAEjecutar = lanzador.InicarProceso;

                    //Valida que no se este ejecutando el bacth de la accion en el momento que se ejecuto otro timmer.
                    if (ThreadsControlServicios.ContainsKey(Proceso))
                    {
                        //si existe valida que no este ejecutandose para no duplicar procesos de mensajes.
                        if (!ThreadsControlServicios[Proceso].IsCompleted)
                        {
                            //levanta la excepcion para cancelar el inicio del nuevo procesamiento
                            Logueo.Evento(" " + "Se continúa ejecutando un proceso con el mismo nombre... [" + Proceso + "] se cancela la ejecucion actual");

                            return;
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
                                ProcesoAEjecutar.BeginInvoke(
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

                //}
            }
            catch (Exception err)
            {
            }
        }

        protected override void OnStop()
        {

        }

        private void timer_ElapsedProcesaBatch(object sender, System.Timers.ElapsedEventArgs e)
        {

            String Proceso = "";
            Logueo.Evento("[ProcesadorNocturo] " + "Consulta para ejecutar Procesos");
            //List<Proceso> losProcesos = new List<Proceso>();
            //losProcesos = DAOProcesos.ObtieneProcesosAEjecutar();

            //for (int k = 0; k < losProcesos.Count; k++)
            //{
                Proceso = "Genérico";//losProcesos[k].Clave;// Config.GetValor("PlugIns", String.Format("{0}", k));

                try
                {
                    LanzadorProcesos lanzador = new LanzadorProcesos();
                    //Inicia los delegados para Ejecutar los batch de las diferentes acciones.
                    EjecutarProceso ProcesoAEjecutar = lanzador.EjecutarProceso;

                    //Valida que no se este ejecutando el bacth de la accion en el momento que se ejecuto otro timmer.
                    if (ThreadsControl.ContainsKey(Proceso))
                    {
                        //si existe valida que no este ejecutandose para no duplicar procesos de mensajes.
                        if (!ThreadsControl[Proceso].IsCompleted)
                        {
                            //levanta la excepcion para cancelar el inicio del nuevo procesamiento
                            Logueo.Evento(" " + "Se continúa ejecutando un proceso con el mismo nombre... [" + Proceso + "] se cancela la ejecucion actual");

                            return;
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
                                ProcesoAEjecutar.BeginInvoke(
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

            //}


        }
    }
}
