using CommonProcesador;
using DNU_CompensadorParabiliumCommon.Constants;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Utilidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.UI.WebControls;

namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    public class DAOArchivo
    {

        static DataTable dataTable = null;


        public static RespuestaInsertFicheroTemp ActualizaEstatusFicheroTemp(RespuestaInsertFicheroTemp respFichero, string claveEstatus, SqlConnection connConsulta, string etiquetaLogueo)
        {
            try
            {
                if (connConsulta.State != ConnectionState.Open)
                    connConsulta.Open();

                string status = string.Empty;
                if (string.IsNullOrEmpty(respFichero.CodRespuesta))
                    status = "ERROR";
                else
                {
                    int valCodigo = Convert.ToInt32(respFichero.CodRespuesta);
                    if (valCodigo >= 500 && valCodigo <= 599)
                        status = "ERROR";
                    else
                        status = "OK";
                }

                SqlCommand command = new SqlCommand("PN_CRG_ActualizaEstatusFicheroTemp", connConsulta);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@idFicheroTemp", respFichero.IdFicheroTemp));
                command.Parameters.Add(new SqlParameter("@cveEstatus_Destino", string.IsNullOrEmpty(claveEstatus) ? status : claveEstatus));
                command.Parameters.Add(new SqlParameter("@error", respFichero.DescRespuesta));

                SqlParameter param = new SqlParameter("@respuesta", SqlDbType.VarChar, 4);
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);

                param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);

                var resp = command.ExecuteNonQuery();

                respFichero.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                respFichero.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                LogueoSPS(respFichero, command.CommandText, etiquetaLogueo);

                return respFichero;

            }
            catch (Exception ex)
            {
                respFichero.CodRespuesta = "500";
                respFichero.DescRespuesta = NombreMetodo() + "(): Mensaje [" + ex.Message + "]";
                throw new Exception(respFichero.DescRespuesta);
            }
        }

        public static RespuestaInsertFicheroTemp GuardaFicheroTemp(string UrlArchivo, string nombreSinExtencion, SqlConnection connConsulta, string etiquetaLogueo)
        {
            RespuestaInsertFicheroTemp respFichero = new RespuestaInsertFicheroTemp();

            try
            {
                if (connConsulta.State != ConnectionState.Open)
                {
                    connConsulta.Open();
                }
                SqlCommand command = new SqlCommand("PN_CRG_InsertaFicheroTemp", connConsulta);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@nombreFicheroOriginal", nombreSinExtencion));
                command.Parameters.Add(new SqlParameter("@ruta", UrlArchivo));

                SqlParameter param = new SqlParameter("@idFicheroTemp", SqlDbType.BigInt);
                param.Value = 0;
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);

                param = new SqlParameter("@extension", SqlDbType.VarChar, 5);
                param.Value = 0;
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);

                param = new SqlParameter("@respuesta", SqlDbType.VarChar, 4);
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);

                param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);

                int resp = command.ExecuteNonQuery();

                respFichero.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                respFichero.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                LogueoSPS(respFichero, command.CommandText, etiquetaLogueo);

                respFichero.IdFicheroTemp = Convert.ToInt64(command.Parameters["@idFicheroTemp"].Value);
                respFichero.extensionFile = command.Parameters["@extension"].Value.ToString();

                return respFichero;
            }
            catch (Exception ex)
            {
                respFichero.CodRespuesta = "500";
                respFichero.DescRespuesta = NombreMetodo() + "(): Mensaje [" + ex.Message + "]";
                throw new Exception(respFichero.DescRespuesta);
            }


        }

        public static DataTable ObtieneDefinicionFicheroTemp(RespuestaInsertFicheroTemp respFichero, SqlConnection connConsulta, string etiquetaLogueo)
        {
            try
            {
                if (connConsulta.State != ConnectionState.Open)
                {
                    connConsulta.Open();
                }
                SqlCommand command = new SqlCommand("PN_CRG_ObtieneDefinicionFicheroTemp", connConsulta);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@idFicheroTemp", respFichero.IdFicheroTemp));

                SqlParameter param1 = new SqlParameter("@respuesta", SqlDbType.VarChar, 4);
                param1.Direction = ParameterDirection.Output;
                command.Parameters.Add(param1);

                SqlParameter param2 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                param2.Direction = ParameterDirection.Output;
                command.Parameters.Add(param2);

                DataTable dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());

                //respFichero.IdFicheroTemp = Convert.ToInt64(command.Parameters["@idFicheroTemp"].Value);
                respFichero.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                respFichero.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                LogueoSPS(respFichero, command.CommandText, etiquetaLogueo);

                return dataTable;
            }
            catch (Exception ex)
            {
                respFichero.CodRespuesta = "500";
                respFichero.DescRespuesta = NombreMetodo() + "(): Mensaje [" + ex.Message + "]";
                throw new Exception(respFichero.DescRespuesta);
            }
        }

        public static bool bulkInsertarDatosArchivoDetalle(RespuestaInsertFicheroTemp respFichero, SqlConnection connConsulta, SqlTransaction transaccionSQL, string tablaAInsertar, string timeoutBulk)
        {
            try
            {
                if (connConsulta.State != ConnectionState.Open)
                    connConsulta.Open();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connConsulta, new SqlBulkCopyOptions(), transaccionSQL))
                {
                    foreach (DataColumn col in dataTable.Columns)
                        bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    
                    bulkCopy.BulkCopyTimeout = string.IsNullOrEmpty(timeoutBulk) ? 30000 : Convert.ToInt32(timeoutBulk);
                    bulkCopy.DestinationTableName = tablaAInsertar;
                    bulkCopy.WriteToServer(dataTable);
                }
                return true;
            }
            catch (Exception ex)
            {
                respFichero.CodRespuesta = "500";
                respFichero.DescRespuesta = NombreMetodo() + "(): Mensaje [" + ex.Message + "]";
                transaccionSQL.Rollback();
                throw new Exception(respFichero.DescRespuesta);
            }
        }

        public static RespuestaInsertFicheroTemp RestableceFicheroTemp(RespuestaInsertFicheroTemp respFichero, SqlConnection connConsulta, string etiquetaLogueo)
        {
            try
            {
                if (connConsulta.State != ConnectionState.Open)
                {
                    connConsulta.Open();
                }
                SqlCommand command = new SqlCommand("PN_CRG_RestableceFicherosTemp", connConsulta);
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 4);
                param2.Direction = ParameterDirection.Output;
                command.Parameters.Add(param2);

                SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                param3.Direction = ParameterDirection.Output;
                command.Parameters.Add(param3);

                var resp = command.ExecuteNonQuery();

                respFichero.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                respFichero.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                LogueoSPS(respFichero, command.CommandText, etiquetaLogueo);

                return respFichero;
            }
            catch (Exception ex)
            {
                respFichero.CodRespuesta = "500";
                respFichero.DescRespuesta = NombreMetodo() + "(): Mensaje [" + ex.Message + "]";
                throw new Exception(respFichero.DescRespuesta);
            }
        }

        private static DataTable CrearTablaFicheroProvicional()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID_FicheroProvisional", typeof(System.Int64));
            dataTable.Columns.Add("ID_FicheroTemp", typeof(System.Int64));
            dataTable.Columns.Add("ID_EstatusFicheroDetalle", typeof(System.Int64));
            dataTable.Columns.Add("Bin", typeof(System.String));
            dataTable.Columns.Add("TarjetaLongitud", typeof(System.Int64));
            dataTable.Columns.Add("TarjetaEnmascarada", typeof(System.String));
            dataTable.Columns.Add("C001", typeof(System.String));
            dataTable.Columns.Add("C002", typeof(System.String));
            dataTable.Columns.Add("C003", typeof(System.String));
            dataTable.Columns.Add("C004", typeof(System.String));
            dataTable.Columns.Add("C005", typeof(System.String));
            dataTable.Columns.Add("C006", typeof(System.String));
            dataTable.Columns.Add("C007", typeof(System.String));
            dataTable.Columns.Add("C008", typeof(System.String));
            dataTable.Columns.Add("C009", typeof(System.String));
            dataTable.Columns.Add("C010", typeof(System.String));
            dataTable.Columns.Add("C011", typeof(System.String));
            dataTable.Columns.Add("C012", typeof(System.String));
            dataTable.Columns.Add("C013", typeof(System.String));
            dataTable.Columns.Add("C014", typeof(System.String));
            dataTable.Columns.Add("C015", typeof(System.String));
            dataTable.Columns.Add("C016", typeof(System.String));
            dataTable.Columns.Add("C017", typeof(System.String));
            dataTable.Columns.Add("C018", typeof(System.String));
            dataTable.Columns.Add("C019", typeof(System.String));
            dataTable.Columns.Add("C020", typeof(System.String));
            dataTable.Columns.Add("C021", typeof(System.String));
            dataTable.Columns.Add("C022", typeof(System.String));
            dataTable.Columns.Add("C023", typeof(System.String));
            dataTable.Columns.Add("C024", typeof(System.String));
            dataTable.Columns.Add("C025", typeof(System.String));
            dataTable.Columns.Add("C026", typeof(System.String));
            dataTable.Columns.Add("C027", typeof(System.String));
            dataTable.Columns.Add("C028", typeof(System.String));
            dataTable.Columns.Add("C029", typeof(System.String));
            dataTable.Columns.Add("C030", typeof(System.String));
            dataTable.Columns.Add("C031", typeof(System.String));
            dataTable.Columns.Add("C032", typeof(System.String));
            dataTable.Columns.Add("C033", typeof(System.String));
            dataTable.Columns.Add("C034", typeof(System.String));
            dataTable.Columns.Add("C035", typeof(System.String));
            dataTable.Columns.Add("C036", typeof(System.String));
            dataTable.Columns.Add("C037", typeof(System.String));
            dataTable.Columns.Add("C038", typeof(System.String));
            dataTable.Columns.Add("C039", typeof(System.String));
            dataTable.Columns.Add("C040", typeof(System.String));
            dataTable.Columns.Add("C041", typeof(System.String));
            dataTable.Columns.Add("C042", typeof(System.String));
            dataTable.Columns.Add("C043", typeof(System.String));
            dataTable.Columns.Add("C044", typeof(System.String));
            dataTable.Columns.Add("C045", typeof(System.String));
            dataTable.Columns.Add("C046", typeof(System.String));
            dataTable.Columns.Add("C047", typeof(System.String));
            dataTable.Columns.Add("C048", typeof(System.String));
            dataTable.Columns.Add("C049", typeof(System.String));
            dataTable.Columns.Add("C050", typeof(System.String));
            dataTable.Columns.Add("C051", typeof(System.String));
            dataTable.Columns.Add("C052", typeof(System.String));
            dataTable.Columns.Add("C053", typeof(System.String));
            dataTable.Columns.Add("C054", typeof(System.String));
            dataTable.Columns.Add("C055", typeof(System.String));
            dataTable.Columns.Add("C056", typeof(System.String));
            dataTable.Columns.Add("C057", typeof(System.String));
            dataTable.Columns.Add("C058", typeof(System.String));
            dataTable.Columns.Add("C059", typeof(System.String));
            dataTable.Columns.Add("C060", typeof(System.String));
            dataTable.Columns.Add("C061", typeof(System.String));
            dataTable.Columns.Add("C062", typeof(System.String));
            dataTable.Columns.Add("C063", typeof(System.String));
            dataTable.Columns.Add("C064", typeof(System.String));
            dataTable.Columns.Add("C065", typeof(System.String));
            dataTable.Columns.Add("C066", typeof(System.String));
            dataTable.Columns.Add("C067", typeof(System.String));
            dataTable.Columns.Add("C068", typeof(System.String));
            dataTable.Columns.Add("C069", typeof(System.String));
            dataTable.Columns.Add("C070", typeof(System.String));
            dataTable.Columns.Add("C071", typeof(System.String));
            dataTable.Columns.Add("C072", typeof(System.String));
            dataTable.Columns.Add("C073", typeof(System.String));
            dataTable.Columns.Add("C074", typeof(System.String));
            dataTable.Columns.Add("C075", typeof(System.String));
            dataTable.Columns.Add("C076", typeof(System.String));
            dataTable.Columns.Add("C077", typeof(System.String));
            dataTable.Columns.Add("C078", typeof(System.String));
            dataTable.Columns.Add("C079", typeof(System.String));
            dataTable.Columns.Add("C080", typeof(System.String));
            dataTable.Columns.Add("C081", typeof(System.String));
            dataTable.Columns.Add("C082", typeof(System.String));
            dataTable.Columns.Add("C083", typeof(System.String));
            dataTable.Columns.Add("C084", typeof(System.String));
            dataTable.Columns.Add("C085", typeof(System.String));
            dataTable.Columns.Add("C086", typeof(System.String));
            dataTable.Columns.Add("C087", typeof(System.String));
            dataTable.Columns.Add("C088", typeof(System.String));
            dataTable.Columns.Add("C089", typeof(System.String));
            dataTable.Columns.Add("C090", typeof(System.String));
            dataTable.Columns.Add("C091", typeof(System.String));
            dataTable.Columns.Add("C092", typeof(System.String));
            dataTable.Columns.Add("C093", typeof(System.String));
            dataTable.Columns.Add("C094", typeof(System.String));
            dataTable.Columns.Add("C095", typeof(System.String));
            dataTable.Columns.Add("C096", typeof(System.String));
            dataTable.Columns.Add("C097", typeof(System.String));
            dataTable.Columns.Add("C098", typeof(System.String));
            dataTable.Columns.Add("C099", typeof(System.String));
            dataTable.Columns.Add("C100", typeof(System.String));
            dataTable.Columns.Add("C101", typeof(System.String));
            dataTable.Columns.Add("C102", typeof(System.String));
            dataTable.Columns.Add("C103", typeof(System.String));
            dataTable.Columns.Add("C104", typeof(System.String));
            dataTable.Columns.Add("C105", typeof(System.String));
            dataTable.Columns.Add("C106", typeof(System.String));
            dataTable.Columns.Add("C107", typeof(System.String));
            dataTable.Columns.Add("C108", typeof(System.String));
            dataTable.Columns.Add("C109", typeof(System.String));
            dataTable.Columns.Add("C110", typeof(System.String));
            dataTable.Columns.Add("C111", typeof(System.String));
            dataTable.Columns.Add("C112", typeof(System.String));
            dataTable.Columns.Add("C113", typeof(System.String));
            dataTable.Columns.Add("C114", typeof(System.String));
            dataTable.Columns.Add("C115", typeof(System.String));
            dataTable.Columns.Add("C116", typeof(System.String));
            dataTable.Columns.Add("C117", typeof(System.String));
            dataTable.Columns.Add("C118", typeof(System.String));
            dataTable.Columns.Add("C119", typeof(System.String));
            dataTable.Columns.Add("C120", typeof(System.String));
            dataTable.Columns.Add("C121", typeof(System.String));
            dataTable.Columns.Add("C122", typeof(System.String));
            dataTable.Columns.Add("C123", typeof(System.String));
            dataTable.Columns.Add("C124", typeof(System.String));
            dataTable.Columns.Add("C125", typeof(System.String));
            dataTable.Columns.Add("C126", typeof(System.String));
            dataTable.Columns.Add("C127", typeof(System.String));
            dataTable.Columns.Add("C128", typeof(System.String));
            dataTable.Columns.Add("C129", typeof(System.String));
            dataTable.Columns.Add("C130", typeof(System.String));
            dataTable.Columns.Add("C131", typeof(System.String));
            dataTable.Columns.Add("C132", typeof(System.String));
            dataTable.Columns.Add("C133", typeof(System.String));

            return dataTable;
        }


        public static void LlenarTablaFicheroProvicional(DataTable tablaLayout, RespuestaInsertFicheroTemp respFichero, FileInfo archivoOriginal, string etiquetaLogueo
                            , FileInfo archivoDecrypt = null)
        {
            try
            {
                FileInfo archivo = null;

                if (archivoDecrypt != null && File.Exists(archivoDecrypt.FullName))
                    archivo = archivoDecrypt;
                else
                    archivo = archivoOriginal;
             
                dataTable = CrearTablaFicheroProvicional();
                int rowTab = 0;
                List<Bines> lstBines = obtenerBines_V6(respFichero, etiquetaLogueo);
                foreach (var line in File.ReadLines(archivo.FullName))
                {
                    try
                    {
                        if (string.IsNullOrEmpty(line)) continue;

                        dataTable.Rows.Add();
                        int columnasOmitir = 0;

                        #region Omitir Columnas

                        foreach (DataColumn item in dataTable.Columns)
                        {
                            //Calcular cuantas columnas omitir antes de cargar la data del archivo
                            string columna = item.ColumnName;
                            if (columna.StartsWith("C0"))
                            {
                                break;
                            }
                            columnasOmitir++;
                        }
                        columnasOmitir--;//El id de la tabla no cuenta

                        #endregion Omitir Columnas

                        #region Carga de Campos C00X
                        string campoC002 = string.Empty;
                        foreach (DataRow item in tablaLayout.Rows)
                        {
                            int destinoCampo = Convert.ToInt32(item.ItemArray[1].ToString());
                            int origenPosicionInicial = Convert.ToInt32(item.ItemArray[2].ToString());
                            int origenLongitud = Convert.ToInt32(item.ItemArray[3].ToString());

                            string columna = string.Empty;

                            try
                            {
                                columna = dataTable.Columns[destinoCampo + columnasOmitir].ColumnName;
                            }
                            catch { continue; }

                            if (string.IsNullOrEmpty(columna))
                                continue;

                            string valor = line.Substring(origenPosicionInicial - 1, origenLongitud).Trim();//El subsring inicia en 0 por eso se resta 1
                            if (columna == "C002")
                            {
                                if (valor.Contains(" "))
                                    valor = valor.Replace(" ", "");
                                if (string.IsNullOrEmpty(valor))
                                    valor = null;

                                campoC002 = valor;
                            }

                            dataTable.Rows[rowTab][columna] = valor;
                        }

                        #endregion Carga de Campos C00X

                        #region Columnas fijas

                        {// ID_FicheroTemp
                            var ID_FicheroTemp = respFichero.IdFicheroTemp.ToString();
                            dataTable.Rows[rowTab]["ID_FicheroTemp"] = ID_FicheroTemp;
                        }
                        {// ID_EstatusFicheroDetalle
                            var ID_EstatusFicheroDetalle = "1";
                            dataTable.Rows[rowTab]["ID_EstatusFicheroDetalle"] = ID_EstatusFicheroDetalle;
                        }
                        {// Bin
                            string bin = string.IsNullOrEmpty(campoC002) ? null: validaBines_V6(campoC002, lstBines);
                            dataTable.Rows[rowTab]["Bin"] = bin;
                        }
                        {// TarjetaLongitud
                            var TarjetaLongitud = string.IsNullOrEmpty(campoC002) ? 0 : campoC002.Length;
                            dataTable.Rows[rowTab]["TarjetaLongitud"] = TarjetaLongitud;
                        }
                        {// TarjetaEnmascarada
                            string tarjeta = campoC002;
                            if (string.IsNullOrEmpty(tarjeta))
                                tarjeta = "**** **** **** ****";
                            else
                            {
                                string parte1 = tarjeta.Substring(0, 4);
                                string parte2 = "****";
                                string parte3 = $"**{tarjeta.Substring(10, 2)}";
                                string parte4 = tarjeta.Substring(12, 4);
                                tarjeta = $"{parte1} {parte2} {parte3} {parte4}";
                            }

                            dataTable.Rows[rowTab]["TarjetaEnmascarada"] = tarjeta;
                        }
                        #endregion Columnas fijas
                    }
                    catch (Exception ex)
                    {
                        Log.Error(etiquetaLogueo + "[Ocurrió Error en línea " + (rowTab + 1) + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
                    }
                    rowTab++;
                }
            }
            catch (Exception ex)
            {
                respFichero.CodRespuesta = "500";
                respFichero.DescRespuesta = NombreMetodo() + "(): Mensaje [" + ex.Message + "]";
                throw new Exception(respFichero.DescRespuesta);
            }
        }

        private static List<Bines> obtenerBines_V6(RespuestaInsertFicheroTemp respFichero, string etiquetaLogueo)
        {
            try
            {
                List<Bines> lstBines = new List<Bines>();
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("PN_CRG_ObtieneBines");

                SqlParameter param = new SqlParameter("@respuesta", SqlDbType.BigInt);
                param.Value = 0;
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);

                param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);

                DataSet losDatos = database.ExecuteDataSet(command);

                respFichero.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                respFichero.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                LogueoSPS(respFichero, command.CommandText, etiquetaLogueo);

                foreach (DataRow row in losDatos.Tables[0].Rows)
                {
                    Bines bines = new Bines()
                    {
                        idBin = Convert.ToInt32(row["idBin"].ToString()),
                        bin = row["bin"].ToString(),
                        longitudBin = row["longitudBin"].ToString(),
                        fechaActivacion = row["fechaActivacion"].ToString(),
                        cveProducto = row["cveProducto"].ToString()
                    };
                    lstBines.Add(bines);
                }

                return lstBines;
            }
            catch (Exception ex)
            {
                respFichero.CodRespuesta = "500";
                respFichero.DescRespuesta = NombreMetodo() + "(): Mensaje [" + ex.Message + "]";
                throw new Exception(respFichero.DescRespuesta);
            }
        }

        private static string validaBines_V6(string claveMA, List<Bines> lstBines)
        {
            foreach (var bin in lstBines)
            {
                string binlst = claveMA.Substring(0, Convert.ToInt32(bin.longitudBin)).Trim();
                foreach (var binLongitud in lstBines)
                {
                    if (binlst.Equals(binLongitud.bin))
                        return binlst;
                }
            }
            return null;
        }

        public static void LlenarTablaFicheroProvicionalTags(DataTable tablaLayout, RespuestaInsertFicheroTemp respFichero, FileInfo archivo, string etiquetaLogueo)
        {
            try
            {
                dataTable = CrearTablaFicheroProvicional();

                string contentJson = ReadJson(archivo.FullName, etiquetaLogueo);


                if (!string.IsNullOrEmpty(contentJson))
                {
                    int rowTab = 0;

                    dynamic js = JsonConvert.DeserializeObject<dynamic>(contentJson);

                    int numRegistros = js.CONTENT.Count;

                    for(int i = 0; i<numRegistros;i++)
                    {
                        dataTable.Rows.Add();
                        try
                        {
                            foreach (DataRow item in tablaLayout.Rows)
                            {
                                
                                int destinoCampo = Convert.ToInt32(item.ItemArray[1].ToString());
                                string origenClave = item.ItemArray[4].ToString();

                                string columna = string.Empty;

                                columna = destinoCampo.ToString().PadLeft(3, '0');

                                columna = "C" + columna;

                                dataTable.Rows[rowTab][columna] = ObtenerValorJSON(origenClave, js.CONTENT[i]);
                            }

                            #region Columnas fijas

                            {// ID_FicheroTemp
                                var ID_FicheroTemp = respFichero.IdFicheroTemp.ToString();
                                dataTable.Rows[rowTab]["ID_FicheroTemp"] = ID_FicheroTemp;
                            }
                            {// ID_EstatusFicheroDetalle
                                var ID_EstatusFicheroDetalle = "1";
                                dataTable.Rows[rowTab]["ID_EstatusFicheroDetalle"] = ID_EstatusFicheroDetalle;
                            }
                            
                            #endregion Columnas fijas
                        }
                        catch (Exception ex)
                        {
                            Log.Error("[Ocurrió Error en línea " + (rowTab + 1) + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
                        }
                        rowTab++;
                    }
                }
                else 
                {
                    throw new Exception("El archivo json vien vacío");
                }
            }
            catch (Exception ex)
            {
                respFichero.CodRespuesta = "500";
                respFichero.DescRespuesta = NombreMetodo() + "(): Mensaje [" + ex.Message + "]";
                throw new Exception(respFichero.DescRespuesta);
            }
        }

        private static string ReadJson(string filePath, string etiquetaLogueo)
        {
            string ruta = filePath;
            string contenidoJson = string.Empty;

            try
            {
                contenidoJson = File.ReadAllText(ruta);
                if (!string.IsNullOrEmpty(contenidoJson))
                    return contenidoJson;

            }
            catch (Exception ex)
            {
                Log.Error(etiquetaLogueo + "[ReadJson] [" +  ex.Message+ "]");
            }
            return contenidoJson;
        }

        private static string ObtenerValorJSON(string tag, dynamic jsonContent)
        {
            string[] valoresTag = tag.Split('.');
            int totTag = valoresTag.Count();

            if (totTag == 3)
            {
                foreach (JProperty p in jsonContent[valoresTag[totTag - 2]].Properties())
                {
                    if (p.Name.Equals(valoresTag[totTag - 1]))
                        return p.Value.ToString();
                }
            }
            if (totTag == 4) 
            {
                foreach (JProperty p in jsonContent[valoresTag[totTag - 3]][valoresTag[totTag - 2]].Properties())
                {
                    if (p.Name.Equals(valoresTag[totTag - 1]))
                        return p.Value.ToString();
                }
            }

            return null;
        }

        private static void LogueoSPS(RespuestaGral respuestaGral, string ComandoEnTexto, string etiquetaLogueo)
        {
            int valCodigo = Convert.ToInt32(respuestaGral.CodRespuesta);

            if (valCodigo >= 200 && valCodigo <= 299)
            {
                Log.Evento(etiquetaLogueo + "[" + respuestaGral.DescRespuesta + "]");
                return;
            }
            else //if (valCodigo >= 500 && valCodigo <= 599)
                Log.Error(etiquetaLogueo + "[" + respuestaGral.DescRespuesta + "]");

            throw new Exception("LogueoSPS.Error al ejecutar '" + ComandoEnTexto + $"'");
        }

        /// <summary>
        /// Devuelve el nombre del metodo que llamo a este metodo.
        /// Ejemplo el metodo consultaDato llama a este metodo el resultado es: consultaDato
        /// </summary>
        /// <returns>Nombre del metodo que lo invocó</returns>
        private static string NombreMetodo([CallerMemberName] string metodo = null)
        {
            return metodo;
        }
    }
}
