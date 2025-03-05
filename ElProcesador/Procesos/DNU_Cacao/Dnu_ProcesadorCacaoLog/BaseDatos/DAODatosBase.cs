using CommonProcesador;
using Dnu_ProcesadorCaCaoLog.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace Dnu_ProcesadorCaCaoLog.BaseDatos
{
    public class DAODatosBase
    {

        public static DatosBaseDatos ObtenerConsulta(Int64 ID_Archivo)
        {

            DatosBaseDatos unaConsulta = new DatosBaseDatos();
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneConfigConsulta");

                command.Parameters.Add(new SqlParameter("@ID_Archivo", ID_Archivo));



                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {


                        unaConsulta.CadenaConexion = (String)losDatos.Tables[0].Rows[k]["CadenaConexion"];
                        unaConsulta.Clave = (String)losDatos.Tables[0].Rows[k]["Clave"];
                        unaConsulta.ID_Consulta = (Int64)losDatos.Tables[0].Rows[k]["ID_Consulta"];
                        unaConsulta.Descripcion = (String)losDatos.Tables[0].Rows[k]["DescripcionConsulta"];
                        unaConsulta.StoredProcedure = (String)losDatos.Tables[0].Rows[k]["StoredProcedure"];


                        //unaConsulta.losDatosdeBD = ObtenerConfiguracionFila(ID_Consulta);
                        for (int i = 0; unaConsulta.ID_Consulta == (Int64)losDatos.Tables[0].Rows[k]["ID_Consulta"]; i++)
                        {
                            //if (losDatos.Tables[0].Rows.Count == (i))
                            //{
                            //    break;
                            //}

                            CampoConfig unaConfiguracion = new CampoConfig();

                            unaConfiguracion.Descripcion = (String)losDatos.Tables[0].Rows[k]["DescripcionCampoConsulta"];
                            unaConfiguracion.EsClave = (Boolean)losDatos.Tables[0].Rows[k]["EsClave"];
                            unaConfiguracion.ID_TipoColectiva = (Int32)losDatos.Tables[0].Rows[k]["ID_TipoColectiva"];
                            unaConfiguracion.Nombre = (String)losDatos.Tables[0].Rows[k]["Nombre"];
                            unaConfiguracion.Posicion = (Int32)losDatos.Tables[0].Rows[k]["Posicion"];
                            unaConfiguracion.TrimCaracteres = (String)losDatos.Tables[0].Rows[k]["TrimCaracteres"];


                            //unCampoConfFila.LosCampos.Add(unaConfiguracion.Posicion, unaConfiguracion);

                            unaConsulta.losDatosdeBD.Add(unaConfiguracion.Posicion, unaConfiguracion);

                            k++;

                            if (losDatos.Tables[0].Rows.Count == (k))
                            {
                                break;
                            }
                        }


                    }
                }

                return unaConsulta;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                // throw new Exception(ex.Message);
                return unaConsulta;
            }
        }


        public static Int32 InsertaDatosEnTabla(DatosBaseDatos laConfig, Int64 ID_Fichero, Archivo unArchivo)
        {
            var total = 0;
            try
            {
                string connectionString = laConfig.CadenaConexion;

                foreach (var item in unArchivo.LosDatos)
                {
                    if (String.IsNullOrEmpty(item.DetalleCrudo))
                        continue;

                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        var CommandText =
                        String.Format("INSERT INTO api_log (id_fichero,ip,client,date_,request,request_version) " +
                        "VALUES({0},'{1}','{2}','{3}','{4}','{5}')",
                       ID_Fichero,
                       item.losCampos[1].ToString(),
                       item.losCampos[2].ToString(),
                       item.losCampos[3].ToString(),
                       item.losCampos[4].ToString(),
                       item.losCampos[5].ToString());
                        using (var cmd = new NpgsqlCommand(CommandText, conn))
                        {

                            conn.Open();
                            total += cmd.ExecuteNonQuery();

                        }
                    }
                }
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
            return total;
        }

    }
}
