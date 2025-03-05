using CommonProcesador;
using Interfases.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoAnualidad.dataService
{
    public class DAOEventoExecute
    {
        public SqlConnection _connConsulta;
        public SqlTransaction _transaccionSQL;


        public DAOEventoExecute(SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            _connConsulta = connConsulta;
            _transaccionSQL = transaccionSQL;
        }

        public Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta, String ClaveEvento, String elUsuario, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;




                SqlDatabase database = new SqlDatabase(conn.ConnectionString);//new SqlDatabase("Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43");

                DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneValoresContratos");
                database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
                //parametros always encripted
                SqlParameter paramSSN = (SqlParameter)command.CreateParameter();
                paramSSN.ParameterName = "@Tarjeta";
                paramSSN.DbType = DbType.AnsiStringFixedLength;
                paramSSN.Direction = ParameterDirection.Input;
                paramSSN.Value = Tarjeta;
                paramSSN.Size = string.IsNullOrEmpty(Tarjeta)
                                    ? 50
                                    : Tarjeta.Length;

                command.Parameters.Add(paramSSN);

                laTabla = database.ExecuteDataSet(command, transaccionSQL).Tables[0];

                for (int k = 0; k < laTabla.Rows.Count; k++)
                {
                    Parametro unParamentro = new Parametro();

                    unParamentro.Nombre = (laTabla.Rows[k]["Nombre"]).ToString();
                    unParamentro.ID_TipoColectiva = Int16.Parse((laTabla.Rows[k]["ID_TipoColectiva"]).ToString());
                    unParamentro.Valor = (laTabla.Rows[k]["valor"]).ToString();

                    larespuesta.Add(unParamentro.Nombre, unParamentro);

                }

                return larespuesta;
            }
            catch (Exception ex)
            {

                Logueo.Error(ex.Message);
                throw ex;
            }
        }
    }
}
