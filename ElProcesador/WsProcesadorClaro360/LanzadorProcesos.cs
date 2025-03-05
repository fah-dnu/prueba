using CommonProcesador;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DNU_ProcesadorClaro360;
namespace WsProcesadorClaro360
{
    public class LanzadorProcesos
    {
        static Thread unTimer;

        public bool EjecutarProceso()
        {

            IProcesoNocturno Proceso = null;
            Boolean Respuesta = false;

            var elProceso = new
            {
                Nombre = "Genérico"
            };

            try
            {
                //obtiene los mensajes de la Base de datos a procesar.
                Logueo.Evento("Se Inicia Proceso nocturno [" + elProceso.Nombre + "]");

                Proceso = new ProcesarClaro360();
                /*
                //valida que la accion no sea vacia y genera el objeto del ensamblado.
                if (elProceso.Nombre != null)
                {
                    // string accion = ProcesoNombre;

                    String _assemblyName = elProceso.Ensamblado;// Config.GetValor(ProcesoNombre, "Ensamblado");
                    String _className = elProceso.Clase;// Config.GetValor(ProcesoNombre, "Clase");

                    if ((!_assemblyName.Equals("")) && (!_className.Equals("")))
                    {
                        Logueo.Evento("Cargando PlugIns de los Procesos para ejecución");
                        //por reflexion genera una instancia del assembly asignado al comando.   
                        Proceso = (IProcesoNocturno)Assembly.Load(_assemblyName).CreateInstance(_className);
                    }
                    else
                    {
                        //Loguear el error "NO se reconoce comando recibido"+accion
                        throw new Exception("El Assembly y la Clase estan configuradas como vacias");
                    }
                }
                else
                {
                    //Loguear el error "Mensaje con Formato invalido"+msg
                    throw new Exception("Nombre del Proceso Invalido [" + elProceso.Nombre + "]");
                }

                if (Proceso == null)
                {
                    throw new Exception("No se pudo crear el Objeto para procesar el batch [" + elProceso.Nombre + "]");
                }
 */
                Logueo.Evento("Se construyó el objeto satisfactoriamente");

               
                //procesa el mensaje.
                try
                {
                    Respuesta = Proceso.Procesar();
                }
                catch (Exception err)
                {
                    Respuesta = false;
                }


                if (Respuesta)
                {
                    Logueo.Evento("Ejecución Exitosa, El Proceso " + elProceso.Nombre + " no reportó fallas en su ejecución");
                    //DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Procesado);
                }
                else
                {
                    Logueo.Evento("Ejecución con Errores, El Proceso " + elProceso.Nombre + " reportó fallas en su ejecución");
                    //DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Error);
                }

                return Respuesta;

                //asgina la respuesta al objeto para almacenarla en la base de datos.        

            }
            catch (Exception ex)
            {
                Logueo.Evento("Ocurrio un Error en la Ejecucion del Proceso " + elProceso.Nombre);
               // DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Error);
                Logueo.Error("Proceso:" + elProceso.Nombre + ": " + ex.ToString());

                return false;
            }

        }

        public bool InicarProceso()
        {

            IProcesoNocturno Proceso = null;
            Boolean Respuesta = false;

            var elProceso = new
            {
                Nombre = "Genérico"
            };

            try
            {
                //obtiene los mensajes de la Base de datos a procesar.


                Logueo.Evento("Se Inicia el proceso [" + elProceso.Nombre + "]");

                Proceso = new ProcesarClaro360();

                /*
                //valida que la accion no sea vacia y genera el objeto del ensamblado.
                if (elProceso.Nombre != null)
                {
                    // string accion = ProcesoNombre;

                    String _assemblyName = elProceso.Ensamblado;// Config.GetValor(ProcesoNombre, "Ensamblado");
                    String _className = elProceso.Clase;// Config.GetValor(ProcesoNombre, "Clase");

                    if ((!_assemblyName.Equals("")) && (!_className.Equals("")))
                    {
                        Logueo.Evento("Cargando PlugIns de los Procesos para ejecución");
                        //por reflexion genera una instancia del assembly asignado al comando.   
                        Proceso = (IProcesoNocturno)Assembly.Load(_assemblyName).CreateInstance(_className);
                    }
                    else
                    {
                        //Loguear el error "NO se reconoce comando recibido"+accion
                        throw new Exception("El Assembly y la Clase estan configuradas como vacias");
                    }
                }
                else
                {
                    //Loguear el error "Mensaje con Formato invalido"+msg
                    throw new Exception("Nombre del Proceso Invalido [" + elProceso.Nombre + "]");
                }

                if (Proceso == null)
                {
                    throw new Exception("No se pudo crear el Objeto para procesar el batch [" + elProceso.Nombre + "]");
                }
                */
                Logueo.Evento("Se construyó el objeto satisfactoriamente");


                //procesa el mensaje.
                try
                {
                    Proceso.Iniciar();

                    Thread.Sleep(Timeout.Infinite);

                    Respuesta = true;
                }
                catch (Exception err)
                {
                    Respuesta = false;
                }


                if (Respuesta)
                {
                    Logueo.Evento("Ejecución Exitosa, El Proceso " + elProceso.Nombre + " no reportó fallas en su ejecución");
                   // DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Procesado);
                }
                else
                {
                    Logueo.Evento("Ejecución con Errores, El Proceso " + elProceso.Nombre + " reportó fallas en su ejecución");
                   // DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Error);
                }

                return Respuesta;

                //asgina la respuesta al objeto para almacenarla en la base de datos.        

            }
            catch (Exception ex)
            {
                Logueo.Evento("Ocurrio un Error en la Ejecucion del Proceso " + elProceso.Nombre);
                //DAOProcesos.ActualizarEstatusEjecucion(elProceso, enumEstatusEjecucion.Error);
                Logueo.Error("Proceso:" + elProceso.Nombre + ": " + ex.ToString());

                return false;
            }
        }
    }
}
