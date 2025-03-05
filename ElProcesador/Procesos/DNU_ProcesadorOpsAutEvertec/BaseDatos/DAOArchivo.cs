using CommonProcesador;
using DNU_ProcesadorOpsAutEvertec.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DNU_ProcesadorOpsAutEvertec.BaseDatos
{
    public class DAOArchivo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Archivo ObtieneParametrosArchivos()
        {
            Archivo unArchivo = new Archivo();

            try
            {
                SqlDatabase database = new SqlDatabase(BDOperacionesEvertec.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneConfigArchivosOpEv");

                DataSet losDatos = database.ExecuteDataSet(command);
                
                int datos = losDatos.Tables[0].Rows.Count;

                if (datos > 0)
                {
                    foreach (DataRow dr in losDatos.Tables[0].Rows)
                    {
                        switch (dr.ItemArray[0].ToString())
                        {
                            case "DirectorioEntrada": unArchivo.RutaLectura = dr.ItemArray[1].ToString(); break;
                            case "DirectorioSalida": unArchivo.RutaEscritura = dr.ItemArray[1].ToString(); break;
                            case "PrefijoArchivos": unArchivo.Nombre = dr.ItemArray[1].ToString(); break;
                            case "NombreColumnaFinal": unArchivo.UltimaColumna = dr.ItemArray[1].ToString(); break;
                            case "LongColumna1": unArchivo.LongitudC1 = Convert.ToInt32(dr.ItemArray[1]); break;
                            case "LongColumna2": unArchivo.LongitudC2 = Convert.ToInt32(dr.ItemArray[1]); break;
                            case "LongColumna3": unArchivo.LongitudC3 = Convert.ToInt32(dr.ItemArray[1]); break;
                            case "LongColumna4": unArchivo.LongitudC4 = Convert.ToInt32(dr.ItemArray[1]); break;
                            case "LongColumna5": unArchivo.LongitudC5 = Convert.ToInt32(dr.ItemArray[1]); break;
                            case "LongColumna6": unArchivo.LongitudC6 = Convert.ToInt32(dr.ItemArray[1]); break;
                            case "LongColumna7": unArchivo.LongitudC7 = Convert.ToInt32(dr.ItemArray[1]); break;
                            case "LongColumna8": unArchivo.LongitudC8 = Convert.ToInt32(dr.ItemArray[1]); break;
                            case "LongColumna9": unArchivo.LongitudC9 = Convert.ToInt32(dr.ItemArray[1]); break;
                            case "LongColumna10": unArchivo.LongitudC10 = Convert.ToInt32(dr.ItemArray[1]); break;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }

            return unArchivo;
        }

        /// <summary>
        /// Inserta un nuevo registro de archivo en base de datos, con estatus en proceso
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo</param>
        /// <param name="connection">Conexión SQL prestablecida a la BD</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        /// <returns>ID del registro del archivo</returns>
        public static Int64 InsertaArchivoEnProceso(string nombreArchivo, SqlConnection connection,
            SqlTransaction transaccionSQL)
        {
            Int64 _ID = -1;

            try
            {
                SqlCommand command = new SqlCommand("Proc_InsertaArchivoOpsAutorizadas", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                command.Parameters.Add(new SqlParameter("@NombreArchivo", nombreArchivo));

                var sqlParameter1 = new SqlParameter("@IdArchivo", SqlDbType.BigInt);
                sqlParameter1.Direction = ParameterDirection.Output;
                command.Parameters.Add(sqlParameter1);

                command.ExecuteNonQuery();

                _ID = Convert.ToInt64(sqlParameter1.Value);
            }

            catch (Exception ex)
            {
                Logueo.Error("InsertaArchivoEnProceso() " + ex.Message);
            }

            return _ID;
        }

        /// <summary>
        /// Inserta los registros del archivo en base de datos
        /// </summary>
        /// <param name="dtFile">DataTable con los registros del archivo</param>
        /// <param name="IdArchivo">Identificador del archivo</param>
        /// <param name="connection">Conexión SQL prestablecida a la BD</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        public static void InsertaDetalleArchivo(DataTable dtFile, Int64 IdArchivo, SqlConnection connection,
            SqlTransaction transaccionSQL)
        {
           try
            {
                SqlCommand command = new SqlCommand("Proc_InsertaArchivoDetalleOpsAutorizadas", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;
                command.CommandTimeout = 0;

                command.Parameters.Add(new SqlParameter("@Archivo", dtFile));
                command.Parameters.Add(new SqlParameter("@IdArchivo", IdArchivo));

                command.ExecuteNonQuery();
            }

            catch (Exception ex)
            {
                Logueo.Error("InsertaDetalleArchivo() " + ex.Message);
            }
        }

        /// <summary>
        /// Inserta el registro en bitácora de un archivo recibido vacío
        /// </summary>
        /// <param name="IdArchivo">Identificador del archivo</param>
        /// <param name="connection">Conexión SQL prestablecida a la BD</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        public static void InsertaArchivoVacioEnBitacora(Int64 IdArchivo, SqlConnection connection,
            SqlTransaction transaccionSQL)
        {
            try
            {
                SqlCommand command = new SqlCommand("Proc_InsertaBitacoraArchivoSinOperaciones", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;
                command.CommandTimeout = 0;

                command.Parameters.Add(new SqlParameter("@IdArchivo", IdArchivo));

                command.ExecuteNonQuery();
            }

            catch (Exception ex)
            {
                Logueo.Error("InsertaArchivoVacioEnBitacora() " + ex.Message);
            }
        }

        /// <summary>
        /// Inserta los registros del archivo en base de datos
        /// </summary>
        /// <param name="idArchivo">Identificador del archivo</param>
        /// <param name="estatus">Bandera con el estatus de procesamiento del archivo</param>
        /// <param name="connection">Conexión SQL prestablecida a la BD</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        public static void ActualizaEstatusArchivo(Int64 idArchivo, int estatus, SqlConnection connection,
            SqlTransaction transaccionSQL)
        {
            try
            {
                SqlCommand command = new SqlCommand("Proc_ActualizaEstatusArchivoOpsAutorizadas", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                command.Parameters.Add(new SqlParameter("@IdArchivo", idArchivo));
                command.Parameters.Add(new SqlParameter("@Estatus", estatus));

                command.ExecuteNonQuery();
            }

            catch (Exception ex)
            {
                Logueo.Error("ActualizaEstatusArchivo() " + ex.Message);
            }
        }
    }
}
