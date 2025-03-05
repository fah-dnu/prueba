using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using CommonProcesador;



namespace dataServices_Class.Acceso_a_datos
{
    public class acceso_aDatos
    {

        //private string ConnectionString = "Data Source=.;Initial Catalog=BusinessDevelopment;User Id=iis;Password=testing;";
        public SqlConnection DataConnection;
        public SqlDatabase database;
        //public acceso_aDatos(string ConnectionString)
        //{
        //  DataConnection = new SqlConnection(ConnectionString);
        // }


        public acceso_aDatos()
        {
            //   DataConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Autorizador_Diconsa"].ConnectionString);
        }
        public acceso_aDatos(SqlConnection connConsulta, SqlDatabase database_)
        {
            DataConnection = connConsulta;
            database = database_;
        }

    public int ExecuteSqlCommand(string SqlCommandText)
        {
            SqlCommand cmd = new SqlCommand(SqlCommandText, DataConnection);
            cmd.CommandType = CommandType.Text;
            cmd.Connection.Open();
            int recordsAffected = cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            return recordsAffected;
        }


        public object ExecuteSqlScalar(string SqlCommandText)
        {
            SqlCommand cmd = new SqlCommand(SqlCommandText, DataConnection);
            cmd.CommandType = CommandType.Text;
            cmd.Connection.Open();
            object result = cmd.ExecuteScalar();


            cmd.Connection.Close();
            return result;
        }


        public int ExecuteSqlProcedure(string SqlProcedure)
        {
            SqlCommand cmd = new SqlCommand(SqlProcedure, DataConnection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection.Open();
            //add a parameter:
            //SqlParameter param = execproc.Parameters.Add("@Id", SqlDbType.Int);
            //param.Value = 100;
            int recordsAffected = cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            return recordsAffected;
        }







        public DataTable ExecuteSqlProcedureSelect(String SqlProcedure)
        {
            DataTable _valueTable = new DataTable();

            SqlCommand cmd = new SqlCommand(SqlProcedure, DataConnection);

            try
            {

                cmd.CommandType = CommandType.Text;
                cmd.Connection.Open();


                cmd.CommandTimeout = 0;


                SqlDataReader dr = cmd.ExecuteReader();

                _valueTable.Load(dr);

            }
            catch (Exception e)
            {

                throw e;
            }
            finally
            {

                cmd.Connection.Close();
            }
            return _valueTable;

        }








    //    string cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43; MultipleActiveResultSets=true;";
    //            using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
    //            {

    //                conn2.Open();

                

    //                using (SqlTransaction transaccionSQL = conn2.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
    //                {

    //                    Parametros = new List<Parametro>();
    //                    SqlParameter param;
    //SqlCommand comando2 = new SqlCommand("EJECUTOR_ObtieneParametrosdeStoredProcedure", conn2);
    //comando2.Transaction = transaccionSQL;
    //                    comando2.CommandType = CommandType.StoredProcedure;
         

        public System.Data.Common.DbDataReader ExecuteSqlDataReader(string SqlSelectCommand,
            Action<SqlDatabase, System.Data.Common.DbCommand, object, object, object> code,

            object param1, object param2, object param3)
        {

            string cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43";

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
            conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    SqlDatabase database = new SqlDatabase(cadenaConexion);
                    System.Data.Common.DbCommand command = database.GetStoredProcCommand(SqlSelectCommand);

                    command.Transaction = transaccionSQL;
                    command.CommandType = CommandType.StoredProcedure;

                    code(database, command, param1, param2, param3);



                    return command.ExecuteReader();
                }
            }
        }










        //public object GetSqlScalar(string SqlSelectCommand)
        //{
        //    //return first row first column
        //    return GetSqlDataTable(SqlSelectCommand).Rows[0][0];
        //}





        public DataSet GetSqlDataSet(String CadenaConexion, string SqlSelectCommand,
            Action<SqlDatabase, System.Data.Common.DbCommand, object, object, object, object, object, object, object, object, object, object, object, object, object> code,

            object param1, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, 
            object param10, object param11, object param12, object param13)
        {
           


                // DataSet ds = new DataSet();
                // SqlDataAdapter sda = new SqlDataAdapter(SqlSelectCommand, DataConnection);
                // sda.Fill(ds);
                // return ds;
                // string cadenaConexion = ConfigurationManager.ConnectionStrings["Autorizador_Diconsa"].ConnectionString;

                //  ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["Autorizador_Diconsa"];
              //  string cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43";


              //  using (SqlConnection conn = new SqlConnection(cadenaConexion))

                //   using (SqlConnection conn = new SqlConnection("aaa"))
                {

                    SqlDatabase database = new SqlDatabase(CadenaConexion);
                    //System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneCadenasPorProcesar");
                    System.Data.Common.DbCommand command = database.GetStoredProcCommand(SqlSelectCommand);

                    code(database, command, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13);


                    // database.AddInParameter(command, "@ID_Cadena", DbType.Int32, ID_Cadena);
                    // database.AddInParameter(command, "@Autorizacion", DbType.String, Autorizacion);

                    // laTabla = database.ExecuteDataSet(command).Tables[0];




                    return database.ExecuteDataSet(command);

                }

          
        }





        //public DataTable GetSqlDataTable(string SqlSelectCommand)
        //{
        //    return GetSqlDataSet(SqlSelectCommand).Tables[0];
        //}

        public static bool ColumnExists(DataTable Table, string ColumnName)
        {
            bool bRet = false;
            foreach (DataColumn col in Table.Columns)
            {
                if (col.ColumnName.ToLower() == ColumnName.ToLower())
                {
                    bRet = true;
                    break;
                }
            }

            return bRet;
        }







    }
}
