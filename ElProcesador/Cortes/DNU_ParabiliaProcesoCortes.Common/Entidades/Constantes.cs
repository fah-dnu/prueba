using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Common.Entidades
{
    public class Constantes
    {
        public static readonly int longitudMedioAcceso = Convert.ToInt32(ConfigurationManager.AppSettings["longitudMedioAccesoAE"].ToString());
    }
}
