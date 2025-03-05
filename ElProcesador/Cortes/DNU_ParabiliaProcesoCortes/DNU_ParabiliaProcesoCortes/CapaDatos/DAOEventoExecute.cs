using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Common.Funciones;
using DNU_ParabiliaProcesoCortes.dataService;
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

        public DAOEventoExecute(SqlConnection connConsulta)
        {
            _connConsulta = connConsulta;
        }
        public Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta, String ClaveEvento, String elUsuario, SqlConnection conn = null, SqlTransaction transaccionSQL = null, string conexionBD = null)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;




                SqlDatabase database = new SqlDatabase(conexionBD);//conn.ConnectionString);//new SqlDatabase("Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43");

                DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneValoresContratos");
                database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
                //database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);
                SqlParameter paramSSN = FuncionesAzure.obtenerParametroSQL("@Tarjeta", Tarjeta, Constantes.longitudMedioAcceso, SqlDbType.VarChar);
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

        public Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta, String ClaveEvento, String elUsuario, SqlConnection conn = null)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

               
                String conexion = conn.ConnectionString;
                Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo paraetros"+ conexion);
                SqlDatabase database = new SqlDatabase(conexion);//new SqlDatabase("Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43");

                DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneValoresContratos");
                database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
               // database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);
                SqlParameter paramSSN = FuncionesAzure.obtenerParametroSQL("@Tarjeta", Tarjeta, Constantes.longitudMedioAcceso, SqlDbType.VarChar);
                command.Parameters.Add(paramSSN);


                laTabla = database.ExecuteDataSet(command).Tables[0];

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

        public Dictionary<String, Parametro> ListaDeParamentrosContratoCliente(String ClaveCadenaComercial, String Tarjeta, String ClaveEvento, String elUsuario, SqlConnection conn = null)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;



                string conexion= PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizador");//ConfigurationManager.AppSettings["DBEmpleadosRead"].ToString();// Config.GetValor("ALTAEMPLEADOS", "strBDLectura");

                SqlDatabase database = new SqlDatabase(conexion);//DBProcesadorArchivo.strBDLecturaAutorizador);//new SqlDatabase("Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43");

                DbCommand command = database.GetStoredProcCommand("ws_ejecutor_ObtieneValoresContratosClientes");
                database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
                //// database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);
                //SqlParameter paramSSN = FuncionesAzure.obtenerParametroSQL("@Tarjeta", Tarjeta, Constantes.longitudMedioAcceso, SqlDbType.VarChar);
                //command.Parameters.Add(paramSSN);


                laTabla = database.ExecuteDataSet(command).Tables[0];

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
