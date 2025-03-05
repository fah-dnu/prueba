using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Limpieza.BaseDatos
{
    class DBProcesadorArchivo
    {
        public static String strBDLecturaArchivo
        {
            get
            {
                return PNConfig.Get("Limpieza", "BDReadData");
            }
        }

    }
}
