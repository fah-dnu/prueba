using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    class AzureExtensions
    {
        public static SqlParameter obtenerParametroSQL(string nombreParametro, string valorParametro, int longitudParametro, SqlDbType tipo)
        {
            SqlParameter paramSSN = new SqlParameter();//cmd.CreateParameter();
            paramSSN.ParameterName = nombreParametro;
            paramSSN.DbType = DbType.AnsiStringFixedLength;
            paramSSN.Direction = ParameterDirection.Input;
            paramSSN.Value = valorParametro;
            paramSSN.Size = longitudParametro;
            paramSSN.SqlDbType = tipo;//SqlDbType.NVarChar;
            //cmd.Parameters.Add(paramSSN);
            return paramSSN;
        }

        public static string ObtenerValorSecretoAzure(string cadena, string user, string idLog, LogueoAltaEmpleadoV2 logEmpelado)
        {
            bool activarAzure = false;
            try
            {
                activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);
            }
            catch (Exception ex)
            {
                activarAzure = false;
            }
            if (activarAzure)
            {
                string app = ConfigurationManager.AppSettings["applicationId"].ToString();
                string clave = ConfigurationManager.AppSettings["clientKey"].ToString();
                responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerValoresClaveAzureKeyVault(app, clave, cadena);
                if (respuestaObtenerCadena.codRespuesta == "0000")
                {
                    cadena = respuestaObtenerCadena.valorAzure;
                }
                else
                {
                    logEmpelado.Error("[Azure]" + JsonConvert.SerializeObject(respuestaObtenerCadena));
                }
            }
            return cadena;
        }
    }
}
