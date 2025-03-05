using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Web.Services.Description;

namespace Procesador
{
    [RunInstaller(true)]
    public partial class ProcesadorInstaller : System.Configuration.Install.Installer
    {

        private ServiceInstaller serviceInstaller1;
        private ServiceProcessInstaller processInstaller;

        internal static string ServiceNameDefault = "";

        internal static string ServiceName = GetConfigurationValue("Instancia");

        public ProcesadorInstaller()
        {



            InitializeComponent();

            // Instantiate installers for process and services.
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller1 = new ServiceInstaller();


            // The services run under the system account.
            processInstaller.Account = ServiceAccount.LocalSystem;

            // The services are started manually.
            serviceInstaller1.StartType = ServiceStartMode.Automatic;


            serviceInstaller1.ServiceName = "ProcesadorNocturno";

            //serviceInstaller1.ServiceName = ServiceName;

            // Add installers to collection. Order is not important.
            Installers.Add(serviceInstaller1);

            Installers.Add(processInstaller);

        }


        private static string GetConfigurationValue(string key)
        {
            Assembly service = Assembly.GetAssembly(typeof(Service));

            Configuration config = ConfigurationManager.OpenExeConfiguration(service.Location);

            if (config.AppSettings.Settings[key] != null)
                return config.AppSettings.Settings[key].Value;
            else
                return ServiceNameDefault;
        }

    }
}
