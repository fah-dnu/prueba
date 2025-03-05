using CommonProcesador;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TELEVIP_EnviarDispersionPremios.Entidades;
using TELEVIP_ImportaTagsAfiliados.Entidades;

namespace TELEVIP_ImportaTagsAfiliados.BaseDatos
{
    class DAOTagDNU
    {


        public static Boolean IndicarPremioDispersado(Int64 ID_REcompensa, SqlConnection unaConexion)
        {


            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("ProcNoct_Televia_SetRecompensaEnviada", unaConexion);
                comando.CommandType = CommandType.StoredProcedure;


                param = new SqlParameter("@ID_REcompensa", SqlDbType.BigInt);
                param.Value = ID_REcompensa;
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


        public static Boolean ActualizarEstatusRecompensa(Int64 ID_Recompensa, Int32 Intentos, Int32 ID_NuevoEstatus, Int32 ID_Motivo, DateTime FechaAplicacion)
        {


            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("ProcNoct_Televia_ActualizarEstatusRecompensa", BDAutorizador.BDEscritura);
                comando.CommandType = CommandType.StoredProcedure;


                param = new SqlParameter("@ID_Corte", SqlDbType.BigInt);
                param.Value = ID_Recompensa;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_EstatusNuevo", SqlDbType.Int);
                param.Value = ID_NuevoEstatus;
                comando.Parameters.Add(param);

                  param = new SqlParameter("@FechaAplicacion", SqlDbType.DateTime);
                param.Value = FechaAplicacion;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Intentos", SqlDbType.Int);
                param.Value = Intentos;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Motivo", SqlDbType.Int);
                param.Value = ID_Motivo;
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

     
        public static List<Premio> ObtenerTagsParaDispersion()
        {
            List<Premio> losNuevosTags = new List<Premio>();


            try
            {
                //  Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_Televia_ObtieneDispersiones");


                laTabla = database.ExecuteDataSet(command).Tables[0];

                foreach (DataRow renglon in laTabla.Rows)
                {
                    Premio unPremio = new Premio();

                    unPremio.ID_Recompensa = Int64.Parse(renglon["id"].ToString());
                    unPremio.id_tag = (renglon["id_tag"].ToString());
                    unPremio.importe = float.Parse(renglon["importe"].ToString());
                    unPremio.f_ingreso = DateTime.Parse(renglon["f_ingreso"].ToString());
                    unPremio.f_aplicacion = DateTime.Parse(renglon["f_aplicacion"].ToString());
                    unPremio.status = Int32.Parse(renglon["status"].ToString());
                    unPremio.id_motivo = Int32.Parse(renglon["id_motivo"].ToString());
                    //unPremio.enviada = true;

                    losNuevosTags.Add(unPremio);
                }

                return losNuevosTags;
            }
            catch (Exception ex)
            {

                Logueo.Error("ObtenerTagsParaDispersion()" + ex.Message);
                throw ex;
            }
        }



        public static void Televia_InsertarEncabezadoRecompensa()
        {


            try
            {
                SqlParameter param;

                using (SqlConnection laCadena = BDAutorizador.BDLectura)
                {
                    laCadena.Open();

                    SqlCommand comando = new SqlCommand("ProcNoct_ejecutor_Televia_InsertarEncabezadoRecompensa", laCadena);
                    comando.CommandType = CommandType.StoredProcedure;


                    //param = new SqlParameter("@ID_Corte", SqlDbType.BigInt);
                    //param.Value = ID_Corte;
                    //comando.Parameters.Add(param);

                    comando.ExecuteNonQuery();
                }

            }
            catch (SqlException e)
            {
                Logueo.Error("[EJECUTOR]Televia_ActualizaPolizaEnCorte(): " + e.Message);
                throw e;
            }
            catch (Exception e)
            {
                Logueo.Error("[EJECUTOR]Televia_ActualizaPolizaEnCorte(): " + e.Message);
                throw e;
            }

        }


    }
}
