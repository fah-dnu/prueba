using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Configuration;
using CommonProcesador.Entidades;


namespace CommonProcesador
{
    public class PNConfig
    {
        public static string Get(String ClaveProceso, String NombreParamentro)
        {
            try
            {
                Propiedad unaPropiedad = new Propiedad();

                if (ConfiguracionContexto.ConfigApps == null || ConfiguracionContexto.ConfigApps[ClaveProceso] == null)
                {
                    ConfiguracionContexto.InicializarContexto();
                }

                if (ConfiguracionContexto.ConfigApps[ClaveProceso] == null)
                {
                    throw new Exception("No existe la Aplicacion: " + ClaveProceso + " en las Configuraciones");
                }



                if (ConfiguracionContexto.ConfigApps[ClaveProceso][NombreParamentro.ToUpper()] == null)
                {
                    ConfiguracionContexto.InicializarContexto();
                }


                if (ConfiguracionContexto.ConfigApps[ClaveProceso][NombreParamentro.ToUpper()] == null)
                {

                    throw new Exception("No existe la propiedad : " + NombreParamentro + " en la Aplicacion: " + ClaveProceso);
                }
                else
                {
                    unaPropiedad = ConfiguracionContexto.ConfigApps[ClaveProceso][NombreParamentro.ToUpper()];
                }

                return unaPropiedad.Valor;

            }
            catch (Exception err)
            {
                // throw new CAppException(8013, "No se puede obtener la propiedad: " + NombreParamentro, err);
                Logueo.Error(" Proceso:" + ClaveProceso + "; Parametro:" + NombreParamentro + "; " + err.Message);
                ConfiguracionContexto.InicializarContexto();
                return "";
            }
        }

    }
}
