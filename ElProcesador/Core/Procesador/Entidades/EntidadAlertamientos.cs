using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcesadorNocturno.Entidades
{
    public class EntidadAlertamientos
    {
        public static DateTime UtlimaConsultaAlertamientos { set; get; }
        public static bool TieneAlertamientos { set; get; }

        //Monitoreo Tablas
        //Tabla aaspnet_Applications
        public static bool MonitoreoTablaaspnet_ApplicationsEstaHabilita { set; get; }
        public static string MonitoreoTablaaspnet_ApplicationsParametros { set; get; }

        //Tabla Configuraciones
        public static bool MonitoreoTablaConfiguracionesEstaHabilita { set; get; }
        public static string MonitoreoTablaConfiguracionesParametros { set; get; }
        //Tabla ContratoValoresFijos
        public static bool MonitoreoTablaContratoValoresFijosEstaHabilita { set; get; }
        public static string MonitoreoTablaContratoValoresFijosParametros { set; get; }
        //Tabla ValorParametroMultiasignacion
        public static bool MonitoreoTablaValorParametroMultiasignacionEstaHabilita { set; get; }
        public static string MonitoreoTablaValorParametroMultiasignacionParametros { set; get; }

        //MonitoreoArchivos
        public static string MonitoreoArchivosUbicacionArchivos { set; get; }

        //Monitor Correo
        public static bool MonitoreoCorreoEstaHabilita { set; get; }
        public static string MonitoreoCorreoListaPara { set; get; }
        public static string MonitoreoCorreoListaCC { set; get; }
        public static string MonitoreoCorreoParametros { set; get; }
        public static string MonitoreoCorreoUrlXML { set; get; }

        //Monitoreo Alertamientos
        public static string MonitoreoAlertamientosBDLectura { set; get; }
        public static string MonitoreoAlertamientosBDEscritura { set; get; }
        public static string MonitoreoAlertamientosParametros { set; get; }

        //Monitoreo Firmas
        public static string MonitoreoFirmasUbicacionArchivoParametros { set; get; }

        //Monitoreo SMS
        public static string MonitoreoSMSDestinatario { set; get; }
        public static string MonitoreoSMSUsuario { set; get; }
        public static string MonitoreoSMSPassword { set; get; }
        public static string MonitoreoSMSUrlServicioEnviarMensaje { set; get; }
    }
}
