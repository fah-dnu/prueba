using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public static class Constants
    {
        public static string PROC_NAME
        {
            get
            {
                return "PROC_EMBOZADOR";
            }
        }

        public static string INSTANCIA
        {
            get
            {
                return PNConfig.Get(Constants.PROC_NAME, "Instancia");
            }
        }

        public static string ObtenerConfiguracion(string key)
        {
            return PNConfig.Get(Constants.PROC_NAME, key);
        }

        public static string OK
        {
            get
            {
                return "0000";
            }
        }


        public static string VERSION
        {
            get
            {
                return PNConfig.Get(Constants.PROC_NAME, "VersionEmbozo");
            }
        }

        public static Encoding ObtenerEncoding(string key)
        {
            string val = PNConfig.Get(Constants.PROC_NAME, key).ToUpper();

            if (val.Equals("ASCII"))
                return Encoding.ASCII;
            else if (val.Equals("UTF8"))
                return Encoding.UTF8;
            else if (val.Equals("UNICODE"))
                return Encoding.Unicode;
            else if (val.Equals("UTF32"))
                return Encoding.UTF32;
            else if (val.Equals("UTF7"))
                return Encoding.UTF7;
            else
                return Encoding.Unicode;
        }
    }
}
