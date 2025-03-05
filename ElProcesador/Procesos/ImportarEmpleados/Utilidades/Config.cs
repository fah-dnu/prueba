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
using ImportarEmpleados.Entidades;
using ImportarEmpleados.BaseDatos;
//using CommonProcesador;



namespace ImportarEmpleados.Utilidades
{
    class Config
    {
       
        public static Dictionary<Int64, Dictionary<String, Parametro>> Configuraciones = new Dictionary<Int64, Dictionary<String, Parametro>>();

        static Config()
        {
            Configuraciones = DAOConfiguracion.GetConf();
        }

        public static String Get(Int64 ID_Cadena, String NombreParamentro)
        {
            try
            {
                Parametro unaPropiedad = new Parametro();

                if (Configuraciones[ID_Cadena] == null)
                {
                    Configuraciones = DAOConfiguracion.GetConf();
                }

                if (Configuraciones[ID_Cadena] == null)
                {
                    throw new Exception("No existe la Aplicacion: " + ID_Cadena + " en las Configuraciones");
                }



                if (Configuraciones[ID_Cadena][NombreParamentro.ToUpper()] == null)
                {
                    Configuraciones = DAOConfiguracion.GetConf();
                }


                if (Configuraciones[ID_Cadena][NombreParamentro.ToUpper()] == null)
                {

                    throw new Exception("No existe la propiedad : " + NombreParamentro + " en la Aplicacion: " + ID_Cadena);
                }
                else
                {
                    unaPropiedad = Configuraciones[ID_Cadena][NombreParamentro.ToUpper()];
                }

                return unaPropiedad.Valor;

            }
            catch (Exception err)
            {
                // throw new CAppException(8013, "No se puede obtener la propiedad: " + NombreParamentro, err);
                CommonProcesador.Logueo.Error(err.Message + " AppID:" + ID_Cadena + "; Parametro:" + NombreParamentro + "; " + err.Message);
                Configuraciones = DAOConfiguracion.GetConf();
                return "";
            }
        }
    }
}
