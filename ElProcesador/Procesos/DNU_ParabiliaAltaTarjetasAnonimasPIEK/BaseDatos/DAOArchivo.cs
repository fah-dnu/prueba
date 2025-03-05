using CommonProcesador;
using DNU_ParabiliaAltaTarjetasAnonimasPIEK.Entidades;
using DNU_ParabiliaAltaTarjetasAnonimasPIEK.Utilidades;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;


namespace DNU_ParabiliaAltaTarjetasAnonimasPIEK.BaseDatos
{
    public class DAOArchivo
    {


        internal static DataTable GeneraDataTableDetalle(List<Archivo> elArchivo, long iD_Fichero, string subproducto = null)
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

        internal static bool ColectivaValida(string claveColectiva, SqlConnection conn, string log, string ip)
        {
            try
            {
                var result = false;
                using (SqlCommand command = new SqlCommand("proc_ValidaColectiva", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    int resp = -1;

                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_ValidaColectiva" + claveColectiva + "]");
                    command.Parameters.Add(new SqlParameter("@ClaveColectiva", claveColectiva));
                    var param = new SqlParameter("@Resultado", result);
                    param.Direction = ParameterDirection.Output;

                    command.Parameters.Add(param);



                    resp = command.ExecuteNonQuery();

                    result = Convert.ToBoolean(command.Parameters["@Resultado"].Value);
                    return result;
                }

            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return false;
            }
        }

