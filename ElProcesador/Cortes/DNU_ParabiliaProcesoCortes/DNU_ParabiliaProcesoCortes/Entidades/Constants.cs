using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    public class Constants
    {
        public static string VERSION
        {
            get
            {
                return PNConfig.Get("PROCESAEDOCUENTA", "VERSION");
            }
        }
    }
}
