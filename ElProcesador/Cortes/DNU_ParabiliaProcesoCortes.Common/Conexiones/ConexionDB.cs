

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Common.Funciones;

namespace DNU_ParabiliaProcesoCortes.Common.ConexionDB
{
    public class DBConnection
    {


        private String stringConnection { get; set; }
    
        public DBConnection(string stringConnection)
        {
           this.stringConnection = stringConnection;
        }


        public DataTable BuildQuery(string storeProcedure, List<StoredSrocedureParameter> parametersProcedure
                                , string methodName, string timeOut = null
                                , SqlConnection conn = null, SqlTransaction transaction = null
                                , List<StoredProcedureOutputParameters> outputParameters = null)
        {

            if (conn == null)
            {
                return GetResult(storeProcedure, parametersProcedure, methodName, timeOut, outputParameters);
            }
            else
            {
                return GetResult(storeProcedure, parametersProcedure, methodName, timeOut, outputParameters, conn, transaction);
            }


        }

        private DataTable GetResult(string storeProcedure, List<StoredSrocedureParameter> parametersProcedure,
            string methodName, string timeOut, List<StoredProcedureOutputParameters> outputParameters,
            SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (transaction != null)
                        cmd.Transaction = transaction;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = string.IsNullOrEmpty(timeOut)
                                           ? 30
                                           : Convert.ToInt32(timeOut);
                    cmd.CommandText = storeProcedure;
                    //Deberia estar abierta , pero se corrobora que se abra la conexion
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    return GetExecutionResponse(cmd, parametersProcedure, outputParameters );


                }//Dispose SqlCommand
            }
            catch (Exception ex)
            {
                return DefaultResponse(methodName, ex);
            }
        }

        private DataTable GetResult(string storeProcedure, List<StoredSrocedureParameter> parametersProcedure, string methodName,
            string timeOut, List<StoredProcedureOutputParameters> outputParameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(stringConnection))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {

                        cmd.Connection = conn;

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = string.IsNullOrEmpty(timeOut)
                                               ? 30
                                               : Convert.ToInt32(timeOut);
                        cmd.CommandText = storeProcedure;
                        conn.Open();
                        return GetExecutionResponse(cmd, parametersProcedure, outputParameters);
                    }// Dispose SqlCommand
                }//Dispose SqlConnection
            }
            catch (Exception ex)
            {
                return DefaultResponse(methodName, ex);
            }
        }

        private DataTable GetExecutionResponse(SqlCommand cmd, List<StoredSrocedureParameter> parametersProcedure,
            List<StoredProcedureOutputParameters> outputParameters)
        {
            if (parametersProcedure != null && parametersProcedure.Count > 0)
            {
                bool activaAzure = FuncionesAzure.ValidateAzureConnection();
                foreach (StoredSrocedureParameter parameter in parametersProcedure)
                {
                    //cmd.Parameters.AddWithValue(parameter.nameParameter, parameter.value);

                    if ((!parameter.encrypted) && activaAzure && FuncionesAzure.ValidateAlwaysEncryptedColumn(parameter.nameParameter))
                    {
                        if (string.IsNullOrEmpty(parameter.value.ToString()))
                        {
                            continue;
                        }
                        parameter.encrypted = true;
                        parameter.length = Constantes.longitudMedioAcceso;
                    }
                    if (parameter.encrypted == false)
                    {
                        cmd.Parameters.AddWithValue(parameter.nameParameter, parameter.value);
                    }
                    else
                    {
                        SqlParameter paramSSN = cmd.CreateParameter();
                        paramSSN.ParameterName = parameter.nameParameter;
                        paramSSN.DbType = DbType.AnsiStringFixedLength;
                        paramSSN.Direction = ParameterDirection.Input;
                        paramSSN.Value = parameter.value;
                        paramSSN.Size = parameter.length;
                        cmd.Parameters.Add(paramSSN);
                    }
                }
            }
            if (outputParameters != null && outputParameters.Count > 0)
            {
                foreach (StoredProcedureOutputParameters parameter in outputParameters)
                {
                    if (parameter.type == SqlDbType.VarChar)
                    {
                        cmd.Parameters.Add(parameter.nameParameter, parameter.type, 1000).Direction = ParameterDirection.Output;

                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(parameter.nameParameter, parameter.type).Direction = ParameterDirection.Output;
                    }
                }
            }

            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                if (outputParameters != null)
                {
                    if (outputParameters.Count > 0)
                    {
                        foreach (StoredProcedureOutputParameters parameter in outputParameters)
                        {
                            parameter.value = cmd.Parameters[parameter.nameParameter].Value;
                        }
                    }
                }

                return opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
            }//Dispose SqlDataAdapter
        }

        private DataTable DefaultResponse(string methodName, Exception ex)
        {
            DataTable table = new DataTable();
            table.Columns.Add("tipo", typeof(string));
            table.Columns.Add("Mesaje", typeof(string));
            table.Columns.Add("MesajeReal", typeof(string));
            table.Columns.Add("Codigo", typeof(string));
            table.Rows.Add("error", "Error en base de datos", ex.Message, "9999");
            // System.Diagnostics.Debug.WriteLine("error" + ex.Message);
            Logueo.Error("[GeneraEstadoCuentaCredito]" + ex.Message + " " + ex.StackTrace);
            return table;
        }

        public void checkNullOrEmptyParameters(List<StoredSrocedureParameter> parametersList, string parameterName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                parametersList.Add(new StoredSrocedureParameter { nameParameter = parameterName, value = value });
            }
        }
        public void checkNullOrEmptyParameters(List<StoredSrocedureParameter> parametersList, string parameterName, string value, bool encrypted, int length)
        {
            if (!string.IsNullOrEmpty(value))
            {
                parametersList.Add(new StoredSrocedureParameter { nameParameter = parameterName, value = value, encrypted = encrypted, length = length });
            }
        }

        public void checkNullOrEmptyParameters(List<StoredSrocedureParameter> parametersList, string parameterName, string value, bool encrypted, int length, SqlDbType type)
        {
            if (!string.IsNullOrEmpty(value))
            {
                parametersList.Add(new StoredSrocedureParameter { nameParameter = parameterName, value = value, encrypted = encrypted, length = length, type = type });
            }
        }
        public void checkNullParametersObjects(List<StoredSrocedureParameter> parametersList, string parameterName, object value)
        {
            //este lo uso porque no puedo hacer un cast to string para validar los valores que no son string ya que si hay un null marcaria error
            if (value != null)
            {
                parametersList.Add(new StoredSrocedureParameter { nameParameter = parameterName, value = value });
            }
        }

    }


    public class StoredSrocedureParameter
    {
        public string nameParameter;
        public object value;
        public bool encrypted = false;
        public int length;
        public SqlDbType type;
    }

    public class StoredProcedureOutputParameters
    {
        public string nameParameter;
        public object value;
        public SqlDbType type;

    }


}
