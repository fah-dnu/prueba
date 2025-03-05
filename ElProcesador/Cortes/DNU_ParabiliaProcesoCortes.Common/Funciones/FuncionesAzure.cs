using CommonProcesador;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Common.Funciones
{
    public class FuncionesAzure
    {
        public static bool ValidateAlwaysEncryptedColumn(string columnName)
        {
            try
            {
                columnName = columnName.Replace("@", "");
                string encryptedColumns = ConfigurationManager.AppSettings["columnasAE"].ToString();
                string[] listaColumnas = encryptedColumns.Split(',');
                foreach (string columna in listaColumnas)
                {
                    if (columnName.ToLower() == columna.ToLower())
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error en validacion columnas ae" + JsonConvert.SerializeObject(ex));
                return false;
            }
        }

        public static bool ValidateAzureConnection()
        {
            try
            {
                bool activeAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"].ToString());
                return activeAzure;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error en validacion de conexion" + JsonConvert.SerializeObject(ex));
                //Logueo log = new Logueo("");
                //log.Error("[Azure " + ex.Message + ex.StackTrace + "]");
                return false;
            }
        }

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
    }
}
