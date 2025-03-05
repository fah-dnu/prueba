using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Utilidades;
using Interfases.Entidades;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.CapaDatos
{
    class DAOEventoExecute
    {
        public SqlConnection _connConsulta;
        public SqlTransaction _transaccionSQL;
        // public String cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43";

        public DAOEventoExecute(SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            _connConsulta = connConsulta;
            _transaccionSQL = transaccionSQL;
        }
        public Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta, String ClaveEvento, String elUsuario, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;




                SqlDatabase database = new SqlDatabase(conn.ConnectionString);//new SqlDatabase("Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43");

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_ObtieneValoresContratos]");
                DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneValoresContratos");
                database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
                database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);

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
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                throw ex;
            }
        }


    }
}
