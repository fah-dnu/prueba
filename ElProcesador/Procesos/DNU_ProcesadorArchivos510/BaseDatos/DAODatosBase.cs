using CommonProcesador;
using DNU_ProcesadorArchivos.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace DNU_ProcesadorArchivos.BaseDatos
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


        public static Datos ObtieneDatosFromConexion(DatosBaseDatos laConfig)
        {

            Datos losDatos = new Datos();

            try
            {
                SqlDatabase database = new SqlDatabase(laConfig.CadenaConexion);
                DbCommand command = database.GetStoredProcCommand(laConfig.StoredProcedure);

                command.Parameters.Add(new SqlParameter("@Fecha", DateTime.Now));


                //return database.ExecuteDataSet(command);
                DataSet losDatosleidos = (DataSet)database.ExecuteDataSet(command);

                if (losDatosleidos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatosleidos.Tables[0].Rows.Count; k++)
                    {
                        Fila nuevaFila = new Fila();

                        for (int i = 0; i < losDatosleidos.Tables[0].Columns.Count; i++)
                        {
                            nuevaFila.losCampos.Add(i + 1, losDatosleidos.Tables[0].Rows[k][i].ToString());
                        }

                        losDatos.LosDatos.Add(nuevaFila);
                    }
                }
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }

            return losDatos;
        }


        public static Int32 InsertaDatosEnTabla(DatosBaseDatos laConfig, Int64 ID_Fichero, Archivo unArchivo)
        {
            Int32 totalRegistros = 0;
            try
            {
                string connectionString = laConfig.CadenaConexion;
                
                // Open a connection to the AdventureWorks database. 
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlDatabase database =new SqlDatabase(connectionString);
                    // Perform an initial count on the destination table.
                    DbCommand commandRowCount = database.GetStoredProcCommand(laConfig.StoredProcedure);// new DbCommand("proc_InsertaFicheroDetalle", connection);

                    commandRowCount.Parameters.Add(new SqlParameter("@Fecha", unArchivo.FechadeOperaciones));
                    commandRowCount.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                    commandRowCount.Parameters.Add(new SqlParameter("@ID_Consulta", laConfig.ID_Consulta));
                    commandRowCount.Parameters.Add(new SqlParameter("@ID_CadenaComercial", unArchivo.ID_ColectivaCCM));

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
    }
}
