using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Entidades;
using Interfases.Entidades;
using Interfases.Enums;
using Interfases.Exceptions;
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
    public class DAOEvento
    {

        public SqlConnection _connConsulta;
        public SqlTransaction _transaccionSQL;
        // public String cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43";

        public DAOEvento(SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            _connConsulta = connConsulta;
            _transaccionSQL = transaccionSQL;
        }

        public int getIDEvento(String ClaveEvento)
        {

            int IDEvento = 0;

            try
            {

                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneIDEvento", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;
                param = new SqlParameter("@ClaveEvento", SqlDbType.VarChar);
                param.Value = ClaveEvento;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Evento", SqlDbType.Int);
                param.Value = 0;
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                //resp = database.ExecuteNonQuery(command);
                comando.CommandTimeout = 5;

                comando.ExecuteNonQuery();

                IDEvento = Convert.ToInt32(comando.Parameters["@ID_Evento"].Value);

                if (IDEvento == 0)
                {
                    throw new GenericalException(CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA, "La Clave del Evento no existe en la Base de Datos.");
                }

                return IDEvento;


            }
            catch (SqlException e)
            {
                Logueo.Error("DaoEvento.getIDEvento(): " + e.Message);
                throw e;
            }
            catch (GenericalException err)
            {
                Logueo.Error("DaoEvento.getIDEvento():" + err.Mensaje);
                throw err;
            }
            catch (Exception e)
            {
                Logueo.Error("DaoEvento. getIDEvento(): " + e.Message);
                throw e;
            }

        }


        public  Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta, String ClaveEvento, String elUsuario, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;




                SqlDatabase database = new SqlDatabase(conn.ConnectionString);//new SqlDatabase("Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43");
               
                DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneValoresContratos");
                database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
                database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);

                laTabla = database.ExecuteDataSet(command,transaccionSQL).Tables[0];

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

        public static Dictionary<String, Parametro> ObtieneDatosOperacion(String Tarjeta, String Autorizacion)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase("Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43");
                DbCommand command = database.GetStoredProcCommand("ws_Quantum_ObtieneDatosOperacion");
                database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);
                database.AddInParameter(command, "@Autorizacion", DbType.String, Autorizacion);

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


        //public static List<Evento> ObtieneEjecucionesEventosAgrupados(Int32 ID_Cadena)
        //{

        //    try
        //    {
        //        List<Evento> larespuesta = new List<Evento>();
        //        DataTable laTabla = null;

        //        SqlDatabase database = new SqlDatabase("Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43");
        //        DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneEjecucionesEventosAgrupados");
        //        database.AddInParameter(command, "@ID_Cadena", DbType.Int32, ID_Cadena);
        //        //database.AddInParameter(command, "@Autorizacion", DbType.String, Autorizacion);

        //        laTabla = database.ExecuteDataSet(command).Tables[0];

        //        for (int k = 0; k < laTabla.Rows.Count; k++)
        //        {
        //            Evento unEvento = new Evento();

        //            unEvento.ID_ConfiguracionCorte =long.Parse((laTabla.Rows[k]["ID_ConfiguracionCorte"]).ToString());
        //            unEvento.ID_AgrupadorEvento = long.Parse((laTabla.Rows[k]["ID_AgrupadorEvento"]).ToString());
        //            unEvento.ID_Evento =Int32.Parse((laTabla.Rows[k]["ID_Evento"]).ToString());
        //            unEvento.ClaveEvento =(laTabla.Rows[k]["ClaveEvento"]).ToString();
        //            unEvento.Descripcion =(laTabla.Rows[k]["Descripcion"]).ToString();
        //            unEvento.ID_CadenaComercial =Int64.Parse((laTabla.Rows[k]["ID_CadenaComercial"]).ToString());
        //            unEvento.ID_Contrato = Int64.Parse((laTabla.Rows[k]["ID_Contrato"]).ToString());
        //            unEvento.ClaveCadenaComercial = (laTabla.Rows[k]["ClaveColectiva"]).ToString();

        //            larespuesta.Add(unEvento);

        //        }

        //        return larespuesta;
        //    }
        //    catch (Exception ex)
        //    {

        //        Logueo.Error(ex.Message);
        //        throw ex;
        //    }
        //}


       static List<EjecutorCadena> ObtieneCadenasEjecuciones()
        {

            try
            {
                List<EjecutorCadena> larespuesta = new List<EjecutorCadena>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase("Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43");
                DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneCadenasPorProcesar");

                //database.AddInParameter(command, "@Autorizacion", DbType.String, Autorizacion);

                laTabla = database.ExecuteDataSet(command).Tables[0];

                for (int k = 0; k < laTabla.Rows.Count; k++)
                {
                    EjecutorCadena unEjecutor = new EjecutorCadena();


                    unEjecutor.ID_ConfiguracionCorte = Int32.Parse((laTabla.Rows[k]["ID_ConfiguracionCorte"]).ToString());
                    unEjecutor.ID_Cadena = Int32.Parse((laTabla.Rows[k]["ID_CadenaComercial"]).ToString());
                    larespuesta.Add(unEjecutor);

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
