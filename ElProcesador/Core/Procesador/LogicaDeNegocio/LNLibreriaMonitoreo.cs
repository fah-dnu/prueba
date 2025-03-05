using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU.Monitoreo.Entidades;
using DNU.Monitoreo.LogicaNegocio;
using Newtonsoft.Json;
using ProcesadorNocturno.Entidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProcesadorNocturno.LogicaDeNegocio
{
    public class LNLibreriaMonitoreo
    {
        private static string TablaAspnet = "MonitoreoTablaaspnet_Applications";
        private static string TablaConfiguraciones = "MonitoreoTablaConfiguraciones";
        private static string TablaContrato = "MonitoreoTablaContratoValoresFijos";
        private static string TablaValor = "MonitoreoTablaValorParametroMultiasignacion";
        private static string TablaEmpleados = "MonitoreoTablatb_Empleados";
        private static string MonitoreoCorreo = "MonitoreoCorreo";
        private static string MonitoreoArchivos = "MonitoreoArchivos";
        private static string MonitoreoAlertamientos = "MonitoreoAlertamientos";
        private static string MonitoreoFirmas = "MonitoreoFirmas";
        private static string MonitoreoSMS = "MonitoreoSMS";
        private static string app = ConfigurationManager.AppSettings["applicationId"].ToString();
        private static string passAppAzure = ConfigurationManager.AppSettings["clientKey"].ToString();
        //private static bool activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);
        private static string Componente = Assembly.GetExecutingAssembly().GetName().Name;
        private static string Usuario = Environment.UserName;
        private static string Instancia = ConfigurationManager.AppSettings["Instancia"].ToString();
        //private static bool activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);

        public RespuestaGenerica EncenderMonitoreoTabla()
        {
            RespuestaGenerica Respuesta = new RespuestaGenerica();
            CorreoObligatoria correo = null;
            Boolean EstaMonitoreando = false;
            EntradaAlertamientosMonitoreo EntradaAlertamientosMonitoreo = new EntradaAlertamientosMonitoreo();

            try
            {
                EntradaAlertamientosMonitoreo.CadenaConexion = EntidadAlertamientos.MonitoreoAlertamientosBDEscritura;
                EntradaAlertamientosMonitoreo.ClaveAplicacion = Componente;
                EntradaAlertamientosMonitoreo.Instancia = Instancia;

                Boolean EstaHabilitadaMonitoreoCorreo = EntidadAlertamientos.MonitoreoCorreoEstaHabilita;
                if (EstaHabilitadaMonitoreoCorreo)
                {
                    correo = new CorreoObligatoria();

                    correo.ListaCorreos = new List<String>();

                    String[] splitListaCorreos = EntidadAlertamientos.MonitoreoCorreoListaPara.Split(';');
                    for (int i = 0; i < splitListaCorreos.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(splitListaCorreos[i]))
                        {
                            correo.ListaCorreos.Add(splitListaCorreos[i]);
                        }
                    }

                    correo.ListaCorreosCopia = new List<String>();
                    String[] splitListaCorreosCopia = EntidadAlertamientos.MonitoreoCorreoListaCC.Split(';');
                    for (int i = 0; i < splitListaCorreosCopia.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(splitListaCorreosCopia[i]))
                        {
                            correo.ListaCorreosCopia.Add(splitListaCorreosCopia[i]);
                        }
                    }

                    String[] splitCorreoParametros = EntidadAlertamientos.MonitoreoCorreoParametros.Split(';');

                    correo.ServidorCorreo = splitCorreoParametros[0].Replace("Servidor=", "");
                    correo.UsuarioCorreo = splitCorreoParametros[1].Replace("Usuario=", "");
                    correo.ClaveUsuarioCorreo = splitCorreoParametros[2].Replace("Clave=", "");
                    correo.PuertoCorreo = Convert.ToInt32(splitCorreoParametros[3].Replace("Puerto=", ""));
                    correo.CorreoRemitente = splitCorreoParametros[4].Replace("Remitente=", "");
                    correo.UrlXML = EntidadAlertamientos.MonitoreoCorreoUrlXML;
                    correo.EnableSsl = Convert.ToBoolean(splitCorreoParametros[5].Replace("EnableSsl=", ""));
                }

                Boolean EstaHabilitadaAspnet = EntidadAlertamientos.MonitoreoTablaaspnet_ApplicationsEstaHabilita;
                if (EstaHabilitadaAspnet)
                {
                    EntradaTablaMonitoreo Entrada = new EntradaTablaMonitoreo();
                    Entrada.Conexion = new Conexion();
                    String[] splitAspnetParametros = EntidadAlertamientos.MonitoreoTablaaspnet_ApplicationsParametros.Split(';');

                    Entrada.Conexion.Servidor = splitAspnetParametros[0].Replace("Data Source=", "");
                    Entrada.Conexion.BaseDatos = splitAspnetParametros[1].Replace("Initial Catalog=", "");
                    Entrada.Conexion.Usuario = splitAspnetParametros[2].Replace("User ID=", "");
                    Entrada.Conexion.Contrasena = splitAspnetParametros[3].Replace("Password=", "");
                    Entrada.caracteristicasTablas = new List<CaracteristicasTabla>();
                    CaracteristicasTabla ca = new CaracteristicasTabla();
                    ca.NombreEsquema = splitAspnetParametros[5].Replace("Esquema=", "");
                    ca.NombreTabla = splitAspnetParametros[6].Replace("Tabla=", "");
                    Entrada.caracteristicasTablas.Add(ca);

                    if (EstaHabilitadaMonitoreoCorreo)
                    {
                        Tabla MonitoreoCorreo = new Tabla(correo, EntradaAlertamientosMonitoreo);
                        Respuesta = MonitoreoCorreo.Monitoreo(Entrada);
                    }
                    else
                    {
                        Tabla Monitoreo = new Tabla(EntradaAlertamientosMonitoreo);
                        Respuesta = Monitoreo.Monitoreo(Entrada);
                    }
                    if (Respuesta.Codigo != 0)
                    {
                        return Respuesta;
                    }
                    else
                    {
                        EstaMonitoreando = true;
                    }

                }

                Boolean EstaHabilitadaConfiguraciones = EntidadAlertamientos.MonitoreoTablaConfiguracionesEstaHabilita;
                if (EstaHabilitadaConfiguraciones)
                {
                    EntradaTablaMonitoreo Entrada = new EntradaTablaMonitoreo();
                    Entrada.Conexion = new Conexion();
                    String[] splitConfiguracionesParametros = EntidadAlertamientos.MonitoreoTablaConfiguracionesParametros.Split(';');

                    Entrada.Conexion.Servidor = splitConfiguracionesParametros[0].Replace("Data Source=", "");
                    Entrada.Conexion.BaseDatos = splitConfiguracionesParametros[1].Replace("Initial Catalog=", "");
                    Entrada.Conexion.Usuario = splitConfiguracionesParametros[2].Replace("User ID=", "");
                    Entrada.Conexion.Contrasena = splitConfiguracionesParametros[3].Replace("Password=", "");
                    Entrada.caracteristicasTablas = new List<CaracteristicasTabla>();
                    CaracteristicasTabla ca = new CaracteristicasTabla();
                    ca.NombreEsquema = splitConfiguracionesParametros[5].Replace("Esquema=", "");
                    ca.NombreTabla = splitConfiguracionesParametros[6].Replace("Tabla=", "");
                    Entrada.caracteristicasTablas.Add(ca);

                    if (EstaHabilitadaMonitoreoCorreo)
                    {
                        Tabla MonitoreoCorreo = new Tabla(correo, EntradaAlertamientosMonitoreo);
                        Respuesta = MonitoreoCorreo.Monitoreo(Entrada);
                    }
                    else
                    {
                        Tabla Monitoreo = new Tabla(EntradaAlertamientosMonitoreo);
                        Respuesta = Monitoreo.Monitoreo(Entrada);
                    }
                    if (Respuesta.Codigo != 0)
                    {
                        return Respuesta;
                    }
                    else
                    {
                        EstaMonitoreando = true;
                    }

                }

                Boolean EstaHabilitadaContrato = EntidadAlertamientos.MonitoreoTablaContratoValoresFijosEstaHabilita;
                if (EstaHabilitadaContrato)
                {
                    EntradaTablaMonitoreo Entrada = new EntradaTablaMonitoreo();
                    Entrada.Conexion = new Conexion();
                    String[] splitContratoParametros = EntidadAlertamientos.MonitoreoTablaContratoValoresFijosParametros.Split(';');

                    Entrada.Conexion.Servidor = splitContratoParametros[0].Replace("Data Source=", "");
                    Entrada.Conexion.BaseDatos = splitContratoParametros[1].Replace("Initial Catalog=", "");
                    Entrada.Conexion.Usuario = splitContratoParametros[2].Replace("User ID=", "");
                    Entrada.Conexion.Contrasena = splitContratoParametros[3].Replace("Password=", "");
                    Entrada.caracteristicasTablas = new List<CaracteristicasTabla>();
                    CaracteristicasTabla ca = new CaracteristicasTabla();
                    ca.NombreEsquema = splitContratoParametros[5].Replace("Esquema=", "");
                    ca.NombreTabla = splitContratoParametros[6].Replace("Tabla=", "");
                    Entrada.caracteristicasTablas.Add(ca);

                    if (EstaHabilitadaMonitoreoCorreo)
                    {
                        Tabla MonitoreoCorreo = new Tabla(correo, EntradaAlertamientosMonitoreo);
                        Respuesta = MonitoreoCorreo.Monitoreo(Entrada);
                    }
                    else
                    {
                        Tabla Monitoreo = new Tabla(EntradaAlertamientosMonitoreo);
                        Respuesta = Monitoreo.Monitoreo(Entrada);
                    }
                    if (Respuesta.Codigo != 0)
                    {
                        return Respuesta;
                    }
                    else
                    {
                        EstaMonitoreando = true;
                    }

                }

                Boolean EstaHabilitadaValor = EntidadAlertamientos.MonitoreoTablaValorParametroMultiasignacionEstaHabilita;
                if (EstaHabilitadaValor)
                {
                    EntradaTablaMonitoreo Entrada = new EntradaTablaMonitoreo();
                    Entrada.Conexion = new Conexion();
                    String[] splitValorParametros = EntidadAlertamientos.MonitoreoTablaValorParametroMultiasignacionParametros.Split(';');

                    Entrada.Conexion.Servidor = splitValorParametros[0].Replace("Data Source=", "");
                    Entrada.Conexion.BaseDatos = splitValorParametros[1].Replace("Initial Catalog=", "");
                    Entrada.Conexion.Usuario = splitValorParametros[2].Replace("User ID=", "");
                    Entrada.Conexion.Contrasena = splitValorParametros[3].Replace("Password=", "");
                    Entrada.caracteristicasTablas = new List<CaracteristicasTabla>();
                    CaracteristicasTabla ca = new CaracteristicasTabla();
                    ca.NombreEsquema = splitValorParametros[5].Replace("Esquema=", "");
                    ca.NombreTabla = splitValorParametros[6].Replace("Tabla=", "");
                    Entrada.caracteristicasTablas.Add(ca);

                    if (EstaHabilitadaMonitoreoCorreo)
                    {
                        Tabla MonitoreoCorreo = new Tabla(correo, EntradaAlertamientosMonitoreo);
                        Respuesta = MonitoreoCorreo.Monitoreo(Entrada);
                    }
                    else
                    {
                        Tabla Monitoreo = new Tabla(EntradaAlertamientosMonitoreo);
                        Respuesta = Monitoreo.Monitoreo(Entrada);
                    }
                    if (Respuesta.Codigo != 0)
                    {
                        return Respuesta;
                    }
                    else
                    {
                        EstaMonitoreando = true;
                    }

                }

                //Boolean EstaHabilitadatb_Empleados = getMonitoreoEstaHabilita(TablaEmpleados);
                //if (EstaHabilitadatb_Empleados)
                //{
                //    EntradaTablaMonitoreo Entrada = new EntradaTablaMonitoreo();
                //    Entrada.Conexion = new Conexion();
                //    Entrada.Conexion.Servidor = getMonitoreoServidor(TablaEmpleados);
                //    Entrada.Conexion.BaseDatos = getMonitoreoBaseDatos(TablaEmpleados);
                //    Entrada.Conexion.Usuario = getMonitoreoUsuario(TablaEmpleados);
                //    Entrada.Conexion.Contrasena = getMonitoreoContrasena(TablaEmpleados);
                //    Entrada.caracteristicasTablas = new List<CaracteristicasTabla>();
                //    CaracteristicasTabla ca = new CaracteristicasTabla();
                //    ca.NombreEsquema = getMonitoreoNombreEsquema(TablaEmpleados);
                //    ca.NombreTabla = getMonitoreoNombreTabla(TablaEmpleados);
                //    Entrada.caracteristicasTablas.Add(ca);

                //    if (EstaHabilitadaMonitoreoCorreo)
                //    {
                //        Tabla MonitoreoCorreo = new Tabla(correo);
                //        Respuesta = MonitoreoCorreo.Monitoreo(Entrada);
                //    }
                //    else
                //    {
                //        Tabla Monitoreo = new Tabla();
                //        Respuesta = Monitoreo.Monitoreo(Entrada);
                //    }
                //    if (Respuesta.Codigo != 0)
                //    {
                //        return Respuesta;
                //    }
                //    else
                //    {
                //        EstaMonitoreando = true;
                //    }

                //}

                if (EstaMonitoreando)
                {
                    Respuesta.Codigo = 0;
                    Respuesta.Respuesta = "Se habilito el Monitoreo de Tablas";
                    return Respuesta;
                }
                else
                {
                    Respuesta.Codigo = -1;
                    Respuesta.Respuesta = "No se habilito ningun Monitoreo de Tablas";
                    return Respuesta;
                }




            }
            catch (Exception ex)
            {

                String MensajeFInal = "";
                if (ex.InnerException != null)
                {
                    MensajeFInal = "No se pudo EncenderMonitoreoTabla |Excepcion: " + ex.Message.ToString() + "Excepcion Interna: " + ex.InnerException.Message.ToString();
                }
                else
                {
                    MensajeFInal = "No se pudo EncenderMonitoreoTabla |Excepcion: " + ex.Message.ToString() + "Linea: " + ex.StackTrace.ToString(); ;
                }

                Respuesta.Codigo = -2;
                Respuesta.Respuesta = MensajeFInal;

                return Respuesta;
            }

        }

        public RespuestaGenerica EncenderMonitoreoArchivo()
        {
            RespuestaGenerica Respuesta = new RespuestaGenerica();
            CorreoObligatoria correo = null;
            Boolean EstaMonitoreando = false;
            EntradaAlertamientosMonitoreo EntradaAlertamientosMonitoreo = new EntradaAlertamientosMonitoreo();

            try
            {
                EntradaAlertamientosMonitoreo.CadenaConexion = EntidadAlertamientos.MonitoreoAlertamientosBDEscritura;
                EntradaAlertamientosMonitoreo.ClaveAplicacion = Componente;
                EntradaAlertamientosMonitoreo.Instancia = Instancia;

                Boolean EstaHabilitadaMonitoreoCorreo = EntidadAlertamientos.MonitoreoCorreoEstaHabilita;
                if (EstaHabilitadaMonitoreoCorreo)
                {
                    correo = new CorreoObligatoria();

                    correo.ListaCorreos = new List<String>();
                    String[] splitListaCorreos = EntidadAlertamientos.MonitoreoCorreoListaPara.Split(';');
                    for (int i = 0; i < splitListaCorreos.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(splitListaCorreos[i]))
                        {
                            correo.ListaCorreos.Add(splitListaCorreos[i]);
                        }
                    }
                    correo.ListaCorreosCopia = new List<String>();

                    String[] splitListaCorreosCopia = EntidadAlertamientos.MonitoreoCorreoListaCC.Split(';');
                    for (int i = 0; i < splitListaCorreosCopia.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(splitListaCorreosCopia[i]))
                        {
                            correo.ListaCorreosCopia.Add(splitListaCorreosCopia[i]);
                        }
                    }

                    String[] splitCorreoParametros = EntidadAlertamientos.MonitoreoCorreoParametros.Split(';');

                    correo.ServidorCorreo = splitCorreoParametros[0].Replace("Servidor=", "");
                    correo.UsuarioCorreo = splitCorreoParametros[1].Replace("Usuario=", "");
                    correo.ClaveUsuarioCorreo = splitCorreoParametros[2].Replace("Clave=", "");
                    correo.PuertoCorreo = Convert.ToInt32(splitCorreoParametros[3].Replace("Puerto=", ""));
                    correo.CorreoRemitente = splitCorreoParametros[4].Replace("Remitente=", "");
                    correo.UrlXML = EntidadAlertamientos.MonitoreoCorreoUrlXML;
                    correo.EnableSsl = Convert.ToBoolean(splitCorreoParametros[5].Replace("EnableSsl=", ""));
                }

                //Boolean EstaHabilitadaArchivos = getMonitoreoEstaHabilita(MonitoreoArchivos);
                if (true)
                {
                    EntradaArchivoMonitoreo Entrada = new EntradaArchivoMonitoreo();

                    Entrada.UrlArchivos = EntidadAlertamientos.MonitoreoArchivosUbicacionArchivos;
                    Entrada.IncluirSubdirectorios = true;
                    Entrada.Extension = new List<String>();
                    Entrada.Extension.Add("*");

                    SMSObligatoria SMSObligatoria = new SMSObligatoria();
                    SMSObligatoria.Destinatario = EntidadAlertamientos.MonitoreoSMSDestinatario;
                    SMSObligatoria.Usuario = EntidadAlertamientos.MonitoreoSMSUsuario;
                    SMSObligatoria.Password = EntidadAlertamientos.MonitoreoSMSPassword;
                    SMSObligatoria.UrlServicioEnviarMensaje = EntidadAlertamientos.MonitoreoSMSUrlServicioEnviarMensaje;

                    if (EstaHabilitadaMonitoreoCorreo)
                    {
                        Archivo MonitoreoCorreo = new Archivo(correo, EntradaAlertamientosMonitoreo, SMSObligatoria);
                        Respuesta = MonitoreoCorreo.Monitoreo(Entrada);
                    }
                    else
                    {
                        Archivo Monitoreo = new Archivo(EntradaAlertamientosMonitoreo);
                        Respuesta = Monitoreo.Monitoreo(Entrada);
                    }
                    if (Respuesta.Codigo != 0)
                    {
                        return Respuesta;
                    }
                    else
                    {
                        EstaMonitoreando = true;
                    }

                }

                if (EstaMonitoreando)
                {
                    Respuesta.Codigo = 0;
                    Respuesta.Respuesta = "Se habilito el Monitoreo de Archivos";
                    return Respuesta;
                }
                else
                {
                    Respuesta.Codigo = -1;
                    Respuesta.Respuesta = "No se habilito ningun Monitoreo de Archivos";
                    return Respuesta;
                }



            }
            catch (Exception ex)
            {
                String MensajeFInal = "";
                if (ex.InnerException != null)
                {
                    MensajeFInal = "No se pudo EncenderMonitoreoArchivo |Excepcion: " + ex.Message.ToString() + "Excepcion Interna: " + ex.InnerException.Message.ToString();
                }
                else
                {
                    MensajeFInal = "No se pudo EncenderMonitoreoArchivo |Excepcion: " + ex.Message.ToString() + "Linea: " + ex.StackTrace.ToString(); ;
                }

                Respuesta.Codigo = -2;
                Respuesta.Respuesta = MensajeFInal;

                return Respuesta;
            }
        }

        public RespuestaGenerica ConsultarAlertamientosAbiertosXClave()
        {
            RespuestaGenerica Respuesta = new RespuestaGenerica();
            EntradaAlertamientosMonitoreo EntradaAlertamientosMonitoreo = new EntradaAlertamientosMonitoreo();

            try
            {
                String[] splitAlertamientosParametros = EntidadAlertamientos.MonitoreoAlertamientosParametros.Split(';');

                if ((EntidadAlertamientos.UtlimaConsultaAlertamientos.AddMinutes(Convert.ToInt32(splitAlertamientosParametros[1].Replace("RevisarAlertasMinutos=", "")))
                    <= DateTime.Now) || EntidadAlertamientos.TieneAlertamientos == true)
                {
                    EntidadAlertamientos.UtlimaConsultaAlertamientos = DateTime.Now;

                    EntradaAlertamientosMonitoreo.CadenaConexion = EntidadAlertamientos.MonitoreoAlertamientosBDLectura;
                    EntradaAlertamientosMonitoreo.ClaveAplicacion = Componente;
                    EntradaAlertamientosMonitoreo.Instancia = Instancia;

                    PeticionConsultarAlertamientosAbiertosXClaveInstancia PeticionConsultarAlertamientosAbiertosXClave = new PeticionConsultarAlertamientosAbiertosXClaveInstancia();

                    PeticionConsultarAlertamientosAbiertosXClave.ClaveAplicacion = EntradaAlertamientosMonitoreo.ClaveAplicacion;
                    PeticionConsultarAlertamientosAbiertosXClave.NumeroHorasActivas = Convert.ToInt32(splitAlertamientosParametros[0].Replace("NumeroHorasActivas=", ""));
                    PeticionConsultarAlertamientosAbiertosXClave.Instancia = Instancia;

                    SMSObligatoria SMSObligatoria = new SMSObligatoria();
                    SMSObligatoria.Destinatario = EntidadAlertamientos.MonitoreoSMSDestinatario;
                    SMSObligatoria.Usuario = EntidadAlertamientos.MonitoreoSMSUsuario;
                    SMSObligatoria.Password = EntidadAlertamientos.MonitoreoSMSPassword;
                    SMSObligatoria.UrlServicioEnviarMensaje = EntidadAlertamientos.MonitoreoSMSUrlServicioEnviarMensaje;

                    Alertamientos MonitoreoAlertamientosConstructor = new Alertamientos(EntradaAlertamientosMonitoreo, SMSObligatoria);
                    Respuesta = MonitoreoAlertamientosConstructor.ConsultarAlertamientosAbiertosXClaveInstancia(PeticionConsultarAlertamientosAbiertosXClave);

                    if (Respuesta.Codigo != 0)
                    {
                        return Respuesta;
                    }

                    List<AlertamientosAplicativos> ListaAlertamientosAplicativos = (List<AlertamientosAplicativos>)Respuesta.Respuesta;

                    if (ListaAlertamientosAplicativos.Count > 0)
                    {
                        EntidadAlertamientos.TieneAlertamientos = true;
                    }
                    else
                    {
                        EntidadAlertamientos.TieneAlertamientos = false;
                    }
                }
                else
                {
                    Respuesta.Codigo = 0;
                    Respuesta.Respuesta = "Aun no es necesario verificar alertamientos";
                }

                return Respuesta;
            }
            catch (Exception ex)
            {
                String MensajeFInal = "";
                if (ex.InnerException != null)
                {
                    MensajeFInal = "No se pudo ConsultarAlertamientosAbiertosXClave |Excepcion: " + ex.Message.ToString() + "Excepcion Interna: " + ex.InnerException.Message.ToString();
                }
                else
                {
                    MensajeFInal = "No se pudo ConsultarAlertamientosAbiertosXClave |Excepcion: " + ex.Message.ToString() + "Linea: " + ex.StackTrace.ToString(); ;
                }

                Respuesta.Codigo = -2;
                Respuesta.Respuesta = MensajeFInal;

                return Respuesta;
            }
        }

        public RespuestaGenerica VerificarFirmasAplicativos()
        {
            RespuestaGenerica Respuesta = new RespuestaGenerica();
            EntradaAlertamientosMonitoreo EntradaAlertamientosMonitoreo = new EntradaAlertamientosMonitoreo();

            try
            {
                EntradaAlertamientosMonitoreo.CadenaConexion = EntidadAlertamientos.MonitoreoAlertamientosBDEscritura;
                EntradaAlertamientosMonitoreo.ClaveAplicacion = Componente;
                EntradaAlertamientosMonitoreo.Instancia = Instancia;

                SMSObligatoria SMSObligatoria = new SMSObligatoria();
                SMSObligatoria.Destinatario = EntidadAlertamientos.MonitoreoSMSDestinatario;
                SMSObligatoria.Usuario = EntidadAlertamientos.MonitoreoSMSUsuario;
                SMSObligatoria.Password = EntidadAlertamientos.MonitoreoSMSPassword;
                SMSObligatoria.UrlServicioEnviarMensaje = EntidadAlertamientos.MonitoreoSMSUrlServicioEnviarMensaje;
                Alertamientos MonitoreoAlertamientosConstructor = new Alertamientos(EntradaAlertamientosMonitoreo, SMSObligatoria);

                PeticionVerificarFirmasAplicativos PeticionVerificarFirmasAplicativos = new PeticionVerificarFirmasAplicativos();
                PeticionVerificarFirmasAplicativos.ListaFirmas = new List<Firma>();
                PeticionVerificarFirmasAplicativos.Usuario = Usuario;
                PeticionVerificarFirmasAplicativos.Instancia = Instancia;
                PeticionVerificarFirmasAplicativos.Componente = Componente;

                PeticionConsultarFirmaAplicativoXInstanciaComponente PeticionConsultarFirmaAplicativoXInstanciaComponente = new PeticionConsultarFirmaAplicativoXInstanciaComponente();
                PeticionConsultarFirmaAplicativoXInstanciaComponente.Instancia = PeticionVerificarFirmasAplicativos.Instancia;
                PeticionConsultarFirmaAplicativoXInstanciaComponente.Componente = Componente;

                RespuestaGenerica RespuestaConsultarFirmasAplicativoXInstanciaComponente = MonitoreoAlertamientosConstructor.ConsultarFirmasAplicativoXInstanciaComponente(PeticionConsultarFirmaAplicativoXInstanciaComponente);
                if (RespuestaConsultarFirmasAplicativoXInstanciaComponente.Codigo != 0)
                {
                    Respuesta = RespuestaConsultarFirmasAplicativoXInstanciaComponente;
                    return RespuestaConsultarFirmasAplicativoXInstanciaComponente;
                }

                List<AplicativosFirmas> ListaAplicativosFirmas = new List<AplicativosFirmas>();
                ListaAplicativosFirmas = (List<AplicativosFirmas>)RespuestaConsultarFirmasAplicativoXInstanciaComponente.Respuesta;

                if (ListaAplicativosFirmas.Count == 0)
                {
                    Respuesta.Codigo = -1;
                    Respuesta.Respuesta = "No cuenta con registros en la tabla AplicativosFirmas";
                    return Respuesta;
                }

                //Firmas Documentos
                string Ubicacion = EntidadAlertamientos.MonitoreoArchivosUbicacionArchivos;
                DirectoryInfo di = new DirectoryInfo(Ubicacion);
                FileInfo[] files = di.GetFiles();
                foreach (FileInfo file in files)
                {
                    foreach (AplicativosFirmas item in ListaAplicativosFirmas)
                    {
                        if (file.Name == item.Archivo)
                        {
                            Firma Firma = new Firma();
                            Firma.Instancia = PeticionVerificarFirmasAplicativos.Instancia;
                            Firma.Componente = PeticionVerificarFirmasAplicativos.Componente;
                            Firma.FechaCreacion = file.CreationTime.ToString("dd/MM/yyyy HH:mm:ss");
                            Firma.Nombre = file.Name;
                            Firma.FechaModificacion = file.CreationTime.ToString("dd/MM/yyyy HH:mm:ss");
                            Firma.Tamaño = file.Length.ToString();
                            PeticionVerificarFirmasAplicativos.ListaFirmas.Add(Firma);
                        }
                    }
                }

                RespuestaGenerica RespuestaVerificarFirmasAplicativos = MonitoreoAlertamientosConstructor.VerificarFirmasAplicativos(PeticionVerificarFirmasAplicativos);
                if (RespuestaVerificarFirmasAplicativos.Codigo != 0)
                {
                    Respuesta = RespuestaVerificarFirmasAplicativos;
                    return RespuestaVerificarFirmasAplicativos;
                }
                else
                {
                    Respuesta = RespuestaVerificarFirmasAplicativos;
                    return RespuestaVerificarFirmasAplicativos;
                }

            }
            catch (Exception ex)
            {
                String MensajeFInal = "";
                if (ex.InnerException != null)
                {
                    MensajeFInal = "No se pudo VerificarFirmasAplicativos |Excepcion: " + ex.Message.ToString() + "Excepcion Interna: " + ex.InnerException.Message.ToString();
                }
                else
                {
                    MensajeFInal = "No se pudo VerificarFirmasAplicativos |Excepcion: " + ex.Message.ToString() + "Linea: " + ex.StackTrace.ToString(); ;
                }

                Respuesta.Codigo = -2;
                Respuesta.Respuesta = MensajeFInal;

                return Respuesta;
            }
        }
        public RespuestaGenerica InsertarAlertamientoReinicio()
        {
            RespuestaGenerica Respuesta = new RespuestaGenerica();
            EntradaAlertamientosMonitoreo EntradaAlertamientosMonitoreo = new EntradaAlertamientosMonitoreo();

            try
            {
                EntradaAlertamientosMonitoreo.CadenaConexion = EntidadAlertamientos.MonitoreoAlertamientosBDEscritura;
                EntradaAlertamientosMonitoreo.ClaveAplicacion = Componente;
                EntradaAlertamientosMonitoreo.Instancia = Instancia;

                SMSObligatoria SMSObligatoria = new SMSObligatoria();
                SMSObligatoria.Destinatario = EntidadAlertamientos.MonitoreoSMSDestinatario;
                SMSObligatoria.Usuario = EntidadAlertamientos.MonitoreoSMSUsuario;
                SMSObligatoria.Password = EntidadAlertamientos.MonitoreoSMSPassword;
                SMSObligatoria.UrlServicioEnviarMensaje = EntidadAlertamientos.MonitoreoSMSUrlServicioEnviarMensaje;
                Alertamientos MonitoreoAlertamientosConstructor = new Alertamientos(EntradaAlertamientosMonitoreo, SMSObligatoria);

                PeticionInsertarAlertamiento peticionInsertarAlertamiento = new PeticionInsertarAlertamiento();

                peticionInsertarAlertamiento.ClaveAplicacion = Componente;
                peticionInsertarAlertamiento.DescripcionAlertamiento = "Se Apago el servicio";
                peticionInsertarAlertamiento.UsuarioCreacionAlertamiento = Usuario;
                peticionInsertarAlertamiento.CadenaConexion = EntidadAlertamientos.MonitoreoAlertamientosBDEscritura;
                peticionInsertarAlertamiento.ID_AplicativoFirma = 0;
                peticionInsertarAlertamiento.Instancia = Instancia;

                RespuestaGenerica respuestaInsertarAlertamiento = MonitoreoAlertamientosConstructor.InsertarAlertamiento(peticionInsertarAlertamiento);
                if (respuestaInsertarAlertamiento.Codigo != 0)
                {
                    return respuestaInsertarAlertamiento;
                }
                else
                {
                    return respuestaInsertarAlertamiento;
                }
            }
            catch (Exception ex)
            {
                String MensajeFInal = "";
                if (ex.InnerException != null)
                {
                    MensajeFInal = "No se pudo InsertarAlertamientoReinicio |Excepcion: " + ex.Message.ToString() + "Excepcion Interna: " + ex.InnerException.Message.ToString();
                }
                else
                {
                    MensajeFInal = "No se pudo InsertarAlertamientoReinicio |Excepcion: " + ex.Message.ToString() + "Linea: " + ex.StackTrace.ToString(); ;
                }

                Respuesta.Codigo = -2;
                Respuesta.Respuesta = MensajeFInal;

                return Respuesta;
            }
        }
        public RespuestaGenerica EvaluarFirmaParametros()
        {
            RespuestaGenerica Respuesta = new RespuestaGenerica();
            EntradaAlertamientosMonitoreo EntradaAlertamientosMonitoreo = new EntradaAlertamientosMonitoreo();

            try
            {
                PeticionEvaluarFirmaParametros PeticionEvaluarFirmaParametros = new PeticionEvaluarFirmaParametros();
                PeticionEvaluarFirmaParametros.Componente = Componente;
                PeticionEvaluarFirmaParametros.RutaXml = EntidadAlertamientos.MonitoreoFirmasUbicacionArchivoParametros;

                Alertamientos MonitoreoAlertamientosConstructor = new Alertamientos();
                RespuestaGenerica RespuestaEvaluarFirmaParametros = MonitoreoAlertamientosConstructor.EvaluarFirmaParametros(PeticionEvaluarFirmaParametros);
                if (RespuestaEvaluarFirmaParametros.Codigo != 0)
                {
                    Respuesta = RespuestaEvaluarFirmaParametros;
                    return RespuestaEvaluarFirmaParametros;
                }
                else
                {
                    Respuesta = RespuestaEvaluarFirmaParametros;
                    return RespuestaEvaluarFirmaParametros;
                }

            }
            catch (Exception ex)
            {
                String MensajeFInal = "";
                if (ex.InnerException != null)
                {
                    MensajeFInal = "No se pudo EvaluarFirmaParametros |Excepcion: " + ex.Message.ToString() + "Excepcion Interna: " + ex.InnerException.Message.ToString();
                }
                else
                {
                    MensajeFInal = "No se pudo EvaluarFirmaParametros |Excepcion: " + ex.Message.ToString() + "Linea: " + ex.StackTrace.ToString(); ;
                }

                Respuesta.Codigo = -2;
                Respuesta.Respuesta = MensajeFInal;

                return Respuesta;
            }
        }

        //Libreria Monitoreo Variables
        public static Boolean getMonitoreoEstaHabilita(String ClavePlugIn)
        {
            return Convert.ToBoolean(ConfigurationManager.AppSettings.Get(ClavePlugIn + "EstaHabilita"));
        }

        public static String getMonitoreoUrlXML(String ClavePlugIn)
        {
            return ConfigurationManager.AppSettings.Get(ClavePlugIn + "UrlXML");
        }

        public static String getMonitoreoUbicacionArchivos(String ClavePlugIn)
        {
            return ConfigurationManager.AppSettings.Get(ClavePlugIn + "UbicacionArchivos");
        }
        public static String getMonitoreoUbicacionArchivoParametros(String ClavePlugIn)
        {
            return ConfigurationManager.AppSettings.Get(ClavePlugIn + "UbicacionArchivoParametros");
        }

        public static String getMonitoreoSMSUrlServicioEnviarMensaje(String ClavePlugIn)
        {
            return ConfigurationManager.AppSettings.Get(ClavePlugIn + "UrlServicioEnviarMensaje");
        }

        //Fin Libreria Monitoreo Variables
        public static string obtenerCadenaAzure(string key)
        {
            String Respuesta = string.Empty;
            try
            {
                string clave = ConfigurationManager.AppSettings[key].ToString();

                if (true)
                {
                    responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerCadenasDeConexionAzure
                        (app, passAppAzure, clave);
                    if (respuestaObtenerCadena.codRespuesta == "0000")
                    {
                        Logueo.Evento("Valor Azure obtenido correctamente del secreto:" + clave);
                        Respuesta = respuestaObtenerCadena.valorAzure;
                    }
                    else
                    {
                        Logueo.Error("Error al consultar Secreto Azure: " + clave + " Json:" + JsonConvert.SerializeObject(respuestaObtenerCadena));
                    }
                }
                else
                {
                    Logueo.Error("Esta desactivada la consulta de Azure");
                }
                return Respuesta;
            }
            catch (Exception ex)
            {
                String MensajeFInal = "";
                if (ex.InnerException != null)
                {
                    MensajeFInal = "No se pudo obtenerCadenaAzure |Excepcion: " + ex.Message.ToString() + "Excepcion Interna: " + ex.InnerException.Message.ToString();
                }
                else
                {
                    MensajeFInal = "No se pudo obtenerCadenaAzure |Excepcion: " + ex.Message.ToString() + "Linea: " + ex.StackTrace.ToString(); ;
                }

                Logueo.Error(MensajeFInal);
                return Respuesta;
            }


        }

        public static RespuestaGenerica CargarValoresFijos()
        {
            RespuestaGenerica Respuesta = new RespuestaGenerica();
            try
            {
                EntidadAlertamientos.MonitoreoTablaaspnet_ApplicationsEstaHabilita = getMonitoreoEstaHabilita(TablaAspnet);

                EntidadAlertamientos.MonitoreoTablaaspnet_ApplicationsParametros = obtenerCadenaAzure(TablaAspnet + "Parametros");

                EntidadAlertamientos.MonitoreoTablaConfiguracionesEstaHabilita = getMonitoreoEstaHabilita(TablaConfiguraciones);
                EntidadAlertamientos.MonitoreoTablaConfiguracionesParametros = obtenerCadenaAzure(TablaConfiguraciones + "Parametros");

                EntidadAlertamientos.MonitoreoTablaContratoValoresFijosEstaHabilita = getMonitoreoEstaHabilita(TablaContrato);
                EntidadAlertamientos.MonitoreoTablaContratoValoresFijosParametros = obtenerCadenaAzure(TablaContrato + "Parametros");

                EntidadAlertamientos.MonitoreoTablaValorParametroMultiasignacionEstaHabilita = getMonitoreoEstaHabilita(TablaValor);
                EntidadAlertamientos.MonitoreoTablaValorParametroMultiasignacionParametros = obtenerCadenaAzure(TablaValor + "Parametros");

                EntidadAlertamientos.MonitoreoArchivosUbicacionArchivos = getMonitoreoUbicacionArchivos(MonitoreoArchivos);

                EntidadAlertamientos.MonitoreoCorreoEstaHabilita = getMonitoreoEstaHabilita(MonitoreoCorreo);
                EntidadAlertamientos.MonitoreoCorreoListaPara = obtenerCadenaAzure(MonitoreoCorreo + "ListaPara");
                EntidadAlertamientos.MonitoreoCorreoListaCC = obtenerCadenaAzure(MonitoreoCorreo + "ListaCC");
                EntidadAlertamientos.MonitoreoCorreoParametros = obtenerCadenaAzure(MonitoreoCorreo + "Parametros");
                EntidadAlertamientos.MonitoreoCorreoUrlXML = getMonitoreoUrlXML(MonitoreoCorreo);

                EntidadAlertamientos.MonitoreoAlertamientosBDLectura = obtenerCadenaAzure(MonitoreoAlertamientos + "BDLectura");
                EntidadAlertamientos.MonitoreoAlertamientosBDEscritura = obtenerCadenaAzure(MonitoreoAlertamientos + "BDEscritura");
                EntidadAlertamientos.MonitoreoAlertamientosParametros = obtenerCadenaAzure(MonitoreoAlertamientos + "Parametros");

                EntidadAlertamientos.MonitoreoFirmasUbicacionArchivoParametros = getMonitoreoUbicacionArchivoParametros(MonitoreoFirmas);

                EntidadAlertamientos.MonitoreoSMSDestinatario = obtenerCadenaAzure(MonitoreoSMS + "Destinatario");
                EntidadAlertamientos.MonitoreoSMSUsuario = obtenerCadenaAzure(MonitoreoSMS + "Usuario");
                EntidadAlertamientos.MonitoreoSMSPassword = obtenerCadenaAzure(MonitoreoSMS + "Password");
                EntidadAlertamientos.MonitoreoSMSUrlServicioEnviarMensaje = getMonitoreoSMSUrlServicioEnviarMensaje(MonitoreoSMS);

                Respuesta.Codigo = 0;
                Respuesta.Respuesta = "Se cargaron valores exitosamente";
                return Respuesta;

            }
            catch (Exception ex)
            {
                var MensajeFInal = string.Empty;
                if (ex.InnerException != null)
                {
                    MensajeFInal = "No se pudo CargarValoresFijos |Excepcion: " + ex.Message.ToString() + "Excepcion Interna: " + ex.InnerException.Message.ToString();
                }
                else
                {
                    MensajeFInal = "No se pudo CargarValoresFijos |Excepcion: " + ex.Message.ToString() + "Linea: " + ex.StackTrace.ToString(); ;
                }

                Respuesta.Codigo = -2;
                Respuesta.Respuesta = MensajeFInal;
                return Respuesta;
            }


        }
    }
}