        internal static string[] AltaTarjeta(Archivo item, String claveColectiva
                        , string tipoManufactura, String connString, int longitudBin
                        , bool tarjetaActiva = false, string colectivaCuentahabiente = null
                        , SqlConnection connSQL = null, SqlTransaction transaccionSQL = null
                        , string subproducto = null, string log="", string ip="")
        {
           
            try
            {
                var result = "99";

                string procedimientoAlmacenado = "";
                string[] datosArchivo = item.RawData.Split('|');
                if (tarjetaActiva && datosArchivo[1] == "00000")
                {
                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [ejecutando sps Procnoc_Travel_AltaParabiliaAutomaticoExternoRepos]");
                    procedimientoAlmacenado = "Procnoc_Travel_AltaParabiliaAutomaticoExternoRepos";
                }
                else
                {
                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [ejecutando sps Procnoc_Travel_AltaParabiliaAutomaticoCredito]");
                   procedimientoAlmacenado = "Procnoc_Travel_AltaParabiliaAutomaticoCredito";// "Procnoc_Travel_AltaParabiliaAutomaico";
                }

                // using (SqlConnection conLocal = new SqlConnection(connString))
                // {
                // using (
                SqlCommand command = new SqlCommand(procedimientoAlmacenado, connSQL, transaccionSQL);//)
                                                                                                      //    {
                command.CommandType = CommandType.StoredProcedure;
                //  command.Connection = conLocal;


                //command.Parameters.Add(new SqlParameter("@ClaveColectiva", claveColectiva));
                command.Parameters.Add(new SqlParameter("@nombre", item.Tarjeta));
               // command.Parameters.Add(new SqlParameter("@tarjeta", item.NumeroTarjeta));
               // command.Parameters.Add(new SqlParameter("@cuenta", item.Cuenta));
                command.Parameters.Add(new SqlParameter("@idEmpPadre", claveColectiva));
                command.Parameters.Add(new SqlParameter("@fechaVencimiento", item.Vencimiento));
                command.Parameters.Add(new SqlParameter("@tipoManufactura", tipoManufactura));
                command.Parameters.Add(new SqlParameter("@subproducto", subproducto));
                command.Parameters.Add(new SqlParameter("@bin", item.NumeroTarjeta.Substring(0, longitudBin)));
                //parametros always encripted
                SqlParameter paramSSN = command.CreateParameter();
                paramSSN.ParameterName = "@tarjeta";
                paramSSN.DbType = DbType.AnsiStringFixedLength;
                paramSSN.Direction = ParameterDirection.Input;
                paramSSN.Value = item.NumeroTarjeta;
                paramSSN.Size =50;
                command.Parameters.Add(paramSSN);

                SqlParameter paramSSNCuenta = command.CreateParameter();
                paramSSNCuenta.ParameterName = "@cuenta";
                paramSSNCuenta.DbType = DbType.AnsiStringFixedLength;
                paramSSNCuenta.Direction = ParameterDirection.Input;
                paramSSNCuenta.Value = item.Cuenta;
                paramSSNCuenta.Size = 50;
                command.Parameters.Add(paramSSNCuenta);

                if (tarjetaActiva)
                {
                    command.Parameters.Add(new SqlParameter("@activo", 1));
                    command.Parameters.Add(new SqlParameter("@colectivaCuentahabiente", colectivaCuentahabiente));

                }

                var codResultado = new SqlParameter("@ResultadoCodigo", SqlDbType.VarChar, 5);
                codResultado.Value = "";
                codResultado.Direction = ParameterDirection.Output;

                command.Parameters.Add(codResultado);

                var mensajeResultado = new SqlParameter("@ResultadoMensaje", SqlDbType.VarChar, 200);
                mensajeResultado.Value = "";
                mensajeResultado.Direction = ParameterDirection.Output;
                command.Parameters.Add(mensajeResultado);
                //always encripted
                var mensajeCuentaCacao = new SqlParameter("@CuentaCacao", SqlDbType.VarChar, 50);
                mensajeCuentaCacao.Value = "";
                mensajeCuentaCacao.Direction = ParameterDirection.Output;
                command.Parameters.Add(mensajeCuentaCacao);

                var mensajeIdMaCACAO = new SqlParameter("@IdMaCACAO", SqlDbType.BigInt);
                mensajeIdMaCACAO.Value = 0;
                mensajeIdMaCACAO.Direction = ParameterDirection.Output;
                command.Parameters.Add(mensajeIdMaCACAO);

                var mensajTipoMa = new SqlParameter("@TipoMARegreso", SqlDbType.VarChar, 50);
                mensajTipoMa.Value = "";
                mensajTipoMa.Direction = ParameterDirection.Output;
                command.Parameters.Add(mensajTipoMa);





                //var mensajeResultadoToken = new SqlParameter("@tokenTar", SqlDbType.VarChar, 200);
                //mensajeResultadoToken.Value = "";
                //mensajeResultadoToken.Direction = ParameterDirection.Output;

                //command.Parameters.Add(mensajeResultadoToken);

                //credito
                var colectivaHC = new SqlParameter("@ClaveColectivaCH", SqlDbType.VarChar, 200);
                colectivaHC.Value = "";
                colectivaHC.Direction = ParameterDirection.Output;

                command.Parameters.Add(colectivaHC);
                //fin credito

                //conLocal.Open();
                var resp = command.ExecuteNonQuery();


                return new string[] { command.Parameters["@ResultadoCodigo"].Value.ToString(),
                    command.Parameters["@ResultadoMensaje"].Value.ToString(), "",command.Parameters["@CuentaCacao"].Value.ToString(),command.Parameters["@IdMaCACAO"].Value.ToString(),command.Parameters["@TipoMARegreso"].Value.ToString()};//command.Parameters["@tokenTar"].Value.ToString()};
                //  }
                //}

            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO][procedimientoTarjeta] [" + log + "] [" + err.Message + "]");
                return new string[] { "99", err.Message, "" };
            }
        }

