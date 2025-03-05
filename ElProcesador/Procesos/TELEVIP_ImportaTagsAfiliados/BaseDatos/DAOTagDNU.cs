using CommonProcesador;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TELEVIP_ImportaTagsAfiliados.Entidades;

namespace TELEVIP_ImportaTagsAfiliados.BaseDatos
{
    class DAOTagDNU
    {


        public static Boolean IndicarProcesado(Boolean Procesado, String unTag, String elID_Cuenta)
        {


            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("ProcNoct_Televia_SetRegistroProcesado", BDAutorizador.BDEscritura);
                comando.CommandType = CommandType.StoredProcedure;


                param = new SqlParameter("@Tag", SqlDbType.VarChar);
                param.Value = unTag;
                comando.Parameters.Add(param);

                param = new SqlParameter("@IDCuenta", SqlDbType.VarChar);
                param.Value = elID_Cuenta;
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

        public static Boolean InsertarRegistro(Tag elNuevoTag)
        {


            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("ProcNoct_Televia_InsertaRegistroNuevoTag", BDAutorizador.BDEscritura);
                comando.CommandType = CommandType.StoredProcedure;

                param = new SqlParameter("@id_tag", SqlDbType.VarChar);
                param.Value = elNuevoTag.ID_Tag;
                comando.Parameters.Add(param);
                
                param = new SqlParameter("@id_cuenta", SqlDbType.VarChar);
                param.Value = elNuevoTag.ID_cuentaTelevia;
                comando.Parameters.Add(param);

                param = new SqlParameter("@FechaAlta", SqlDbType.DateTime);
                param.Value = elNuevoTag.Fecha_Alta;
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

        public static Boolean ProcesaImportacionDelTag(Tag elNuevoTag, SqlConnection DBConexion, SqlTransaction transaccionSQL)
        {


            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("ProcNoct_Televia_ProcesaImportacionNuevoTag", DBConexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;

                param = new SqlParameter("@Tag", SqlDbType.VarChar);
                param.Value = elNuevoTag.ID_Tag;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Cuenta", SqlDbType.VarChar);
                param.Value = elNuevoTag.ID_cuentaTelevia;
                comando.Parameters.Add(param);

                comando.CommandTimeout = 5;

                comando.ExecuteNonQuery();


                return true;
            }
            catch (Exception ex)
            {

                Logueo.Error(ex.Message);
                return false;
            }
        }


        public static List<Tag> ObtenerNuevosTagsAfiliadosSinProcesar()
        {
            List<Tag> losNuevosTags = new List<Tag>();


            try
            {
                //  Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_Televia_ObtieneTagsPorProcesar");


                laTabla = database.ExecuteDataSet(command).Tables[0];

                foreach (DataRow renglon in laTabla.Rows)
                {
                    Tag unTag = new Tag();

                    unTag.ID_Tag = (renglon["id_tag"].ToString());
                    unTag.ID_cuentaTelevia = renglon["id_cuenta"] == null ? "" : renglon["id_cuenta"].ToString();
                    unTag.Importado = Boolean.Parse(renglon["Importado"].ToString());

                    losNuevosTags.Add(unTag);
                }

                return losNuevosTags;
            }
            catch (Exception ex)
            {

                Logueo.Error("ObtenerNuevosTagsAfiliadosSinProcesar()" + ex.Message);
                throw ex;
            }
        }

 
        public static  bool ImportarTagCuenta (String laCuenta )
        {
            try
            {

                  using (SqlConnection conn = BDAutorizador.BDEscritura)
                    {
                        conn.Open();

                        using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                        {
                            try
                            {
                              

                                //IMPORTAR LOS TAGS DEL USUARIO
                                elUser.TAGS = ObtieneTagsDeCuenta(laPeticion);
                                elUser.FechaNacimiento.Valor = laPeticion.FechaNacimiento;
                                elUser.Genero.Valor = laPeticion.Genero;
                                elUser.Cuenta.Valor = laPeticion.CuentaTelevia;

                                ElFlujo += "17,";
                                foreach (Tag unTag in elUser.TAGS)
                                {
                                    ElFlujo += "17.1, [" + unTag.ClaveTag + "],";
                                    TagImportacion elNuevoTag = new TagImportacion(unTag.ClaveTag);
                                    elNuevoTag.CuentaTelevia = laNuevaColectiva.Cuenta;
                                    DAOTag.ProcesaImportacionDelTag(elNuevoTag, laPeticion.CuentaTelevia, conn, transaccionSQL);
                                }
                                ElFlujo += "18,";
                                transaccionSQL.Commit();

                             

                            } catch ( Exception err)
                            {
                                transaccionSQL.Rollback();
                                Logueo.Error("LNTeleviaServer.ModificarUsuario()" + err.Message);
                                throw new Exception("NO SE MODIFICO NINGUN USUARIO: " + err.Message);

                            }
                        }

                    }
                           

                return true

            }catch (Exception err)
            {
                return false;
            }
        }


    }
}
