using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador.Entidades;
using CommonProcesador.BaseDatos;
using System.Configuration;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using log4net;
using Newtonsoft.Json;

namespace CommonProcesador
{
    public class ConfiguracionContexto
    {

        public static Dictionary<String, Dictionary<String, Propiedad>> ConfigApps;


        public static void InicializarContexto()
        {
            try
            {
                string appAKV = ConfigurationManager.AppSettings["applicationId"].ToString();
                string clave = ConfigurationManager.AppSettings["clientKey"].ToString();
                bool activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);
                if (activarAzure)
                {
                    responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(appAKV, clave);
                }
                ConfiguracionContexto.ConfigApps = DAOConfiguracion.GetConfiguraciones(appAKV, clave);
            }
            catch (Exception err)
            {//
                Logueo.Error(err.Message);
            }
        }

    }
}
