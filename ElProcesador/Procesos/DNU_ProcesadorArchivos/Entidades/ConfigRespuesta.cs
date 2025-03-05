using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DNU_ProcesadorArchivos.Entidades
{
    public class ConfigRespuesta
    {
        String FTPUser {get;set;}
        String FTPPass {get;set;}
        String FTPUbicacion {get;set;}
        String FTPIP {get;set;}
        String FTPPuerto {get;set;}
        String FTPTipoSeguridad {get;set;}
        String DistribucionEmail {get;set;}
        bool SoloOK { get; set; }

    }
}