        internal static  DataTable  AgregarMedioAcceso(string cuentaCacao,string idMaCACAO,string tipoMARegreso, SqlConnection connSQL = null, SqlTransaction transaccionSQL = null, string subproducto = null, string log="", string ip="")
        {
       
            try
            {
                var result = "99";

                string procedimientoAlmacenado = "";
         
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [ejecutando sps ws_Parabilia_ObtenerDatosAltaMAParabiliaSNTran]");
                procedimientoAlmacenado = "[ws_Parabilia_ObtenerDatosAltaMAParabiliaSNTran]";// "Procnoc_Travel_AltaParabiliaAutomaico";
            

                
                SqlCommand command = new SqlCommand(procedimientoAlmacenado, connSQL, transaccionSQL);//)
                                                                                                      //    {
                command.CommandType = CommandType.StoredProcedure;
              
                command.Parameters.Add(new SqlParameter("@idMedioAcceso", idMaCACAO));

               // command.Parameters.Add(new SqlParameter("@medioAccesoNuevo", cuentaCacao));
                SqlParameter paramSSN = command.CreateParameter();
                paramSSN.ParameterName = "@medioAccesoNuevo";
                paramSSN.DbType = DbType.AnsiStringFixedLength;
                paramSSN.Direction = ParameterDirection.Input;
                paramSSN.Value = cuentaCacao;
                paramSSN.Size = 50;
                command.Parameters.Add(paramSSN);

                command.Parameters.Add(new SqlParameter("@tipoMedioAcceso", tipoMARegreso));
               

                //conLocal.Open();
              //  var resp = command.ExecuteNonQuery();

                using (SqlDataAdapter sda = new SqlDataAdapter(command))
                {
                    DataSet opcional = new DataSet();
                    sda.Fill(opcional);
                    return opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
                }//Dispose SqlDataAdapter

            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                DataTable table = new DataTable();
                table.Columns.Add("tipo", typeof(string));
                table.Columns.Add("Mesaje", typeof(string));
                table.Columns.Add("Codigo", typeof(string));
                table.Columns.Add("MesajeReal", typeof(string));
                table.Rows.Add("error", "Error en base de datos", err.Message, "9999");
                return table;
            }
        }

        internal static string[] AltaTarjetaExterna(Archivo item, String claveColectiva, string tipoManufactura, String connString, bool tarjetaActiva = false, string colectivaCuentahabiente = null, string log="", string ip="")
        {
           
            try
            {
                var result = "99";

                string procedimientoAlmacenado = "";
                string[] datosArchivo = item.RawData.Split('|');
                if (tarjetaActiva && datosArchivo[1] == "00000")
                {
                    procedimientoAlmacenado = "Procnoc_Travel_AltaParabiliaAutomaticoExternoRepos";
                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Procnoc_Travel_AltaParabiliaAutomaticoExternoRepos]");

                }
                else
                {
                    procedimientoAlmacenado = "Procnoc_Travel_AltaParabiliaAutomaticoExterno";
                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Procnoc_Travel_AltaParabiliaAutomaticoExterno]");
                }

