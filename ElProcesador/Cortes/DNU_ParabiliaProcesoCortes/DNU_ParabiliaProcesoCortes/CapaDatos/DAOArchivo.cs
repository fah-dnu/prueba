using CommonProcesador;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.CapaDatos
{
    class DAOArchivo
    {

        public static List<ArchivoConfiguracion> ObtenerArchivosConfigurados(string log)
        {

            List<ArchivoConfiguracion> losArchivos = new List<ArchivoConfiguracion>();
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Proc_ObtieneConfigArchivosSinFilas]");
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneConfigArchivosSinFilas");

                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {
                        ArchivoConfiguracion unArchivo = new ArchivoConfiguracion();

                        unArchivo.ID_Archivo = (Int64)losDatos.Tables[0].Rows[k]["ID_Archivo"];
                        unArchivo.ID_ConsultaBD = (Int64)losDatos.Tables[0].Rows[k]["ID_ConsultaBD"];
                        unArchivo.ClaveArchivo = (String)losDatos.Tables[0].Rows[k]["ClaveArchivo"];
                        unArchivo.DescripcionArchivo = (String)losDatos.Tables[0].Rows[k]["DescripcionArchivo"];
                        unArchivo.ID_TipoProceso = (Int32)losDatos.Tables[0].Rows[k]["ID_TipoProceso"];
                        unArchivo.Nombre = (String)losDatos.Tables[0].Rows[k]["Nombre"];
                        unArchivo.Prefijo = (String)losDatos.Tables[0].Rows[k]["Prefijo"];
                        unArchivo.Sufijo = (String)losDatos.Tables[0].Rows[k]["Sufijo"];
                        unArchivo.FormatoFecha = (String)losDatos.Tables[0].Rows[k]["FormatoFecha"];
                        unArchivo.PosicionFecha = (String)losDatos.Tables[0].Rows[k]["PosicionFecha"].ToString();
                        unArchivo.TipoArchivo = (String)losDatos.Tables[0].Rows[k]["TipoArchivo"].ToString();




                        losArchivos.Add(unArchivo);

                    }
                }


            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                throw new Exception(ex.Message);
            }

            return losArchivos;
        }

        internal static long GuardarFicheroEnBD(String unArchivo,
            SqlConnection conn,
            SqlTransaction transaccionSQL, string log, string ip)
        {

            try
            {
                Logueo.Evento("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_InsertaFichero]");
                SqlCommand command = new SqlCommand("proc_InsertaFichero", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                int resp = -1;

                command.Parameters.Add(new SqlParameter("@NombreFichero", unArchivo));
                command.Parameters.Add(new SqlParameter("@ID_Archivo", DBNull.Value));
                command.Parameters.Add(new SqlParameter("@ID_Consulta", DBNull.Value));

                SqlParameter param = new SqlParameter("@ID_Fichero", SqlDbType.BigInt);
                param.Value = 0;
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);


                resp = command.ExecuteNonQuery();

                Int64 elID = Convert.ToInt64(command.Parameters["@ID_Fichero"].Value);

                return elID;

            }
            catch (Exception err)
            {
                Logueo.Error("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return 0;
            }
        }

        internal static DataTable GeneraDataTableDetalle(List<DatosArchivo> elArchivo, long iD_Fichero, string subproducto = null)
        {
            DataTable dt = new DataTable("Detalle");
            dt.Columns.Add(new DataColumn("ID_FicheroDetalle"));
            dt.Columns.Add(new DataColumn("ID_Fichero"));
            dt.Columns.Add(new DataColumn("ID_Fila"));
            dt.Columns.Add(new DataColumn("DetalleCompleto"));
            dt.Columns.Add(new DataColumn("C001"));
            dt.Columns.Add(new DataColumn("C002"));
            dt.Columns.Add(new DataColumn("C003"));
            dt.Columns.Add(new DataColumn("C004"));
            dt.Columns.Add(new DataColumn("C005"));
            dt.Columns.Add(new DataColumn("C006"));
            dt.Columns.Add(new DataColumn("C007"));
            dt.Columns.Add(new DataColumn("C008"));
            dt.Columns.Add(new DataColumn("C009"));
            dt.Columns.Add(new DataColumn("C010"));
            dt.Columns.Add(new DataColumn("C011"));
            dt.Columns.Add(new DataColumn("C012"));
            dt.Columns.Add(new DataColumn("C013"));
            dt.Columns.Add(new DataColumn("C014"));
            dt.Columns.Add(new DataColumn("C015"));
            dt.Columns.Add(new DataColumn("C016"));

            foreach (var line in elArchivo)
            {
                DataRow row = dt.NewRow();

                if (String.IsNullOrEmpty(line.RawData))
                    continue;

                row["ID_FicheroDetalle"] = null;
                row["ID_Fichero"] = iD_Fichero;
                row["ID_Fila"] = 9;
                row["DetalleCompleto"] = line.RawData;
                row["C001"] = line.Emisor;
                row["C002"] = line.Cuenta;
                row["C003"] = "";//line.NumeroTarjeta;
                row["C004"] = line.Tarjeta;
                row["C005"] = DateTime.Parse(line.Vencimiento).ToString("yyyy-MM-dd");
                row["C006"] = (subproducto is null || subproducto == "") ? null : subproducto;
                row["C007"] = null;
                row["C008"] = null;
                row["C009"] = null;
                row["C010"] = null;
                row["C011"] = null;
                row["C012"] = null;
                row["C013"] = null;
                row["C014"] = null;
                row["C015"] = null;
                row["C016"] = null;

                dt.Rows.Add(row);
            }
            return dt;
        }

        internal static bool GuardarFicheroDetallesEnBD(DataTable dtDetalle, string log = "", string ip = "")
        {

            try
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    bulkCopy.DestinationTableName =
                        "dbo.FicheroDetalle";

                    try
                    {
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(dtDetalle);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error(" [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                        Console.WriteLine(ex.Message);
                    }
                }

                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [GuardarFicheroDetallesEnBD(): ERROR:" + err.Message + "]");
                return false;
            }
        }

        public static DataSet EjecutarSP(string procedimiento, Hashtable parametros, string pConexion)
        {
            DataSet retorno = new DataSet();
            SqlConnection connection = new SqlConnection(pConexion);
            try
            {

                connection.Open();
                SqlCommand query = new SqlCommand(procedimiento, connection);
                query.CommandType = CommandType.StoredProcedure;
                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                    }

                }

                SqlDataAdapter sda = new SqlDataAdapter(query);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional : null;
            }
            catch (Exception e)
            {
                Logueo.Error("[EjecutarSP] [Error al ejecutar sp: " + procedimiento + "] [Conexion: ] [Mensaje: " + e.Message + " TRACE: " + e.StackTrace + "]");
                return null;
            }
            finally
            {
                connection.Close();
            }

            return retorno;
        }

        public static DataSet EjecutarSP(string procedimiento, Hashtable parametros, SqlConnection conn2)
        {
            DataSet retorno = new DataSet();
     
            try
            {

                //connection.Open();
                SqlCommand query = new SqlCommand(procedimiento, conn2);
                query.CommandType = CommandType.StoredProcedure;
                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                    }

                }

                SqlDataAdapter sda = new SqlDataAdapter(query);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional : null;
            }
            catch (Exception e)
            {
                Logueo.Error("[EjecutarSP] [Error al ejecutar sp: " + procedimiento + "] [Conexion: ] [Mensaje: " + e.Message + " TRACE: " + e.StackTrace + "]");
                return null;
            }
           

            return retorno;
        }

        public static bool bulkInsertarDatosArchivoDetalle(DataTable tablaDetalles, string conn, string tablaAInsertar, string idFile, SqlTransaction transaction = null,string tipoArchivo=null)
        {

            try
            {
                tablaDetalles.Columns.Add("ID_Archivo", typeof(Int64));
                tablaDetalles.Columns.Add("TipoArchivo", typeof(String));
                foreach (DataRow fila in tablaDetalles.Rows)
                {

                    fila["ID_Archivo"] = idFile;
                    fila["TipoArchivo"] = tipoArchivo;

                }
                tablaDetalles.AcceptChanges();

                using (SqlConnection connection = new SqlConnection(conn))
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                {

                    foreach (DataColumn columna in tablaDetalles.Columns)
                    {
                        if (columna.ColumnName == "NombreComple")
                        {
                            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(columna.ColumnName, "NombreCompleto"));

                        }
                        else
                        {
                            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(columna.ColumnName, columna.ColumnName));
                        }
                    }
                   
                    //cmd.CommandText = "TRUNCATE TABLE membersE";
                    //cmd.ExecuteNonQuery();
                    bulkCopy.BulkCopyTimeout = 30000;
                    bulkCopy.DestinationTableName = tablaAInsertar;
                    bulkCopy.WriteToServer(tablaDetalles);
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[EjecutarSP] [Error al ejecutar el bulk de insercion] [Mensaje: " + ex.Message + "]");
                return false;
            }
            return true;
        }



    }
}
