using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.BaseDatos
{
    class DBProcesadorArchivo
    {

        public static String strBDLecturaArchivo
        {
            get
            {
                return PNConfig.Get("ConciliacionSmart", "BDReadData");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");
            }
        }


        public static String strBDEscrituraArchivo
        {
            get
            {
                return PNConfig.Get("ConciliacionSmart", "BDReadData");// ;
            }
        }
    }
}
