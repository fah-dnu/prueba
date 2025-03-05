using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MonitoreaOperaciones.Utilidades
{
    public class Config
    {
        private static Config _config;

        private Config()
        {
            this.Cadenas = (ConfigMonitor)ConfigurationManager.GetSection("MonitorOperaciones");
        }

        public static Config Instance()
        {
            if (_config == null)
                _config = new Config();

            return _config;
        }

        public ConfigMonitor Cadenas { get; private set; }




        //public Int64 LI_Activas { get; set; }
        //public Int64 LI_Declinadas { get; set; }
        //public Int64 LS_Activas { get; set; }
        //public Int64 LS_Declinadas { get; set; }
        //public Int64 ID_Cadena { get; set; }
        //public String ListaDistribucion { get; set; }
    }
}
