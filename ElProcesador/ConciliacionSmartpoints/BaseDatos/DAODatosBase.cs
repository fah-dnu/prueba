using CommonProcesador;
using ConciliacionSmartpoints.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.BaseDatos
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
                            if(losDatos.Tables[0].Rows[k]["TrimCaracteres"] == null)
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

        public static Int32 InsertaDatosEnTabla(DatosBaseDatos laConfig,  Ficheros unFichero)
        {
            Int32 totalRegistros = 0;
            try
            {
                string connectionString = laConfig.CadenaConexion;

                // Open a connection to the AdventureWorks database. 
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlDatabase database = new SqlDatabase(connectionString);
                    // Perform an initial count on the destination table.
                    DbCommand commandRowCount = database.GetStoredProcCommand(laConfig.StoredProcedure);// new DbCommand("proc_InsertaFicheroDetalle", connection);

                    string fecha = FechadeOperaciones(unFichero);
                    commandRowCount.Parameters.Add(new SqlParameter("@Fecha", fecha));// 20200720
                    commandRowCount.Parameters.Add(new SqlParameter("@ID_Fichero", unFichero.IdFichero));
                    commandRowCount.Parameters.Add(new SqlParameter("@ID_Consulta", laConfig.ID_Consulta));

                    // Create a table with some rows. 
                    DataTable losDatos = database.ExecuteDataSet(commandRowCount).Tables[0];

                    totalRegistros = losDatos.Rows.Count;

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(DBProcesadorArchivo.strBDEscrituraArchivo))
                    {
                        bulkCopy.DestinationTableName =
                            "dbo.DatosBD";

                        try
                        {
                            // Write from the source to the destination.
                            bulkCopy.WriteToServer(losDatos);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            catch (Exception err)
            {
            }

            return totalRegistros;
        }

        public static  string FechadeOperaciones(Ficheros unFichero)
        {
            string laFecha = "";            
           try
           {
            string elNombreREal = unFichero.NombreFichero;
            string[] lasPartesNombreARchivo = elNombreREal.Split('_');
            lasPartesNombreARchivo = lasPartesNombreARchivo[4].Split('.');
            laFecha = lasPartesNombreARchivo[0];

           }
           catch (Exception err)
           {
              Logueo.Error("Error al Obtener la Fecha del Nombre del Archivo:" + err);

           }
         return laFecha;
        }

        public static void Proc_InsertaConsultaWhere(string consultaWhere, int idConsulta, int idArchivo)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();
                    SqlCommand command = //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                    new SqlCommand("Proc_InsertaConsultaWhere", conn);
                    //command.CommandText = "proc_InsertaFichero";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@ConsultaWhere", consultaWhere));
                    command.Parameters.Add(new SqlParameter("@IdConsulta", idConsulta));
                    command.Parameters.Add(new SqlParameter("@IdArchivo", idArchivo));

                    command.ExecuteNonQuery();
                    conn.Close();
                }


            }
            catch (Exception err)
            {
                Logueo.Error("ProcesaArch(): ERROR AL GUARDAR EL ERROR EN BD DEL Archivo " + idArchivo + " " + err.Message);

            }
        }
    }
}
