using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MonitoreaOperaciones.Utilidades
{
    public class ConfigMonitor : ConfigurationSection
    {

        [ConfigurationProperty("Cadenas")]
        public CadenasCollection CadenasItems
        {
            get { return ((CadenasCollection)(base["Cadenas"])); }
        }

         [ConfigurationProperty("PeriodoTiempoMin", DefaultValue = "30", IsRequired = true)]
        public int PeriodoTiempoMin
        {
            get
            {
                return (int)this["PeriodoTiempoMin"];
            }

            set
            {
                this["PeriodoTiempoMin"] = value;
            }
        }


    }




}
