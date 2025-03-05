using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using Dnu.AutorizadorParabiliaAzure.Services;
using Dnu.AutorizadorParabiliaAzure.Models;
using log4net;

namespace CommonProcesador.BaseDatos
{
    public class DBProceso
    {
        static DBProceso() {
            string appAKV = ConfigurationManager.AppSettings["applicationId"].ToString();
            string clave = ConfigurationManager.AppSettings["clientKey"].ToString();
            bool activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);
            if (activarAzure)
            {
                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(appAKV, clave);
            }
        }

        public static SqlConnection BDLectura
        {
            get
            {
                SqlConnection unaConexion = new SqlConnection(strBDLectura);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static SqlConnection BDEscritura
        {
            get
            {
                SqlConnection unaConexion = new SqlConnection(strBDEscritura);
                unaConexion.Open();
                return unaConexion;
            }
        }

        public static String strBDLectura
        {
            get
            {
                string app = ConfigurationManager.AppSettings["applicationId"].ToString();
                string appPass = ConfigurationManager.AppSettings["clientKey"].ToString();
                string claveCadena = ConfigurationManager.ConnectionStrings["DataBaseProcesos"].ToString();


                responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerCadenasDeConexionAzure(app, appPass, claveCadena);

                return respuestaObtenerCadena.valorAzure;

            }
        }

        public static String strBDEscritura
        {
            get
            {
                string aplication = ConfigurationManager.AppSettings["applicationId"].ToString();
                string cliente = ConfigurationManager.AppSettings["clientKey"].ToString();
                string claveCadena = ConfigurationManager.ConnectionStrings["DataBaseProcesos"].ToString();


                responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerCadenasDeConexionAzure(aplication, cliente, claveCadena);

                return respuestaObtenerCadena.valorAzure;
            }
        }

    }
}