                using (SqlConnection conLocal = new SqlConnection(connString))
                {
                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;


                        //command.Parameters.Add(new SqlParameter("@ClaveColectiva", claveColectiva));
                        command.Parameters.Add(new SqlParameter("@nombre", item.Tarjeta));
                        command.Parameters.Add(new SqlParameter("@tarjeta", item.NumeroTarjeta));
                        command.Parameters.Add(new SqlParameter("@cuenta", item.Cuenta));
                        command.Parameters.Add(new SqlParameter("@idEmpPadre", claveColectiva));
                        command.Parameters.Add(new SqlParameter("@fechaVencimiento", item.Vencimiento));
                        command.Parameters.Add(new SqlParameter("@tipoManufactura", tipoManufactura));
                        if (tarjetaActiva)
                        {
                            command.Parameters.Add(new SqlParameter("@activo", 1));
                            command.Parameters.Add(new SqlParameter("@colectivaCuentahabiente", colectivaCuentahabiente));

                        }

                        var codResultado = new SqlParameter("@ResultadoCodigo", SqlDbType.VarChar, 5);
                        codResultado.Value = "";
                        codResultado.Direction = ParameterDirection.Output;

                        command.Parameters.Add(codResultado);

                        var mensajeResultado = new SqlParameter("@ResultadoMensaje", SqlDbType.VarChar, 200);
                        mensajeResultado.Value = "";
                        mensajeResultado.Direction = ParameterDirection.Output;

                        command.Parameters.Add(mensajeResultado);

                        conLocal.Open();
                        var resp = command.ExecuteNonQuery();


                        return new string[] { command.Parameters["@ResultadoCodigo"].Value.ToString(),
                    command.Parameters["@ResultadoMensaje"].Value.ToString()};
                    }
                }

            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return new string[] { "99", err.Message };
            }
        }

        //alta credito
        internal static string[] AltaTarjetaCredito(Archivo item, String claveColectiva, string tipoManufactura, string subproducto, SqlConnection connSQL = null, SqlTransaction transaccionSQL = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                var result = "99";

                // using (SqlConnection conLocal = new SqlConnection(connString))
                //{
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Procnoc_Travel_AltaParabiliaAutomaticoCredito]");
                SqlCommand command = new SqlCommand("Procnoc_Travel_AltaParabiliaAutomaticoCredito", connSQL, transaccionSQL);

                //using (SqlCommand command = new SqlCommand("Procnoc_Travel_AltaParabiliaAutomaticoCredito",connSQL,transaccionSQL))
                //{
                command.CommandType = CommandType.StoredProcedure;
                //  command.Connection = conLocal;


                //command.Parameters.Add(new SqlParameter("@ClaveColectiva", claveColectiva));
                command.Parameters.Add(new SqlParameter("@emisor", item.Emisor));
                command.Parameters.Add(new SqlParameter("@subproducto", subproducto));
                command.Parameters.Add(new SqlParameter("@nombre", item.Tarjeta));
                command.Parameters.Add(new SqlParameter("@tarjeta", item.NumeroTarjeta));
                command.Parameters.Add(new SqlParameter("@cuenta", item.Cuenta));
                command.Parameters.Add(new SqlParameter("@idEmpPadre", claveColectiva));
                command.Parameters.Add(new SqlParameter("@fechaVencimiento", item.Vencimiento));
                command.Parameters.Add(new SqlParameter("@tipoManufactura", tipoManufactura));

                var codResultado = new SqlParameter("@ResultadoCodigo", SqlDbType.VarChar, 5);
                codResultado.Value = "";
                codResultado.Direction = ParameterDirection.Output;

                command.Parameters.Add(codResultado);

                var mensajeResultado = new SqlParameter("@ResultadoMensaje", SqlDbType.VarChar, 200);
                mensajeResultado.Value = "";
                mensajeResultado.Direction = ParameterDirection.Output;

                command.Parameters.Add(mensajeResultado);
                //variable clavecolectivacuentahabiente
                var mensajeColectiva = new SqlParameter("@ClaveColectivaCH", SqlDbType.VarChar, 200);
                mensajeColectiva.Value = "";
                mensajeColectiva.Direction = ParameterDirection.Output;

                command.Parameters.Add(mensajeColectiva);

                //conLocal.Open();
                var resp = command.ExecuteNonQuery();



                return new string[] { command.Parameters["@ResultadoCodigo"].Value.ToString(),
                    command.Parameters["@ResultadoMensaje"].Value.ToString(), "" };//command.Parameters["@ClaveColectivaCH"].Value.ToString()};
                //}
                // }

            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
               return new string[] { "99", err.Message };
            }
        }


        internal static bool GuardarFicheroDetallesEnBD(DataTable dtDetalle, string log="", string ip="")
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
                        LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                        Console.WriteLine(ex.Message);
                    }
                }

                return true;

            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [GuardarFicheroDetallesEnBD(): ERROR:" + err.Message + "]");
                return false;
            }
        }

        public static List<ArchivoConfiguracion> ObtenerArchivosConfigurados(string log,string ip)
        {
          
            List<ArchivoConfiguracion> losArchivos = new List<ArchivoConfiguracion>();
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Proc_ObtieneConfigArchivosSinFilas]");
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
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
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
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_InsertaFichero]");
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
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return 0;
            }
        }


        internal static string[] ObtenerClabeTarjeta(String tarjeta, String tipoMA, String connString, string log, string ip)
        {
           
            try
            {
                var result = "99";

                using (SqlConnection conLocal = new SqlConnection(connString))
                {
                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Procnoc_ObtieneClaveMAFromTarjetaAndTipoClaveMA]");
                    using (SqlCommand command = new SqlCommand("Procnoc_ObtieneClaveMAFromTarjetaAndTipoClaveMA"))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;


                        //command.Parameters.Add(new SqlParameter("@ClaveColectiva", claveColectiva));
                        command.Parameters.Add(new SqlParameter("@claveMedioAcceso", tarjeta));
                        command.Parameters.Add(new SqlParameter("@ClaveTipoMA", tipoMA));


                        conLocal.Open();
                        var reader = command.ExecuteReader();
                        string[] clabe = new string[0];
                        string sClaveMA = string.Empty;
                        while (reader.Read())
                        {
                            sClaveMA = reader["ClaveMA"].ToString();
                        }

                        if (!String.IsNullOrEmpty(sClaveMA))
                        {
                            return new string[] { "00", sClaveMA };
                        }
                        else
                        {
                            string tar = tarjeta.Substring(0, 6) + "******" + tarjeta.Substring(12, 4);
                            LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [ObtenerClabeTarjeta, No se encontró " + tipoMA + " para tarjeta: " + tar + "]");
                            return new string[] { "99", "------------------" };
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return new string[] { "99", err.Message };
            }
        }

        public static DataTable validaExistenciaSubproducto(string subproducto, string conexion, string log, string ip)
        {
           
            DataTable retorno = new DataTable();
           
            try
            {


                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_parabilia_verificaSubproducto]");
                    using (SqlCommand command = new SqlCommand("ws_parabilia_verificaSubproducto"))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;

                        command.Parameters.Add(new SqlParameter("@subProducto", subproducto));
                        command.Parameters.Add(new SqlParameter("@procesoStock", "1"));


                        conLocal.Open();
                        SqlDataAdapter sda = new SqlDataAdapter(command);
                        DataSet opcional = new DataSet();
                        sda.Fill(opcional);
                        retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;

                    }
                }

            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                retorno = null;
            }
            return retorno;
            // return losArchivos;
        }

        public static DataTable resultadoAltaCorte(string subproducto, string tarjeta, SqlConnection connSQL = null, SqlTransaction transaccionSQL = null, string log="", string ip="")
        {
           
            DataTable retorno = new DataTable();
            //  List<ArchivoConfiguracion> losArchivos = new List<ArchivoConfiguracion>();
            try
            {


                //  using (SqlConnection conLocal = new SqlConnection(conexion))
                // {
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_parabilia_generarCuentaCorteCredito]");
                SqlCommand command = new SqlCommand("ws_parabilia_generarCuentaCorteCredito", connSQL, transaccionSQL);
                //using (SqlCommand command = new SqlCommand("ws_parabilia_generarCuentaCorteCredito", connSQL,transaccionSQL))
                //{
                command.CommandType = CommandType.StoredProcedure;
                //    command.Connection = conLocal;


                command.Parameters.Add(new SqlParameter("@tarjeta", tarjeta));
                command.Parameters.Add(new SqlParameter("@limiteCredito", "0.00"));


                //  conLocal.Open();
                SqlDataAdapter sda = new SqlDataAdapter(command);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;

                //}
            }

            //            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                 retorno = null;
            }
            return retorno;
            // return losArchivos;
        }

        public static DataTable resultadoObtieneDatosTarjetaExterna(string subproducto, string tarjeta, string conexion, string log, string ip)
        {
            
            DataTable retorno = new DataTable();
            //  List<ArchivoConfiguracion> losArchivos = new List<ArchivoConfiguracion>();
            try
            {


                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    //SqlCommand command = new SqlCommand("ws_parabilia_obtenerDatosManufacturaCuentahabiente");
                    LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_parabilia_obtenerDatosManufacturaCuentahabiente]");
                    using (SqlCommand command = new SqlCommand("ws_parabilia_obtenerDatosManufacturaCuentahabiente"))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;


                        command.Parameters.Add(new SqlParameter("@tarjeta", tarjeta));


                        conLocal.Open();
                        SqlDataAdapter sda = new SqlDataAdapter(command);
                        DataSet opcional = new DataSet();
                        sda.Fill(opcional);
                        retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
                    }
                }
            }
            catch (Exception err)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                retorno = null;
            }
            return retorno;
            // return losArchivos;
        }


    }
}
