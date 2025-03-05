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
    class DAOTagTelevia
    {


        public static Boolean ProcesaDispersionDelTag(Premio elPremio, SqlConnection DBConexion)
        {


            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("TVIP_InsertaDispersion", DBConexion);
                comando.CommandType = CommandType.StoredProcedure;
                // comando.Transaction = transaccionSQL;

                //param = new SqlParameter("@Tag", SqlDbType.VarChar);
                //param.Value = elPremio.ID_Recompensa;
                //comando.Parameters.Add(param);

                param = new SqlParameter("@id_motivo", SqlDbType.VarChar);
                param.Value = elPremio.id_motivo;
                comando.Parameters.Add(param);

                param = new SqlParameter("@id_tag", SqlDbType.VarChar);
                param.Value = elPremio.id_tag;
                comando.Parameters.Add(param);

                param = new SqlParameter("@importe", SqlDbType.Float);
                param.Value = elPremio.importe;
                comando.Parameters.Add(param);

                param = new SqlParameter("@f_ingreso", SqlDbType.DateTime);
                param.Value = elPremio.f_ingreso;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Recompensa", SqlDbType.BigInt);
                param.Value = elPremio.ID_Recompensa;
                comando.Parameters.Add(param);

                // comando.CommandTimeout = 5;

                comando.ExecuteNonQuery();


                return true;
            }
            catch (Exception ex)
            {

                Logueo.Error(ex.Message);
                return false;
            }
        }



        public static List<Premio> ObtenerPremiosParaSincronizar()
        {
            List<Premio> losNuevosTags = new List<Premio>();


            try
            {
                //  Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDTelevip.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("TVIP_ConsultaParaSincronicar");


                laTabla = database.ExecuteDataSet(command).Tables[0];

                foreach (DataRow renglon in laTabla.Rows)
                {
                    Premio unPremio = new Premio();

                    unPremio.ID_Recompensa = Int64.Parse(renglon["ID_Recompensa"].ToString());

                    unPremio.id = Int64.Parse(renglon["id"].ToString());

                    if (renglon["f_aplicacion"].ToString().Length!=0)
                    {
                        unPremio.f_aplicacion = DateTime.Parse(renglon["f_aplicacion"].ToString());
                    }else
                    {
                        unPremio.f_aplicacion = DateTime.Now.AddDays(2);
                    }
                   

                    if (renglon["status"].ToString().ToString().Length != 0)
                    {
                        unPremio.status = Int32.Parse(renglon["status"].ToString());
                    }

                    if (renglon["id_motivo"].ToString().ToString().Length != 0)
                    {
                        unPremio.id_motivo = Int32.Parse(renglon["id_motivo"].ToString());
                    }

                    if (renglon["num_intentos"].ToString().ToString().Length != 0)
                    {
                        unPremio.intentos = Int32.Parse(renglon["num_intentos"].ToString());
                    }

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


        public static Boolean IndicarPremioSincronizado(Int64 ID)
        {


            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("TVIP_SetPremioSincronizado", BDAutorizador.BDEscritura);
                comando.CommandType = CommandType.StoredProcedure;


                param = new SqlParameter("@ID", SqlDbType.BigInt);
                param.Value = ID;
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

    }
}
