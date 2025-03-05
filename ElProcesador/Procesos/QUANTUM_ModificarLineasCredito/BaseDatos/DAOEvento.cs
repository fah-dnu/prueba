using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfases.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data;
using System.Data.Common;
using CommonProcesador;
using QUANTUM_ModificarLineasCredito.Entidades;
using System.Data.SqlClient;
using Interfases.Exceptions;
using Interfases.Enums;

namespace QUANTUM_ModificarLineasCredito.BaseDatos
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


        public static Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta, String ClaveEvento, String elUsuario)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDModificarLC.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ws_Quantum_ObtieneValoresContratos");
                database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
                database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);

                laTabla = database.ExecuteDataSet(command).Tables[0];

                for (int k = 0; k < laTabla.Rows.Count; k++)
                {
                    Parametro unParamentro = new Parametro();

                    unParamentro.Nombre = (laTabla.Rows[k]["Nombre"]).ToString().Trim();
                    unParamentro.ID_TipoColectiva = Int16.Parse((laTabla.Rows[k]["ID_TipoColectiva"]).ToString().Trim());
                    unParamentro.Valor = (laTabla.Rows[k]["valor"]).ToString().Trim();

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


        public static Respuesta AsignarVigencia(String Tarjeta, int DiasVigencia , String Usuario,SqlConnection conn, SqlTransaction _transaccionSQL )
        {

            SqlDataReader Reader=null;
            Respuesta laRespuesta = new Respuesta();
            try
            {


                SqlParameter param = null;


                SqlCommand comando = new SqlCommand("ws_Quantum_DefinirVigencia", conn);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;
                int resp = -1;

                param = new SqlParameter("@Tarjeta", SqlDbType.VarChar);
                param.Value = Tarjeta;
                comando.Parameters.Add(param);

                param = new SqlParameter("@DiasVigencia", SqlDbType.Int);
                param.Value = DiasVigencia;
                comando.Parameters.Add(param);



                Reader = comando.ExecuteReader();

                if (null != Reader)
                {
                    while (Reader.Read())
                    {

                        laRespuesta.Saldo = Decimal.Parse(Reader["Saldo"].ToString());
                        laRespuesta.Tarjeta = Tarjeta;
                        laRespuesta.Autorizacion = Reader["Autorizacion"].ToString();
                        laRespuesta.CodigoRespuesta = Int32.Parse(Reader["CodigoRespuesta"].ToString());

                    }
                }


                Logueo.Evento("Se Asginó la vigencia a las cuentas de la Tarjeta y sus cuentas:" + Tarjeta);
                return laRespuesta;

            }
            catch (Exception err)
            {
                throw err;
            }
            finally
            {
                Reader.Close();
            }
        }


        public static List<LineaCredito> ListaUsuariosParaLC()
        {
            List<LineaCredito> lasNuevasLineas = new List<LineaCredito>();

            
            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDModificarLC.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ws_Quantum_ObtieneUsuariosParaLimiteCredito");


                laTabla = database.ExecuteDataSet(command).Tables[0];

                foreach (DataRow renglon in laTabla.Rows)
                {
                    LineaCredito unaLineaCredito = new LineaCredito();

                    unaLineaCredito.Consecutivo = renglon["ID"].ToString();
                    unaLineaCredito.ID_Cliente = renglon["NoCliente"].ToString();
                    unaLineaCredito.LimiteCredito = (Decimal)renglon["Capacidad"];
                    unaLineaCredito.Tarjeta = renglon["Tarjeta"].ToString();
                    unaLineaCredito.DiasVigencia = (int)renglon["Vigencia"];



                    lasNuevasLineas.Add(unaLineaCredito);
                }

                return lasNuevasLineas;
            }
            catch (Exception ex)
            {

                Logueo.Error("ListaUsuariosParaLC()" + ex.Message);
                throw ex;
            }
        }

        public static Boolean IndicarProcesado(Boolean Procesado, String Tarjeta)
        {
            

            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("ws_Quantum_SetProcesadoLimiteCredito", BDModificarLC.BDEscritura);
                comando.CommandType = CommandType.StoredProcedure;


                param = new SqlParameter("@Procesado", SqlDbType.Bit);
                param.Value = Procesado;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Tarjeta", SqlDbType.VarChar);
                param.Value = Tarjeta;
                comando.Parameters.Add(param);

               
                comando.CommandTimeout = 5;

                comando.ExecuteNonQuery();


                return true;
            }
            catch (Exception ex)
            {

                Logueo.Error(ex.Message);
                throw ex;
            }
        }
 
    }
}
