using CommonProcesador;
using Dnu_ProcesadorCacaoLogCleaner.Entidades;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace Dnu_ProcesadorCacaoLogCleaner.BaseDatos
{
    public class DAODatosBase
    {
        internal static void EjecutaLimpiezaCacaoLogs(string connectionString)
        {
            try
            {
                List<LogModel> lstlogModel = obtenerListaLogs(connectionString);
                if (lstlogModel != null)
                {
                    var cpunt = lstlogModel.GroupBy(g => g.date_).Count();

                    if (lstlogModel.GroupBy(g => g.date_).Count() > 3)
                    {
                        if (enviarErroresHistorico(connectionString))
                            eliminarLogs(connectionString);
                    }
                }

                
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        private static Boolean eliminarLogs(String connectionString)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    var CommandText = "TRUNCATE TABLE api_log";
                    using (var cmd = new NpgsqlCommand(CommandText, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Logueo.Error(ex.Message);
                return false;
            }
        }

        private static Boolean enviarErroresHistorico(String connectionString)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    var CommandText = "INSERT INTO api_log_history SELECT * FROM api_log where request not like '%200%'";
                    using (var cmd = new NpgsqlCommand(CommandText, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;

            }
            catch(Exception ex)
            {
                Logueo.Error(ex.Message);
                return false;
            }
            
        }

        private static List<LogModel> obtenerListaLogs(string connectionString)
        {
            List<LogModel> lstlogModel = new List<LogModel>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                var CommandText = "SELECT * FROM api_log";
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(CommandText, conn))
                {
                    ds.Reset();
                    da.Fill(ds);
                    dt = ds.Tables[0];
                    conn.Close();
                    foreach (DataRow item in dt.Rows)
                    {
                        lstlogModel.Add(new LogModel
                        {
                            client = item["client"].ToString(),
                            id = Convert.ToInt64(item["id"]),
                            date_ = DateTime.Parse(item["date_"].ToString().Substring(0,11)).ToString("yyyy-MM-dd"),
                            id_fichero = Convert.ToInt64(item["id_fichero"]),
                            ip = item["ip"].ToString(),
                            request = item["request"].ToString(),
                            request_version = item["request_version"].ToString()
                        });
                    }
                }
            }

            return lstlogModel;
        }
    }
}
