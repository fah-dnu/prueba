using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfases.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data;
using System.Data.Common;
using CommonProcesador;
using QUANTUM_NuevoCliente.Entidades;
using System.Data.SqlClient;

namespace QUANTUM_NuevoCliente.BaseDatos
{
    public class DAOCliente
    {

        public static Cliente AgregarCliente(Cliente elCliente, int IdTipoColectiva, String Sucursal, SqlConnection DBConexion, SqlTransaction transaccionSQL)
        {
            
            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("ws_Quantum_CrearUsuario", DBConexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;


                int resp = -1;
                param = new SqlParameter("@AMaterno", SqlDbType.VarChar);
                param.Value = elCliente.AMaterno;
                comando.Parameters.Add(param);

                param = new SqlParameter("@APaterno", SqlDbType.VarChar);
                param.Value = elCliente.APaterno;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Email", SqlDbType.VarChar);
                param.Value = elCliente.Email;
                comando.Parameters.Add(param);

                param = new SqlParameter("@FechaNacimiento", SqlDbType.DateTime);
                param.Value = elCliente.FechaNacimiento;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Movil", SqlDbType.VarChar);
                param.Value = elCliente.Movil;
                comando.Parameters.Add(param);

                param = new SqlParameter("@NoCliente", SqlDbType.VarChar);
                param.Value = elCliente.ID_Cliente;
                comando.Parameters.Add(param);

                param = new SqlParameter("@NombreORazonSocial", SqlDbType.VarChar);
                param.Value = elCliente.NombreORazonSocial;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Password", SqlDbType.VarChar);
                param.Value = elCliente.Password;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Telefono", SqlDbType.VarChar);
                param.Value = elCliente.Telefono;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Sucursal", SqlDbType.VarChar);
                param.Value = Sucursal;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Id_TipoColectiva", SqlDbType.BigInt);
                param.Value = IdTipoColectiva;
                comando.Parameters.Add(param);


                param = new SqlParameter("@ID_Colectiva", SqlDbType.BigInt);
                param.Direction = ParameterDirection.Output;
                param.Value = 0;
                comando.Parameters.Add(param);


                comando.ExecuteNonQuery();

                elCliente.ID_Colectiva = (Int64)comando.Parameters["@ID_Colectiva"].Value;

                return elCliente;
            }
            catch (Exception ex)
            {

                Logueo.Error(ex.Message);
                throw ex;
            }
        }


        public static Boolean GenerarCuentas(Cliente elCliente,String ClaveCadenaComercial, String Tarjeta,  String Track2, String TipoCuenta, SqlConnection DBConexion, SqlTransaction transaccionSQL)
        {

            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("ws_Quantum_CrearCuentas", DBConexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;


                int resp = -1;
                param = new SqlParameter("@ID_Colectiva", SqlDbType.BigInt);
                param.Value = elCliente.ID_Colectiva;
                comando.Parameters.Add(param);

                param = new SqlParameter("@TipoCuenta", SqlDbType.VarChar);
                param.Value = TipoCuenta;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Tarjeta", SqlDbType.VarChar);
                param.Value = Tarjeta;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ClaveCadenaComercial", SqlDbType.VarChar);
                param.Value = ClaveCadenaComercial;
                comando.Parameters.Add(param);

                comando.ExecuteNonQuery();

                return true;
               
            }
            catch (Exception ex)
            {

                Logueo.Error(ex.Message);
                throw ex;
            }
        }



        public static List<Cliente> ListaUsuariosParaLC()
        {
            List<Cliente> lasNuevasLineas = new List<Cliente>();


            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDNuevoCliente.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ws_Quantum_ObtieneUsuariosParaLimiteCredito");


                laTabla = database.ExecuteDataSet(command).Tables[0];

                foreach (DataRow renglon in laTabla.Rows)
                {
                    Cliente unCliente = new Cliente();

                    //unCliente.Consecutivo = renglon["ID"].ToString();
                    //unCliente.ID_Cliente = renglon["NoCliente"].ToString();
                    //unCliente.LimiteCredito = (Decimal)renglon["Capacidad"];
                    //unCliente.Tarjeta = renglon["Tarjeta"].ToString();
                    //unCliente.DiasVigencia = (int)renglon["Vigencia"];
                  
                    unCliente.Consecutivo = renglon["ID"].ToString();
                    unCliente.ID_Cliente = renglon["NoCliente"].ToString();
                    unCliente.NombreORazonSocial = renglon["NOmbre"].ToString();
                    unCliente.APaterno = renglon["Paterno"].ToString();
                    unCliente.AMaterno = renglon["Materno"].ToString();
                    unCliente.FechaNacimiento = (DateTime)renglon["FechaNacimiento"];
                    unCliente.Tarjeta = renglon["Tarjeta"].ToString();
                   // unCliente.ClaveSucursal = renglon["SUCURSAL"] == null ? "" : renglon["SUCURSAL"].ToString().ToString();
                    unCliente.Telefono = renglon["Telefono"].ToString();
                    unCliente.ClaveSucursal = "";

                    unCliente.LimiteCredito = (Decimal)renglon["Capacidad"];
                    unCliente.DiasVigencia = (int)renglon["Vigencia"];
                    


                    lasNuevasLineas.Add(unCliente);
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

                SqlCommand comando = new SqlCommand("ws_Quantum_SetProcesadoLimiteCredito", BDNuevoCliente.BDEscritura);
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
