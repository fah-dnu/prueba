using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfases.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data;
using System.Data.Common;
using CommonProcesador;
using Interfases.Exceptions;
using Interfases.Enums;
using System.Data.SqlClient;

namespace QUANTUM_AplicarMovimientos.BaseDatos
{
    public class DAOEvento
    {

        SqlConnection _connConsulta;
        SqlTransaction _transaccionSQL;

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


        public static Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial,String Tarjeta, String ClaveEvento, String elUsuario)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDAplicarMov.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ws_Quantum_ObtieneValoresContratos");
                database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
                database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);

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

        public static Dictionary<String, Parametro> ObtieneDatosOperacion( String Tarjeta, String Autorizacion)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDAplicarMov.strBDLectura);
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
 

    }
}
