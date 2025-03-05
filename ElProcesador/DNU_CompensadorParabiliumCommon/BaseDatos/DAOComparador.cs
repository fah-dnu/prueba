using CommonProcesador;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Utilidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DNU_CompensadorParabiliumCommon.Extensions;
using RestSharp.Validation;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Interfases.Entidades;
using System.Linq.Expressions;
using Executer.Entidades;
using Executer.Utilidades;
using Interfases.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Remoting.Lifetime;

namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    public class DAOComparador
    {
        /// <summary>
        /// Valida que existan movimientos con timeout y actualiza para el Reproceso
        /// </summary>
        /// <returns></returns>
        public static int ValidaActualizarMovimientosTimeout()
        {
            int registros = 0;
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_COMP_ValidaReprocesoCompensacion");
                command.CommandTimeout = 600;
                DataTable tablabin = new DataTable();
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);
                if (losDatos2 != null && losDatos2.Tables.Count == 2)
                {
                    tablabin = losDatos2.Tables[1];
                }
                foreach (DataRow row in losDatos2.Tables[0].Rows)
                {
                    registros = Convert.ToInt32(row["ExisteRegistroTimeout"].ToString());

                }
            }
            catch (Exception)
            {

                throw;
            }

            return registros;
        }

        /// <summary>
        /// Actualiza Operaciones Importe cero para mostrar en la consulta de Movimientos
        /// </summary>
        /// <returns></returns>
        public static void CompensaOperacionesImporteCero(string connConsulta, string etiquetaLogueo)
        {
            try
            {
                string sp = "PN_CIC_CompensaOperacionesImporteCero";
                RespuestaGral respuestaGral = new RespuestaGral();

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        command.ExecuteNonQuery();

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }
                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + "[CompensaOperacionesImporteCero():  ERROR: " + err.Message);
            }
        }

        /// <summary>
        /// Metodo que obtiene la moneda relacionada a la cuenta de la tarjeta que es enviada
        /// </summary>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public static string ObtieneMonedaCuenta(string tarjeta)
        {
            string ClaveNumericaMoneda = string.Empty;

            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaAutorizador);
                DbCommand command = database.GetStoredProcCommand("Proc_Comp_ObtieneMonedaCuenta");

                DbParameter paramSSN = command.CreateParameter();
                paramSSN.ParameterName = "@claveMA";
                paramSSN.DbType = DbType.AnsiStringFixedLength;
                paramSSN.Direction = ParameterDirection.Input;
                paramSSN.Value = tarjeta.TrimEnd(' ');
                paramSSN.Size = tarjeta.TrimEnd(' ').Length;
                command.Parameters.Add(paramSSN);

                DataTable tablabin = new DataTable();
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);
                foreach (DataRow row in losDatos2.Tables[0].Rows)
                {
                    ClaveNumericaMoneda = row["ClaveMoneda"].ToString().Trim();

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
            return ClaveNumericaMoneda;
        }

        public static List<Movimiento> ObtieneMovimientosPorProcesar()
        {
            List<Movimiento> MisMovimientos = new List<Movimiento>();


            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDEscrituraArchivo);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_COMP_ObtieneRegistrosParaGenerarPolizas");
                command.CommandTimeout = 600;
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);

                DataTable tablabin = new DataTable();
                if (losDatos2 != null && losDatos2.Tables.Count == 2)
                {
                    tablabin = losDatos2.Tables[1];
                }

                int regi = 0;

                foreach (DataRow row in losDatos2.Tables[0].Rows)
                {
                    try
                    {
                        regi++;
                        Movimiento dato = new Movimiento();
                        dato.IDFichero = Convert.ToInt64(row["ID_Fichero"]);
                        dato.Ruta = row["Ruta"].ToString();

                        dato.IDFicheroDetalle = Convert.ToInt64(row["ID_FicheroDetalle"]);
                        dato.ClaveMA = row["C002"].ToString().TrimEnd(' ');

                        dato.MonedaCuenta = ObtieneMonedaCuenta(row["C002"].ToString());

                        dato.IdColectiva = 0;

                        dato.TipoMA = "TAR";
                        dato.ClaveEvento = row["C001"].ToString();
                        dato.Observaciones = "PROCESAMIENTO AUTOMATICO T112";
                        dato.Autorizacion = row["C018"].ToString();


                        dato.FechaOperacion = GetFechaOperacion(dato, row["C005"].ToString(), row["FechaPresentacion"].ToString());

                        dato.ReferenciaNumerica = row["ID_FicheroDetalle"].ToString();
                        dato.Ticket = row.GetStringValue("C004");

                        dato.T112_CodigoMonedaLocal = row["C056"].ToString(); //Codigo moneda compensacion
                        dato.T112_ImporteCompensadoLocal = ((decimal.Parse(row["C077"].ToString()) / 100) + ObtieneValorDecimal(row["C045"].ToString())).ToString();

                        #region DNUUH-33
                        dato.T112_Imp_45 = (ObtieneValorDecimal(row["C045"].ToString())).ToString(); //Metodo obtiene float lo divide entre 100
                        dato.T112_Imp_77 = (decimal.Parse(row["C077"].ToString()) / 100).ToString();
                        dato.Referencia2 = row["C097"].ToString();  //Referencia 2.
                        #endregion

                        dato.T112_CuotaIntercambio = (decimal.Parse(row["C020"].ToString()) / 100).ToString();

                        if (row["C009"].ToString().Equals("840")) //es Dolar
                        {
                            dato.T112_ImporteCompensadoDolar = (decimal.Parse(row["C008"].ToString()) / 100).ToString();
                        }
                        else
                        {
                            dato.T112_ImporteCompensadoDolar = "0";
                        }

                        dato.Importe = ((decimal.Parse(row["C077"].ToString()) / 100) + ObtieneValorDecimal(row["C045"].ToString())).ToString(); ///Que diferencia hay con importe compensado local?
                        dato.ImporteMonedaOriginal = ((decimal.Parse(row["C006"].ToString()) / 100)).ToString(); // se comenta el c0045 ya que puede ser moneda difernete a c006
                        dato.MonedaOriginal = row["C007"].ToString();
                        dato.T112_ImporteCompensadoPesos = ((decimal.Parse(row["C077"].ToString()) / 100) + ObtieneValorDecimal(row["C045"].ToString())).ToString();//porque siempre viene en pesos?

                        try
                        {
                            dato.T112_IVA = (decimal.Parse(row["C050"].ToString().Substring(3, 12)) / 100).ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_IVA = "0";
                            Log.Error("ObtieneMovimientos(): error al Obtenr el Campo IVA [" + regi + "] Valor en T112: [" + row["C050"].ToString() + "];" + err.Message);

                        }

                        try
                        {

                            dato.T112_FechaPresentacion = row["FechaPresentacion"].ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_FechaPresentacion = DateTime.Now.ToString("yyyyMMdd");
                            Log.Error("ObtieneMovimientos(): error al Obtenr el Campo Fecha Presentacion" + err.Message);

                        }

                        try
                        {
                            dato.T112_NombreArchivo = row["NombreArchivo"].ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_NombreArchivo = "";
                            Log.Error("ObtieneMovimientos(): error al Obtenr el Campo Nombre de Archivo" + err.Message);

                        }

                        //Nuevos Campos 
                        dato.T112_CodigoTx = row.GetStringValue("C001");
                        dato.T112_Comercio = row.GetStringValue("C010");
                        dato.T112_Ciudad = row.GetStringValue("C011");
                        dato.T112_Pais = row.GetStringValue("C012");
                        dato.T112_MCC = row.GetStringValue("C013");
                        dato.MonedaDestino = row.GetStringValue("C009");
                        dato.T112_Referencia = row.GetStringValue("C004");
                        dato.T112_FechaProc = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        dato.T112_FechaJuliana = row.GetStringValue("C019");
                        dato.T112_FechaConsumo = row.GetStringValue("C005");
                        dato.T112_Ciclo = row.GetStringValue("Ciclo");// dato.T112_NombreArchivo.IndexOf('.') > 0 ? dato.T112_NombreArchivo.Substring(dato.T112_NombreArchivo.IndexOf('.')  - 1, 1) : dato.T112_NombreArchivo.Substring(dato.T112_NombreArchivo.Length - 1) ; ///porque no obtenerlo del campo ciclo?
                        dato.GrupoCuenta = row.GetStringValue("GrupoCuenta");

                        //if (tablabin != null && tablabin.Rows.Count > 0)
                        //{
                        //    foreach (DataRow rowTable in tablabin.Rows)
                        //    {
                        //        if (dato.ClaveMA.Substring(0, 8) == rowTable["ClaveBin"].ToString())
                        //        {
                        //            dato.GrupoCuenta = rowTable["ID_GrupoCuenta"].ToString();
                        //            break;
                        //        }
                        //    }
                        //}

                        //Nuevos Campos Registro Compensacion
                        dato.CodigoFuncion = row.GetStringValue("C017");
                        dato.Reverso = row.GetStringValue("Reverso");
                        dato.Bin = row.GetStringValue("Bin");
                        dato.ImporteOrigen = (decimal.Parse(row["C006"].ToString()) / 100).ToString();
                        dato.ImporteDestino = (decimal.Parse(row["C008"].ToString()) / 100).ToString();
                        dato.MonedaCuotaIntercambio = row.GetStringValue("C056");
                        try
                        {
                            dato.MonedaIva = row["C050"].ToString().Substring(15, 3);
                        }
                        catch (Exception ex)
                        {
                            dato.MonedaIva = "";
                        }
                        dato.ImporteMCCR = ObtieneValorDecimal(row["C045"].ToString()).ToString(); //porque este no se divide entre 100?
                        dato.ImporteCompensacion = (decimal.Parse(row["C077"].ToString()) / 100).ToString();
                        dato.CodigoProceso = row["C001"].ToString();  ///se repite varias veces?..
                        dato.Representacion = row.GetStringValue("Representacion");

                        dato.SufijoArchivo = row["Sufijo"].ToString();

                        MisMovimientos.Add(dato);

                    }
                    catch (Exception err)
                    {
                        Log.Error("ObtieneMovimientos(): Formato de Registro no valido [" + regi + "] " + row.ToString() + ";" + err.Message);
                    }
                }



                return MisMovimientos;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private static decimal ObtieneValorDecimal(string de)
        {
            try
            {
                if (String.IsNullOrEmpty(de))
                    return 0.0M;

                if (String.IsNullOrWhiteSpace(de))
                    return 0.0M;


                if (decimal.Parse(de) == 0.0M)
                    return 0.0M;

                return decimal.Parse(de) / 100;


            }
            catch (Exception ex)
            {
                Log.Error("Error en la obtencion del valor Decimal del campo " + ex.Message);
                return 0.0M;
            }
        }

        private static string GetFechaOperacion(Movimiento dato, string v, string fechaPresentacion)
        {
            try
            {
                if (dato.ClaveEvento == "19" || dato.ClaveEvento == "29")
                {
                    DateTime dt = new DateTime();

                    if (DateTime.TryParse(v, out dt))
                    {
                        return v;
                    }
                    else
                    {
                        //T112_V2_MI112202006017.txt
                        var tempo = fechaPresentacion.Length == 8 ? fechaPresentacion.Substring(0, 4) + "-" + fechaPresentacion.Substring(4, 2) + "-" + fechaPresentacion.Substring(6) : fechaPresentacion;
                        if (DateTime.TryParse(tempo, out dt))
                        {
                            return fechaPresentacion;
                        }
                        else
                        {
                            return v;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error al generar la fecha de operacion para el caso " + dato.IDFicheroDetalle + " retornando : " + v);
            }

            return v;

        }

        public static void ObtieneYRegistraReporteMUC()
        {
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDEscrituraAutorizador);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_COMP_RegistraClientesReporteMUC");
                command.CommandTimeout = 600;
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        //CSOs
        public static DataTable ObtenerListaCSOs(string estatus)
        {
            DataTable tabLst = new DataTable();
            try
            {
                SqlParameter param;
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDEscrituraAutorizador);
                DbCommand command = database.GetStoredProcCommand("PROCNOC_COMP_ObtieneRelacionesCSO");
                command.CommandTimeout = 600;

                param = new SqlParameter("@cveEstatusRelacionCSO", SqlDbType.VarChar);
                param.Value = estatus;
                command.Parameters.Add(param);

                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);

                tabLst = losDatos2.Tables[0];

                Log.Evento("[ObtenerListaCSOs] [Se encontraron : " + tabLst.Rows.Count + "]");

            }
            catch (Exception ex)
            {
                Log.Error("[ObtenerListaCSOs] [Estatus] [" + estatus + "]" + ex.Message);
            }

            return tabLst;
        }

        public static void CancelarPolizasRelacionCSO(SqlConnection connOperacion, SqlTransaction transaccionSQL, Int64 idRelacionCSO
                        , Int64 idRegistroComp, Int64 idOperacion)
        {
            SqlDataReader SqlReader = null;

            try
            {
                SqlParameter param;
                SqlCommand comando = new SqlCommand("PROCNOC_COMP_CancelaPolizasRelacionCSO", connOperacion);
                comando.Transaction = transaccionSQL;
                comando.CommandType = CommandType.StoredProcedure;


                param = new SqlParameter("@idRelacionCSO", SqlDbType.BigInt);
                param.Value = idRelacionCSO;
                comando.Parameters.Add(param);

                param = new SqlParameter("@idRegistroCompensacion", SqlDbType.BigInt);
                param.Value = idRegistroComp;
                comando.Parameters.Add(param);

                param = new SqlParameter("@idOperacion", SqlDbType.BigInt);
                param.Value = idOperacion;
                comando.Parameters.Add(param);

                SqlReader = comando.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logueo.Error("[CancelarPolizasRelacionCSO] [IdRelacionCSO: " + idRelacionCSO + "] " +
                                "[IdRegistroComp: " + idRegistroComp + "] " +
                                "[IdOperacion: " + idOperacion + "] " + ex.Message);
                throw new Exception("[CancelarPolizasRelacionCSO] [" + ex.Message + "]", ex);
            }
            finally
            {
                try
                {
                    SqlReader.Close();
                }
                catch (Exception err) { }
            }
        }

        public static void ActualizarEstatusRelacionCSO(SqlConnection connOperacion, SqlTransaction transaccionSQL, Int64 idRelacionCSO
                        , string estatus)
        {
            SqlDataReader SqlReader = null;

            try
            {
                SqlParameter param;
                SqlCommand comando = new SqlCommand("PROCNOC_COMP_ActualizaEstatusRelacionCSO", connOperacion);
                comando.Transaction = transaccionSQL;
                comando.CommandType = CommandType.StoredProcedure;


                param = new SqlParameter("@idRelacionCSO", SqlDbType.BigInt);
                param.Value = idRelacionCSO;
                comando.Parameters.Add(param);

                param = new SqlParameter("@cveEstatusRelacionCSO", SqlDbType.VarChar);
                param.Value = estatus;
                comando.Parameters.Add(param);

                SqlReader = comando.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logueo.Error("[ActualizarEstatusRelacionCSO] [IdRelacionCSO: " + idRelacionCSO + "] " +
                                "[Estatus: " + estatus + "]");
                throw new Exception("[ActualizarEstatusRelacionCSO] [" + ex.Message + "]", ex);
            }
            finally
            {
                try
                {
                    SqlReader.Close();
                }
                catch (Exception err) { }
            }
        }

        #region Metodos V6
        public static List<Colectiva> ObtenerColectivas(string connConsulta, string etiquetaLogueo)
        {
            List<Colectiva> lstColectivas = new List<Colectiva>();
            Colectiva colectiva;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CMP_ObtieneColectivas";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                colectiva = new Colectiva()
                                {
                                    IdColectiva = reader["idColectiva"].ToString(),
                                    CveColectiva = reader["cveColectiva"].ToString(),
                                    CveTipoColectiva = reader["cveTipoColectiva"].ToString(),
                                    DescColectiva = reader["colectiva"].ToString()
                                };

                                lstColectivas.Add(colectiva);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }
                return lstColectivas;

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);

                return lstColectivas;
            }
        }

        public static List<ClaveContrato> ObtenerValoresContratos(string connConsulta, string etiquetaLogueo)
        {
            List<ClaveContrato> lstClavesContrato = new List<ClaveContrato>();
            ClaveContrato ValoresContrato;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CMP_ObtieneValoresContrato";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ValoresContrato = new ClaveContrato()
                                {
                                    idValorContrato = reader["idValorContrato"].ToString(),
                                    cveValorContrato = reader["cveValorContrato"].ToString(),
                                    descripcionContrato = reader["valorContrato"].ToString()
                                };

                                lstClavesContrato.Add(ValoresContrato);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }
                return lstClavesContrato;

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);

                return lstClavesContrato;
            }
        }

        public static ValorContrato ObtenerValorContrato(string cveColectiva, string nombreContrato, string connConsulta, string etiquetaLogueo)
        {
            ValorContrato ValorContrato = null;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CMP_ObtieneValorContrato";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@cveColectiva", SqlDbType.VarChar,100);
                        param.Value = cveColectiva;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@valorContrato", SqlDbType.VarChar, 5000);
                        param.Value = nombreContrato;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ValorContrato = new ValorContrato()
                                {
                                    NombreParametro = reader["NombreParametro"].ToString(),
                                    ID_TipoColectiva = reader["ID_TipoColectiva"].ToString(),
                                    TipoDato = reader["TipoDato"].ToString(),
                                    ClaveTipoColectiva = reader["ClaveTipoColectiva"].ToString(),
                                    Valor = reader["Valor"].ToString()
                                };

                            }
                        }
                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                        
                        if (!EvaluarRespuestaSPS(respuestaGral))
                        {
                            ValorContrato = null;
                        }
                    }

                }
                return ValorContrato;

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
                ValorContrato = null;
                return ValorContrato;
            }
        }

        public static List<Producto> ObtenerProductos(string connConsulta, string cveColectiva, string etiquetaLogueo)
        {
            List<Producto> lstProductos = new List<Producto>();
            Producto producto;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CMP_ObtieneProductos";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        command.Parameters.Add(new SqlParameter("@cveColectiva", cveColectiva));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                producto = new Producto()
                                {
                                    IdProducto = reader["idProducto"].ToString(),
                                    CveProducto = reader["cveProducto"].ToString(),
                                    CveColectiva = reader["cveColectiva"].ToString(),
                                    CveDivisa = reader["cveDivisa"].ToString(),
                                    DescProducto = reader["producto"].ToString(),
                                    GrupoCuenta = reader["grupoCuenta"].ToString()
                                };

                                lstProductos.Add(producto);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }
                }
            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }

            return lstProductos;
        }

        public static List<ClaveParametroMultiasignacion> ObtenerParametrosMultiasignacion(string connConsulta, string etiquetaLogueo)
        {
            List<ClaveParametroMultiasignacion> lstParametrosMultiasignacion = new List<ClaveParametroMultiasignacion>();
            ClaveParametroMultiasignacion parametrosMultiasignacion;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CMP_ObtieneParametrosMultiasignacion";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);


                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                parametrosMultiasignacion = new ClaveParametroMultiasignacion()
                                {
                                    idParametroMultiasignacion = reader["idParametroMultiasignacion"].ToString(),
                                    cveParametroMultiasignacion = reader["cveParametroMultiasignacion"].ToString(),
                                    parametroMultiasignacion = reader["parametroMultiasignacion"].ToString()
                                };

                                lstParametrosMultiasignacion.Add(parametrosMultiasignacion);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);

                    }

                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }

            return lstParametrosMultiasignacion;
        }

        public static ValorParametroMultiasignacion ObtenerParametroMultiasignacion(string cveProducto, string nombreParametroMultiasignacion, string connConsulta, string etiquetaLogueo)
        {
            ValorParametroMultiasignacion valorParametroMultiasignacion = null;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CMP_ObtieneParametroMultiasignacion";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@cveProducto", SqlDbType.VarChar,500);
                        param.Value = cveProducto;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@parametroMultiasignacion", SqlDbType.VarChar, 500);
                        param.Value = nombreParametroMultiasignacion;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                valorParametroMultiasignacion = new ValorParametroMultiasignacion()
                                {
                                    NombreParametro = reader["NombreParametro"].ToString(),
                                    ID_TipoColectiva = reader["ID_TipoColectiva"].ToString(),
                                    TipoDato = reader["TipoDato"].ToString(),
                                    ClaveTipoColectiva = reader["ClaveTipoColectiva"].ToString(),
                                    Valor = reader["Valor"].ToString()
                                };
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);

                        int resp = Convert.ToInt32(respuestaGral.CodRespuesta);
                        if (!(resp >= 200 && resp <= 299))
                        {
                            throw new Exception($"[Respuesta invalida: {resp}]");
                        }
                    }
                }
                return valorParametroMultiasignacion;

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
                valorParametroMultiasignacion = null;
                return valorParametroMultiasignacion;
            }
        }

        public static List<RegistroCompensar> ObtenerRegistrosPorCompensar(string connConsulta, string cveProducto
                        , string etiquetaLogueo)
        {
            List<RegistroCompensar> lstRegComp = new List<RegistroCompensar>();
            RegistroCompensar registroCompensar;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CMP_ObtieneRegistrosPorCompensar";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        command.Parameters.Add(new SqlParameter("@cveProducto", cveProducto));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                registroCompensar = new RegistroCompensar()
                                {
                                    cveColectiva = reader["cveColectiva"].ToString(),
                                    descColectiva = reader["colectiva"].ToString(),
                                    cveProducto = reader["cveProducto"].ToString(),
                                    grupoCuenta = reader["grupoCuenta"].ToString(),
                                    bin = reader["bin"].ToString(),
                                    idFichero = reader["idFichero"].ToString(),
                                    fichero = reader["fichero"].ToString(),
                                    fechaPresentacion = reader["fechaPresentacion"].ToString(),
                                    ciclo = reader["ciclo"].ToString(),
                                    idFicheroDetalle = reader["idFicheroDetalle"].ToString(),
                                    codigoProceso = reader["codigoProceso"].ToString(),
                                    codigoFuncion = reader["codigoFuncion"].ToString(),
                                    reverso = reader["reverso"].ToString(),
                                    representacion = reader["representacion"].ToString(),
                                    tarjeta = reader["tarjeta"].ToString(),
                                    tarjetaEnmascarada = reader["tarjetaEnmascarada"].ToString(),
                                    MedioAcceso = reader["tarjeta"].ToString(),
                                    TipoMedioAcceso = reader["cveTipoMedioAcceso"].ToString(),
                                    autorizacion = reader["autorizacion"].ToString(),
                                    referencia = reader["referencia"].ToString(),
                                    referencia2 = reader["referencia2"].ToString(),
                                    fechaJuliana = reader["fechaJuliana"].ToString(),
                                    fechaOperacion = reader["fechaOperacion"].ToString(),
                                    horaOperacion = reader["horaOperacion"].ToString(),
                                    impOrigen = reader["impOrigen"].ToString(),
                                    cveDivisaOrigen = reader["cveDivisaOrigen"].ToString(),
                                    impDestino = reader["impDestino"].ToString(),
                                    cveDivisaDestino = reader["cveDivisaDestino"].ToString(),
                                    impCuotaIntercambio = reader["impCuotaIntercambio"].ToString(),
                                    cveDivisaCuotaIntercambio = reader["cveDivisaCuotaIntercambio"].ToString(),
                                    impIva = reader["impIva"].ToString(),
                                    cveDivisaIva = reader["cveDivisaIva"].ToString(),
                                    impMCCR = reader["impMCCR"].ToString(),
                                    cveDivisaMCCR = reader["cveDivisaMCCR"].ToString(),
                                    impMarkUp = reader["impMarkUp"].ToString(),
                                    cveDivisaMarkUp = reader["cveDivisaMarkUp"].ToString(),
                                    impCompensacion = reader["impCompensacion"].ToString(),
                                    cveDivisaCompensacion = reader["cveDivisaCompensacion"].ToString(),
                                    tipoCambioProcesador = reader["tipoCambioProcesador"].ToString(),
                                    comercio = reader["comercio"].ToString(),
                                    comercioCiudad = reader["comercioCiudad"].ToString(),
                                    comercioPais = reader["comercioPais"].ToString(),
                                    comercioMCC = reader["comercioMCC"].ToString(),
                                    comercioCP = reader["comercioCP"].ToString(),
                                    comercioEstado = reader["comercioEstado"].ToString(),
                                    transaccionDestino = reader["transaccionDestino"].ToString(),
                                    transaccionOrigen = reader["transaccionOrigen"].ToString(),
                                    cveEstatusFicheroDetalle = reader["cveEstatusFicheroDetalle"].ToString(),
                                    cveTipoAlias = reader["cveTipoAlias"].ToString(),
                                    cveTipoRed = reader["cveTipoRed"].ToString(),

                                    tipoCambio = reader["tipoCambio"].ToString(),
                                    codigoPostal = reader["codigoPostal"].ToString(),
                                    estado = reader["estado"].ToString(),
                                    importeOriginal = reader["importeOriginal"].ToString(),
                                    importeComisiones = reader["importeComisiones"].ToString(),
                                    exponentesMoneda = reader["exponentesMoneda"].ToString(),
                                    referenciaEmisor = reader["referenciaEmisor"].ToString(),
                                    identificadorMensaje = reader["identificadorMensaje"].ToString(),
                                    codigoAdquirente = reader["codigoAdquirente"].ToString(),
                                    codigoDocumento = reader["codigoDocumento"].ToString(),
                                    codigoRazonMensaje = reader["codigoRazonMensaje"].ToString(),
                                    actividadNegocio = reader["actividadNegocio"].ToString(),
                                    modoEntradaPos = reader["modoEntradaPos"].ToString(),
                                    tipoTerminal = reader["tipoTerminal"].ToString(),
                                    numeroControl = reader["numeroControl"].ToString(),
                                    indicadorLiquidacion = reader["indicadorLiquidacion"].ToString(),
                                    indicadorDocumentacion = reader["indicadorDocumentacion"].ToString(),

                                    identificadorTransaccion = reader["identificadorTransaccion"].ToString()
                                };

                                lstRegComp.Add(registroCompensar);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }

            return lstRegComp;
        }

        public static void ObtenerColectivasRelacionadas(string connConsulta, ref Dictionary<String, Parametro> lstParametros, string etiquetaLogueo)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            string nomParamTemp = null;
            try
            {
                string sp = "PN_CMP_ObtieneColectivasRelacionadas";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        SqlParameter paramSSN = command.CreateParameter();
                        paramSSN.ParameterName = "@tarjeta";
                        paramSSN.DbType = DbType.AnsiStringFixedLength;
                        paramSSN.Direction = ParameterDirection.Input;
                        paramSSN.Value = lstParametros["@tarjeta"].Valor;
                        paramSSN.Size = lstParametros["@tarjeta"].Valor.Length;
                        command.Parameters.Add(paramSSN);

                        command.Parameters.Add(new SqlParameter("@tarjetaEnmascarada", lstParametros["@tarjetaEnmascarada"].Valor));

                        command.Parameters.Add(new SqlParameter("@switchProcesador", lstParametros["@switchProcesador"].Valor));


                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                nomParamTemp = reader["NombreParametro"].ToString();
                                if (lstParametros.ContainsKey(nomParamTemp))
                                {
                                    lstParametros[nomParamTemp].ID_TipoColectiva = Convert.ToInt32(reader["ID_TipoColectiva"]);
                                    lstParametros[nomParamTemp].Valor = reader["Valor"].ToString();
                                    lstParametros[nomParamTemp].ClaveTipoColectiva = reader["ClaveTipoColectiva"].ToString();
                                }
                                else
                                {
                                    Parametro parametro = new Parametro();
                                    parametro.Nombre = nomParamTemp;
                                    parametro.ID_TipoColectiva = Convert.ToInt32(reader["ID_TipoColectiva"]);
                                    parametro.Valor = reader["Valor"].ToString();
                                    parametro.ClaveTipoColectiva = reader["ClaveTipoColectiva"].ToString();

                                    lstParametros.Add(parametro.Nombre, parametro);
                                }
                                

                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        public static void InsertarSolicitudesReportes(string connConsulta, string cveColectivaEmisor, string cveColectivaSubEmisor, string fechaPresentacion,
                                                string etiquetaLogueo)
        {
            try
            {
                string sp = "PN_RPT_InsertaSolicitudesReportes";
                RespuestaGral respuestaGral = new RespuestaGral();

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        command.Parameters.Add(new SqlParameter("@cveColectiva_Emisor", cveColectivaEmisor));
                        command.Parameters.Add(new SqlParameter("@cveColectiva_SubEmisor", cveColectivaSubEmisor));
                        command.Parameters.Add(new SqlParameter("@fechaPresentacion", fechaPresentacion));

                        command.ExecuteNonQuery();

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }
                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        public static List<SolicitudReporte> ObtenerSolicitudesReportes(string connConsulta, string etiquetaLogueo)
        {
            List<SolicitudReporte> lstSolicitudesReportes = new List<SolicitudReporte>();
            SolicitudReporte solicitudReporte;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_RPT_ObtieneSolicitudesReportes";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                solicitudReporte = new SolicitudReporte()
                                {
                                    ID_SolicitudReporte = reader["idSolicitudReporte"].ToString(),
                                    ID_Reporte = reader["idReporte"].ToString(),
                                    ClaveReporte = reader["cveReporte"].ToString(),
                                    ID_EstatusSolicitudReporte = reader["idEstatusSolicitudReporte"].ToString(),
                                    ClaveEstatusSolicitudReporte = reader["cveEstatusSolicitudReporte"].ToString(),
                                    ClavePlugIn = reader["cvePlugIn"].ToString(),
                                    FechaPresentacion = reader["fechaPresentacion"].ToString(),
                                    FechaGeneracion = reader["fechaGeneracion"].ToString()
                                };

                                lstSolicitudesReportes.Add(solicitudReporte);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);

            }

            return lstSolicitudesReportes;
        }

        public static bool InsertarSolicitudReporteAuto(SolicitudReporte solicitudReporte, string connConsulta, string etiquetaLogueo, ref string idGeneracionReporte)
        {
            try
            {
                string sp = "PN_RPT_InsertaSolicitudReporte";
                RespuestaGral respuestaGral = new RespuestaGral();

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@cveReporte", SqlDbType.VarChar, 10);
                        param.Value = solicitudReporte.ClaveReporte;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@cvePlugIn", SqlDbType.VarChar, 10);
                        param.Value = solicitudReporte.ClavePlugIn;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@fechaPresentacion", SqlDbType.Date);
                        param.Value = solicitudReporte.FechaPresentacion;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@fechaGeneracion", SqlDbType.Date);
                        param.Value = solicitudReporte.FechaGeneracion;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@idGeneracionReporte", SqlDbType.BigInt);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        var resp = command.ExecuteNonQuery();

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);

                        idGeneracionReporte = command.Parameters["@idGeneracionReporte"].Value.ToString();

                        int codigo = Convert.ToInt32(respuestaGral.CodRespuesta);
                        if (codigo >= 200 && codigo <= 299)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
            return false;
        }

        public static void ActualizaEstatusSolicitudReporte(string idGeneracionReporte, string idSolcitudReporte, string cveEstatus_Destino, string connConsulta, string etiquetaLogueo)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_RPT_ActualizaEstatusSolicitudReporte";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@idSolicitudReporte", SqlDbType.BigInt);
                        param.Value = idSolcitudReporte;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@cveEstatus_Destino", SqlDbType.VarChar, 10);
                        param.Value = cveEstatus_Destino;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@folio", SqlDbType.VarChar, 20);
                        param.Value = idGeneracionReporte;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        var resp = command.ExecuteNonQuery();

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        public static void RestableceSolicitudesReportes(string connConsulta, string etiquetaLogueo)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_RPT_RestableceSolicitudesReportes";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        var resp = command.ExecuteNonQuery();

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }
                }
            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        public static bool ActualizarEstatusFicheroDetalle(string idFicheroDetalle, string cveEstatus_Destino, string connConsulta, string etiquetaLogueo, string error = null)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CMP_ActualizaEstatusFicheroDetalle";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@idFicheroDetalle", SqlDbType.BigInt);
                        param.Value = idFicheroDetalle;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@cveEstatus_Destino", SqlDbType.VarChar, 10);
                        param.Value = cveEstatus_Destino;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@error", SqlDbType.VarChar, 5000);
                        param.Value = error;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        var resp = command.ExecuteNonQuery();

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }

                return EvaluarRespuestaSPS(respuestaGral);
            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);

                return false;
            }
        }

        public static bool ObtenerInformacionOperacion(RegistroCompensar registroCompensar, string connConsulta
                                , string etiquetaLogueo, ref Dictionary<String, Parametro> lstParametros, ref string mensajeError)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CMP_ObtieneInformacionOperacion";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@cveColectiva", SqlDbType.VarChar, 100);
                        param.Value = registroCompensar.cveColectiva;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@grupoCuenta", SqlDbType.VarChar, 20);
                        param.Value = registroCompensar.grupoCuenta;
                        command.Parameters.Add(param);


                        // For parameters encrypt
                        SqlParameter paramSSN = command.CreateParameter();
                        paramSSN.ParameterName = "@tarjeta";
                        paramSSN.DbType = DbType.AnsiStringFixedLength;
                        paramSSN.Direction = ParameterDirection.Input;
                        paramSSN.Value = registroCompensar.tarjeta;
                        paramSSN.Size = registroCompensar.tarjeta.Length;
                        command.Parameters.Add(paramSSN);

                        param = new SqlParameter("@tarjetaEnmascarada", SqlDbType.VarChar);
                        param.Value = registroCompensar.tarjetaEnmascarada;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@autorizacion", SqlDbType.VarChar);
                        param.Value = registroCompensar.autorizacion;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@codigoProceso", SqlDbType.VarChar);
                        param.Value = registroCompensar.codigoProceso;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@reverso", SqlDbType.VarChar);
                        param.Value = registroCompensar.reverso;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@representacion", SqlDbType.VarChar);
                        param.Value = registroCompensar.representacion;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@cveTipoAlias", SqlDbType.VarChar);
                        param.Value = registroCompensar.cveTipoAlias;
                        command.Parameters.Add(param);


                        param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (lstParametros.ContainsKey(reader["NombreParametro"].ToString()))
                                {
                                    lstParametros[reader["NombreParametro"].ToString()].Valor = reader["Valor"].ToString();
                                }
                                else
                                {
                                    Parametro parametro = new Parametro();
                                    parametro.Nombre = reader["NombreParametro"].ToString();
                                    parametro.ID_TipoColectiva = Convert.ToInt32(reader["ID_TipoColectiva"]);
                                    parametro.Valor = reader["Valor"].ToString();
                                    parametro.ClaveTipoColectiva = reader["ClaveTipoColectiva"].ToString();

                                    lstParametros.Add(parametro.Nombre, parametro);
                                }


                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        mensajeError = respuestaGral.DescRespuesta;
                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }

                return EvaluarRespuestaSPS(respuestaGral);
            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
                mensajeError = "{\"Error\":\"" + err.Message + "\"}";
                return false;
            }
        }

        public static List<Variante> ObtenerVariantes(string cvePertenenciaTipo, Dictionary<String, Parametro> lstParametros, string connConsulta, string etiquetaLogueo)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            List<Variante> lstVariantes = new List<Variante>();
            Variante variante;
            try
            {
                string sp = "PN_CMP_ObtieneVariantes";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@cvePertenenciaTipo", SqlDbType.VarChar);
                        param.Value = cvePertenenciaTipo;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ParametroV6 parammetro = new ParametroV6()
                                {
                                    Nombre = reader["nombreParametro"].ToString(),
                                    Posicion = reader["posicion"].ToString(),
                                    InputOuput = reader["inputOuput"].ToString(),
                                    TipoDato = TipoDato.getTipoDatoSQL(reader["tipoDato"].ToString()),
                                    EsAccion = reader["esAccion"].ToString(),
                                    Valor = reader["valor"].ToString(),
                                };

                                variante = new Variante()
                                {
                                    Regla = reader["regla"].ToString(),
                                    IdPertenenciaVariante = reader["idPertenenciaVariante"].ToString(),
                                    CvePertenenciaVariante = reader["cvePertenenciaVariante"].ToString(),
                                    PertenenciaTipo = reader["pertenenciaTipo"].ToString(),
                                    IdPertenencia = reader["idPertenencia"].ToString(),
                                    IdTipoColectiva = reader["idTipoColectiva"].ToString(),
                                    CveTipoColectiva = reader["cveTipoColectiva"].ToString(),
                                    CveTipoVariante = reader["cveTipoVariante"].ToString(),
                                    TipoVariante = reader["tipoVariante"].ToString(),
                                    Ejecucion = reader["ejecucion"].ToString(),
                                    varianteFinal = reader["varianteFinal"].ToString().Equals("1")
                                                    ? true
                                                    : false
                                };

                                int varianteTemp = lstVariantes.FindIndex(elemento => elemento.Regla == variante.Regla);

                                if (varianteTemp < 0)
                                {
                                    variante.Parametros = new List<ParametroV6> { parammetro };
                                    lstVariantes.Add(variante);
                                }
                                else
                                {
                                    lstVariantes[varianteTemp].Parametros = (lstVariantes[varianteTemp].Parametros == null)
                                                                             ? new List<ParametroV6>()
                                                                             : lstVariantes[varianteTemp].Parametros;
                                    lstVariantes[varianteTemp].Parametros.Add(parammetro);
                                }
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }
                }

                if (EvaluarRespuestaSPS(respuestaGral) && ValidarParametrosVariantes(lstParametros, lstVariantes, etiquetaLogueo))
                    return lstVariantes;
                else
                {
                    throw new Exception("Ocurrio error al obtener las variantes");
                }

            }
            catch (Exception err)
            {
                throw new Exception(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        private static bool ValidarParametrosVariantes(Dictionary<String, Parametro> lstParametros, List<Variante> lstVariantes
                                                , string etiquetaLogueo)
        {
            RespuestaGral respuestaGralPar = new RespuestaGral();
            bool respParametrosVariantes = true;

            foreach (Variante variante in lstVariantes)
            {
                foreach (ParametroV6 unParametro in variante.Parametros)
                {
                    respuestaGralPar = new RespuestaGral();
                    if (lstParametros.ContainsKey(unParametro.Nombre))
                    {
                        unParametro.Valor = lstParametros[unParametro.Nombre].Valor;
                        string valorTemp = unParametro.Valor;
                        if (unParametro.Nombre.ToLower().Equals("@tarjeta") || unParametro.Nombre.ToLower().Equals("@medioacceso")) 
                        {
                            valorTemp = valorTemp.Length == 16
                                                ? valorTemp.Substring(0,4) + "******" + valorTemp.Substring(9,6)
                                                : valorTemp;
                        }

                        respuestaGralPar.CodRespuesta = "200";
                        respuestaGralPar.DescRespuesta = $"Descripcion,{unParametro.Nombre}:{valorTemp}";
                        LogueoSPS(respuestaGralPar, etiquetaLogueo);
                    }
                    else
                    {
                        Dictionary<string, string> msgError = new Dictionary<string, string>();

                        msgError.Add("responseCode", "404");
                        msgError.Add("responseDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                        msgError.Add("responseMessage", "Valor de parámetro para variante no obtenido");
                        msgError.Add("objectName", typeof(DAOComparador).Name);
                        msgError.Add("objectVersion", "6");
                        msgError.Add("objectType", "Clase");
                        respuestaGralPar.CodRespuesta = "404";
                        respuestaGralPar.DescRespuesta = JsonConvert.SerializeObject(msgError);
                        LogueoSPS(respuestaGralPar, etiquetaLogueo);

                        return respParametrosVariantes = false;
                    }
                }
                if (!EvaluarRespuestaSPS(respuestaGralPar))
                {
                    lstVariantes = new List<Variante>();

                    return respParametrosVariantes = false;
                }
            }

            return respParametrosVariantes;
        }

        public static RespuestaVarianteV6 EjecutarVariante(Variante variante, Dictionary<String, Parametro> lstParametros, string connConsulta, string etiquetaLogueo)
        {
            RespuestaVarianteV6 respuestaV6 = new RespuestaVarianteV6();
            try
            {
                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(variante.Regla, conn))
                    {
                        SqlParameter param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@cumpleVariante", SqlDbType.VarChar, 500);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@cveVariante", SqlDbType.VarChar, 500);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        foreach (ParametroV6 elParametro in variante.Parametros)
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            String MedioAcceso = string.Empty;
                            String elValorParametro = string.Empty;

                            switch (elParametro.TipoDato)
                            {
                                case TipoDatoSQL.VARCHAR:
                                    if (elParametro.Nombre == "@MedioAcceso")
                                    {
                                        MedioAcceso = elParametro.Valor;
                                        SqlParameter paramSSN = command.CreateParameter();
                                        paramSSN.ParameterName = elParametro.Nombre;
                                        paramSSN.DbType = DbType.AnsiStringFixedLength;
                                        paramSSN.Direction = ParameterDirection.Input;
                                        paramSSN.Value = elParametro.Valor;
                                        paramSSN.Size = elParametro.Valor.Length;
                                        command.Parameters.Add(paramSSN);
                                        break;
                                    }
                                    if (elParametro.Nombre == "@tarjeta")
                                    {
                                        MedioAcceso = elParametro.Valor;
                                        SqlParameter paramSSN = command.CreateParameter();
                                        paramSSN.ParameterName = elParametro.Nombre;
                                        paramSSN.DbType = DbType.AnsiStringFixedLength;
                                        paramSSN.Direction = ParameterDirection.Input;
                                        paramSSN.Value = elParametro.Valor;
                                        paramSSN.Size = elParametro.Valor.Length;
                                        command.Parameters.Add(paramSSN);
                                        break;
                                    }
                                    if (elParametro.Nombre == "@MedioAccesoDesencriptado")
                                    {
                                        param = new SqlParameter(elParametro.Nombre, SqlDbType.VarChar);
                                        param.Value = MedioAcceso;
                                        command.Parameters.Add(param);
                                        break;
                                    }
                                    else
                                    {
                                        param = new SqlParameter(elParametro.Nombre, SqlDbType.VarChar);
                                        param.Value = elParametro.Valor;
                                        command.Parameters.Add(param);
                                        break;
                                    }
                                case TipoDatoSQL.DATETIME:
                                case TipoDatoSQL.CHAR:

                                    param = new SqlParameter(elParametro.Nombre, SqlDbType.VarChar);
                                    param.Value = elParametro.Valor;
                                    command.Parameters.Add(param);
                                    break;
                                case TipoDatoSQL.INT:

                                    param = new SqlParameter(elParametro.Nombre, SqlDbType.Int);
                                    param.Value = elParametro.Valor;
                                    command.Parameters.Add(param);
                                    break;
                                case TipoDatoSQL.BIGINT:

                                    param = new SqlParameter(elParametro.Nombre, SqlDbType.BigInt);
                                    param.Value = elParametro.Valor;
                                    command.Parameters.Add(param);
                                    break;
                                case TipoDatoSQL.BIT:

                                    param = new SqlParameter(elParametro.Nombre, SqlDbType.Bit);
                                    param.Value = elParametro.Valor;
                                    command.Parameters.Add(param);
                                    break;
                                case TipoDatoSQL.DECIMAL:
                                case TipoDatoSQL.DOUBLE:

                                    param = new SqlParameter(elParametro.Nombre, SqlDbType.Decimal);
                                    param.Value = elParametro.Valor;
                                    command.Parameters.Add(param);
                                    break;
                                case TipoDatoSQL.FLOAT:

                                    param = new SqlParameter(elParametro.Nombre, SqlDbType.Float);
                                    param.Value = elParametro.Valor;
                                    command.Parameters.Add(param);
                                    break;
                                case TipoDatoSQL.TINYINT:

                                    param = new SqlParameter(elParametro.Nombre, SqlDbType.TinyInt);
                                    param.Value = elParametro.Valor;
                                    command.Parameters.Add(param);
                                    break;
                                case TipoDatoSQL.SMALLINT:

                                    param = new SqlParameter(elParametro.Nombre, SqlDbType.SmallInt);
                                    param.Value = elParametro.Valor;
                                    command.Parameters.Add(param);
                                    break;
                                default:
                                    String msjError = "No hay Mapearo de Tipo de Dato : " + elParametro.Nombre + " en la Variante " + variante.Regla + ".";
                                    Loguear.Error(msjError);
                                    break;
                            }
                        }

                        using (var respuestaVariante = command.ExecuteReader())
                        {
                            while (respuestaVariante.Read())
                            {
                                ParametroV6 nuevoParam = new ParametroV6();

                                nuevoParam.Nombre = (String)respuestaVariante["NombreParametro"];
                                nuevoParam.TipoDato = TipoDato.getTipoDatoSQL(respuestaVariante["TipoDato"].ToString());
                                nuevoParam.Valor = respuestaVariante["Valor"].ToString();

                                if (lstParametros.ContainsKey(nuevoParam.Nombre))
                                    lstParametros[nuevoParam.Nombre] = nuevoParam;
                                else
                                    lstParametros.Add(nuevoParam.Nombre, nuevoParam);

                            }
                        }

                        respuestaV6.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaV6.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();
                        respuestaV6.cumpleVariante = command.Parameters["@cumpleVariante"].Value.Equals("1")
                                                        ? true
                                                        : false;
                        respuestaV6.cveVariante = command.Parameters["@cveVariante"].Value.ToString();
                        respuestaV6.respuestaSP = EvaluarRespuestaSPS(respuestaV6);

                        LogueoSPS(respuestaV6, etiquetaLogueo);

                        if (!respuestaV6.respuestaSP)
                            return respuestaV6;

                        if (variante.varianteFinal && respuestaV6.cumpleVariante)
                            return respuestaV6;

                        return respuestaV6;
                    }
                }
            }
            catch (Exception err)
            {
                respuestaV6.CodRespuesta = "500";
                respuestaV6.DescRespuesta = err.Message;
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
                return respuestaV6;
            }

        }

        public static List<Evento> ObtenerEventos(string cvePertenenciaTipo, string cveVariante, string connConsulta
                                                , ref string cveEstatusDestinoOp, ref string mensajeErrorEventos, string etiquetaLogueo)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            List<Evento> lstEventos = new List<Evento>();
            Evento evento;
            try
            {
                string sp = "PN_CMP_ObtieneEventos";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@cvePertenenciaTipo", SqlDbType.VarChar);
                        param.Value = cvePertenenciaTipo;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@cveVariante", SqlDbType.VarChar);
                        param.Value = cveVariante;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@cveEstatusPostOperacionDestino", SqlDbType.VarChar, 20);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                evento = new Evento()
                                {
                                    IdEvento = reader["idEvento"].ToString(),
                                    CveEvento = reader["cveEvento"].ToString(),
                                    DescEvento = reader["evento"].ToString()
                                };

                                lstEventos.Add(evento);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();
                        cveEstatusDestinoOp = command.Parameters["@cveEstatusPostOperacionDestino"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);

                        if (!EvaluarRespuestaSPS(respuestaGral))
                            mensajeErrorEventos = respuestaGral.DescRespuesta;
                    }
                }

            }
            catch (Exception err)
            {
                mensajeErrorEventos = "[{{\"responseCode\":\"406\", \"responseDate\":\"" +
                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                                "\", \"responseMessage\":\"" + err.Message + "\", \"objectName\":\"ObtenerEventos\", " +
                                                "\"objectVersion\":\"1.0.0.0\", \"objectType\":Método\"}}]";
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }

            return lstEventos;
        }

        public static bool EvaluarRespuestaSPS(RespuestaGral respuestaGral)
        {
            try
            {
                int valCodigo = Convert.ToInt32(respuestaGral.CodRespuesta);
                if (valCodigo >= 200 && valCodigo <= 299)
                    return true;        //Regresa true ya que es una respuesta exitosa
                else
                    return false;       //Regresa false ya que es una respuesta de error
            }
            catch (Exception e)
            {
                return false;           //Regresa false por que hubo un error en la respuesta
            }

        }

        private static void LogueoSPS(RespuestaGral respuestaGral, string etiquetaLogueo)
        {
            int valCodigo = Convert.ToInt32(respuestaGral.CodRespuesta);
            if (valCodigo >= 500 && valCodigo <= 599)
                Log.Error(etiquetaLogueo + "[" + respuestaGral.DescRespuesta + "]");
            else
                Log.Evento(etiquetaLogueo + "[" + respuestaGral.DescRespuesta + "]");

        }

        /// <summary>
        /// Devuelve el nombre del metodo que llamo a este metodo, ejemplo el metodo consultaDato llama a este metodo el resultado es: consultaDato
        /// </summary>
        /// <returns>Nombre del metodo que lo invocó</returns>
        private static string NombreMetodo([CallerMemberName] string metodo = null)
        {
            return metodo;
        }
        #endregion
    }
}
