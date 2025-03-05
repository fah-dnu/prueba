using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Xml;
using CommonProcesador;
using Dnu_AutorizadorCacao_NCliente.Entidades;
using DNU_ParabiliaAltaTarjetasNominales.Entidades;
using DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;
using Executer.Entidades;
using Interfases.Entidades;
using Npgsql;

namespace DNU_ParabiliaAltaTarjetasNominales.BaseDatos
{
    public class BDsps
    {
        public static bool bulkInsertarDatosArchivoDetalle(DataTable tablaDetalles, string conn, string tablaAInsertar, string idFile, SqlTransaction transaction = null)
        {

            try
            {
                foreach (DataRow fila in tablaDetalles.Rows)
                {

                    fila["ID_Archivo"] = idFile;


                }
                tablaDetalles.AcceptChanges();

                using (SqlConnection connection = new SqlConnection(conn))
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                {
                    // Set up the column mappings by name.
                    SqlBulkCopyColumnMapping ClaveEmisor =
                        new SqlBulkCopyColumnMapping("ClaveCliente", "ClaveEmisor");
                    bulkCopy.ColumnMappings.Add(ClaveEmisor);

                    SqlBulkCopyColumnMapping FechaEnvio =
                        new SqlBulkCopyColumnMapping("FechaEnvio", "FechaEnvio");
                    bulkCopy.ColumnMappings.Add(FechaEnvio);

                    SqlBulkCopyColumnMapping ClaveEmpleadora =
                        new SqlBulkCopyColumnMapping("ClaveEmpleadora", "ClaveEmpleadora");
                    bulkCopy.ColumnMappings.Add(ClaveEmpleadora);

                    SqlBulkCopyColumnMapping ClaveBIN =
                        new SqlBulkCopyColumnMapping("ClaveBIN", "ClaveBIN");
                    bulkCopy.ColumnMappings.Add(ClaveBIN);


                    SqlBulkCopyColumnMapping NumeroEmpleado =
                        new SqlBulkCopyColumnMapping("NumeroEmpleado", "RegistroEmpleado");
                    bulkCopy.ColumnMappings.Add(NumeroEmpleado);

                    SqlBulkCopyColumnMapping Nombre =
                        new SqlBulkCopyColumnMapping("Nombre", "Nombre");
                    bulkCopy.ColumnMappings.Add(Nombre);

                    SqlBulkCopyColumnMapping PrimerApellido =
                        new SqlBulkCopyColumnMapping("PrimerApellido", "PrimerApellido");
                    bulkCopy.ColumnMappings.Add(PrimerApellido);

                    SqlBulkCopyColumnMapping SegundoApellido =
                        new SqlBulkCopyColumnMapping("SegundoApellido", "SegundoApellido");
                    bulkCopy.ColumnMappings.Add(SegundoApellido);

                    SqlBulkCopyColumnMapping NombreEmbozado =
                 new SqlBulkCopyColumnMapping("NombreEmbozado", "NombreEmbozado");
                    bulkCopy.ColumnMappings.Add(NombreEmbozado);

                    SqlBulkCopyColumnMapping Telefono =
                 new SqlBulkCopyColumnMapping("Telefono", "Telefono");
                    bulkCopy.ColumnMappings.Add(Telefono);

                    SqlBulkCopyColumnMapping Correo =
                 new SqlBulkCopyColumnMapping("Correo", "Correo");
                    bulkCopy.ColumnMappings.Add(Correo);

                    SqlBulkCopyColumnMapping ID_EstatusCACAO =
                 new SqlBulkCopyColumnMapping("ID_EstatusCACAO", "ID_EstatusCACAO");
                    bulkCopy.ColumnMappings.Add(ID_EstatusCACAO);

                    SqlBulkCopyColumnMapping ReintentosCACAO =
                 new SqlBulkCopyColumnMapping("ReintentosCACAO", "ReintentosCACAO");
                    bulkCopy.ColumnMappings.Add(ReintentosCACAO);

                    SqlBulkCopyColumnMapping TipoMedioAcceso =
                 new SqlBulkCopyColumnMapping("TipoMedioAcceso", "TipoMedioAcceso");
                    bulkCopy.ColumnMappings.Add(TipoMedioAcceso);

                    SqlBulkCopyColumnMapping MedioAcceso =
                 new SqlBulkCopyColumnMapping("MedioAcceso", "MedioAcceso");
                    bulkCopy.ColumnMappings.Add(MedioAcceso);


                    SqlBulkCopyColumnMapping TipoTarjeta =
                 new SqlBulkCopyColumnMapping("TipoTarjeta", "TipoTarjeta");
                    bulkCopy.ColumnMappings.Add(TipoTarjeta);

                    SqlBulkCopyColumnMapping TarjetaTitular =
                 new SqlBulkCopyColumnMapping("TarjetaTitular", "TarjetaTitular");
                    bulkCopy.ColumnMappings.Add(TarjetaTitular);

                    SqlBulkCopyColumnMapping TipoMedioAccesoTitular =
                 new SqlBulkCopyColumnMapping("TipoMedioAccesoTitular", "TipoMedioAccesoTitular");
                    bulkCopy.ColumnMappings.Add(TipoMedioAccesoTitular);

                    SqlBulkCopyColumnMapping MedioAccesoTitular =
                new SqlBulkCopyColumnMapping("MedioAccesoTitular", "MedioAccesoTitular");
                    bulkCopy.ColumnMappings.Add(MedioAccesoTitular);

                    SqlBulkCopyColumnMapping ID_Archivo =
                 new SqlBulkCopyColumnMapping("ID_Archivo", "ID_Archivo");
                    bulkCopy.ColumnMappings.Add(ID_Archivo);

                    SqlBulkCopyColumnMapping Subproducto =
                new SqlBulkCopyColumnMapping("Subproducto", "Subproducto");
                    bulkCopy.ColumnMappings.Add(Subproducto);

                    SqlBulkCopyColumnMapping Calle =
                new SqlBulkCopyColumnMapping("Calle", "Calle");
                    bulkCopy.ColumnMappings.Add(Calle);

                    SqlBulkCopyColumnMapping NoExterior =
                new SqlBulkCopyColumnMapping("NoExterior", "NoExterior");
                    bulkCopy.ColumnMappings.Add(NoExterior);

                    SqlBulkCopyColumnMapping NoInterior =
                new SqlBulkCopyColumnMapping("NoInterior", "NoInterior");
                    bulkCopy.ColumnMappings.Add(NoInterior);

                    SqlBulkCopyColumnMapping Colonia =
                new SqlBulkCopyColumnMapping("Colonia", "Colonia");
                    bulkCopy.ColumnMappings.Add(Colonia);

                    SqlBulkCopyColumnMapping DelegacionMun =
                new SqlBulkCopyColumnMapping("DelegacionMun", "DelegacionMun");
                    bulkCopy.ColumnMappings.Add(DelegacionMun);

                    SqlBulkCopyColumnMapping Ciudad =
                new SqlBulkCopyColumnMapping("Ciudad", "Ciudad");
                    bulkCopy.ColumnMappings.Add(Ciudad);

                    SqlBulkCopyColumnMapping Estado =
                new SqlBulkCopyColumnMapping("Estado", "Estado");
                    bulkCopy.ColumnMappings.Add(Estado);

                    SqlBulkCopyColumnMapping CP =
                new SqlBulkCopyColumnMapping("CP", "CP");
                    bulkCopy.ColumnMappings.Add(CP);

                    SqlBulkCopyColumnMapping Pais =
                new SqlBulkCopyColumnMapping("Pais", "Pais");
                    bulkCopy.ColumnMappings.Add(Pais);

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
                Logueo.Error("[EjecutarSP] [Error al ejecutar sp: " + procedimiento + "] [Conexion: " + connection + "] [Mensaje: " + e.Message + " TRACE: " + e.StackTrace + "]");
                return null;
            }
            finally
            {
                connection.Close();
            }

            return retorno;
        }

        public static DataSet EjecutarSP_Postgres(string procedimiento, Hashtable parametros, string pConexion)
        {
            DataSet retorno = new DataSet();
            NpgsqlConnection connection = new NpgsqlConnection(pConexion);
            try
            {

                connection.Open();
                NpgsqlCommand query = new NpgsqlCommand(procedimiento, connection);
                query.CommandType = CommandType.StoredProcedure;
                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                    }

                }

                NpgsqlDataAdapter sda = new NpgsqlDataAdapter(query);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional : null;
            }
            catch (Exception e)
            {
                Logueo.Error("[EjecutarSP] [Error al ejecutar sp: " + procedimiento + "] [Mensaje: " + e.Message + "]");
                return null;
            }
            finally
            {
                connection.Close();
            }

            return retorno;
        }

        public static void insertarRespuestaAltas(XmlDocument pResponse, SqlConnection pConn, string pIdDetalle, string pNombre)
        {


            SqlCommand query = new SqlCommand("procnoc_travel_InsertaRespuestaAltas", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@codRespuesta", pResponse.GetElementsByTagName("Codigo")[0].InnerText);
            SqlParameter paramSSN = query.CreateParameter();
            paramSSN.ParameterName = "@cuenta";
            paramSSN.DbType = DbType.AnsiStringFixedLength;
            paramSSN.Direction = ParameterDirection.Input;
            paramSSN.Value = SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Cuenta")[0].InnerText);
            paramSSN.Size = SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Cuenta")[0].InnerText).Length;
            query.Parameters.Add(paramSSN);
            //tarjeta
            SqlParameter paramSSNTar = query.CreateParameter();
            paramSSNTar.ParameterName = "@tarjeta";
            paramSSNTar.DbType = DbType.AnsiStringFixedLength;
            paramSSNTar.Direction = ParameterDirection.Input;
            paramSSNTar.Value = SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Tarjeta")[0].InnerText);
            paramSSNTar.Size = SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Tarjeta")[0].InnerText).Length;
            query.Parameters.Add(paramSSNTar);

            query.Parameters.AddWithValue("@descRespuesta", pResponse.GetElementsByTagName("Descripcion")[0].InnerText);
            query.Parameters.AddWithValue("@fecha", pResponse.GetElementsByTagName("Fecha")[0].InnerText);
            query.Parameters.AddWithValue("@hora", pResponse.GetElementsByTagName("Hora")[0].InnerText);
            query.Parameters.AddWithValue("@identificacion", SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Identificacion")[0].InnerText));
            query.Parameters.AddWithValue("@nombre", pNombre);
            query.Parameters.AddWithValue("@idArchivoDetalle", pIdDetalle);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
        }
        
        public static void insertarRespuestaCacaoAltasCredito(XmlDocument pResponse, SqlConnection pConn, string pIdDetalle, string pNombre, string tipoManufactura = null)
        {


            SqlCommand query = new SqlCommand("procnoc_travel_InsertaRespuestaCACAOAltasCredito", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@codRespuesta", pResponse.GetElementsByTagName("Codigo")[0].InnerText);
            query.Parameters.AddWithValue("@cuenta", SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Cuenta")[0].InnerText));
            query.Parameters.AddWithValue("@descRespuesta", pResponse.GetElementsByTagName("Descripcion")[0].InnerText);
            query.Parameters.AddWithValue("@fecha", pResponse.GetElementsByTagName("Fecha")[0].InnerText);
            query.Parameters.AddWithValue("@hora", pResponse.GetElementsByTagName("Hora")[0].InnerText);
            query.Parameters.AddWithValue("@identificacion", SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Identificacion")[0].InnerText));
            query.Parameters.AddWithValue("@nombre", pNombre);
            query.Parameters.AddWithValue("@tarjeta", SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("Tarjeta")[0].InnerText));
            query.Parameters.AddWithValue("@idArchivoDetalle", pIdDetalle);
            if ((!(tipoManufactura is null)) && tipoManufactura.ToUpper() == "V")
            {
                query.Parameters.AddWithValue("@tipoManufactura", tipoManufactura);
            }

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
        }


        public static String obtieneClaveMACacaoFromTarjeta(String tarjeta, string tipoMA)
        {
            String conexionSting = PNConfig.Get("ALTAEMPLEADODNU", "BDReadAutorizador");

            using (SqlConnection con = new SqlConnection(conexionSting))
            {
                using (SqlCommand query = new SqlCommand("Procnoc_ObtieneClaveMAFromTarjetaAndTipoClaveMA", con))
                {

                    query.CommandType = CommandType.StoredProcedure;
                   // query.Parameters.AddWithValue("@claveMedioAcceso", tarjeta);
                    query.Parameters.AddWithValue("@ClaveTipoMA", tipoMA);

                    SqlParameter paramSSNTar = query.CreateParameter();
                    paramSSNTar.ParameterName = "@claveMedioAcceso";
                    paramSSNTar.DbType = DbType.AnsiStringFixedLength;
                    paramSSNTar.Direction = ParameterDirection.Input;
                    paramSSNTar.Value = tarjeta;
                    paramSSNTar.Size =tarjeta.Length;
                    query.Parameters.Add(paramSSNTar);


                    con.Open();

                    var reader = query.ExecuteReader();
                    string sClaveMA = string.Empty;
                    while (reader.Read())
                    {
                        sClaveMA = reader["ClaveMA"].ToString();
                    }

                    if (!String.IsNullOrEmpty(sClaveMA))
                        return sClaveMA;
                    else
                        throw new Exception("No se localizó la claveMA tipo CACAO por Tarjeta " + tarjeta);
                }
            }
        }


        public static void insertarRespuestaAltasReproceso(SqlConnection pConn, string pIdDetalle, string claveEmpleadora
                                                            , string regEmpleado)
        {
            SqlCommand query = new SqlCommand("procnoc_travel_InsertaRespuestaAltasReproceso", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@idArchivoDetalle", pIdDetalle);
            query.Parameters.AddWithValue("@claveEmpleadora", claveEmpleadora);
            query.Parameters.AddWithValue("@regEmpleado", regEmpleado);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
        }

        public static void insertarRespuestaEvertecBajas(XmlDocument pResponse, SqlConnection pConn, string pIdDetalle)
        {
            SqlCommand query = new SqlCommand("procnoc_travel_InsertaRespuestaEvertecBajas", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@numeroTarjeta", SeguridadCifrado.cifrar(pResponse.GetElementsByTagName("numeroTarjeta")[0].InnerText));
            query.Parameters.AddWithValue("@estadoDeseado", pResponse.GetElementsByTagName("estadoDeseado")[0].InnerText);
            query.Parameters.AddWithValue("@motivoCancelacion", pResponse.GetElementsByTagName("motivoCancelacion")[0].InnerText);
            query.Parameters.AddWithValue("@codigoRespuesta", pResponse.GetElementsByTagName("codigoRespuesta")[0].InnerText);
            query.Parameters.AddWithValue("@descripcionRespuesta", pResponse.GetElementsByTagName("descripcionRespuesta")[0].InnerText);
            query.Parameters.AddWithValue("@idArchivoDetalle", pIdDetalle);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
        }


        public static DataSet insertarDatosParabiliaAltas(ParamParabiliaAltas pParametros, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("Procnoc_Travel_AltaParabilia", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@claveColectiva", pParametros.ClaveColectiva);
            query.Parameters.AddWithValue("@nombre", pParametros.Nombre);
            //query.Parameters.AddWithValue("@cuenta", SeguridadCifrado.descifrar(pParametros.NumeroCuenta));
            //query.Parameters.AddWithValue("@tarjeta", SeguridadCifrado.descifrar(pParametros.Tarjeta));

            //always encripted cuenta
            SqlParameter paramSSN = query.CreateParameter();
            paramSSN.ParameterName = "@cuenta";
            paramSSN.DbType = DbType.AnsiStringFixedLength;
            paramSSN.Direction = ParameterDirection.Input;
            paramSSN.Value = SeguridadCifrado.descifrar(pParametros.NumeroCuenta);
            paramSSN.Size = 50;
            query.Parameters.Add(paramSSN);
            //tarjeta
            SqlParameter paramSSNTar = query.CreateParameter();
            paramSSNTar.ParameterName = "@tarjeta";
            paramSSNTar.DbType = DbType.AnsiStringFixedLength;
            paramSSNTar.Direction = ParameterDirection.Input;
            paramSSNTar.Value = SeguridadCifrado.descifrar(pParametros.Tarjeta);
            paramSSNTar.Size = 50;
            query.Parameters.Add(paramSSNTar);

            query.Parameters.AddWithValue("@idArchivoDetalleAltas", pParametros.IdArchivoDetalle);
          
            query.Parameters.AddWithValue("@idEmpPadre", pParametros.IdEmpleadoraPadre);
            query.Parameters.AddWithValue("@fechaVencimiento", pParametros.FechaVencimientoTarjeta);
            // query.Parameters.AddWithValue("@correo", pParametros.correo);
            query.Parameters.AddWithValue("@telefono", pParametros.telefono);
            query.Parameters.AddWithValue("@tipoMedioAcceso", string.IsNullOrEmpty(pParametros.tipoMedioAcceso)
                                                              ? null
                                                              : pParametros.tipoMedioAcceso);
            query.Parameters.AddWithValue("@medioAcceso", string.IsNullOrEmpty(pParametros.medioAcceso)
                                                              ? null
                                                              : pParametros.medioAcceso);
            query.Parameters.AddWithValue("@nombreEmbozar", string.IsNullOrEmpty(pParametros.nombreEmbozar)
                                                              ? null
                                                              : pParametros.nombreEmbozar);
            query.Parameters.AddWithValue("@tipoManufactura", string.IsNullOrEmpty(pParametros.tipoManufactura)
                                                              ? null
                                                              : pParametros.tipoManufactura);
            query.Parameters.AddWithValue("@subproducto", string.IsNullOrEmpty(pParametros.SubProducto)
                                                        ? null
                                                        : pParametros.SubProducto);

            string tarjetaDecript = SeguridadCifrado.descifrar(pParametros.Tarjeta);
            query.Parameters.AddWithValue("@bin", tarjetaDecript.Length >= 8 ? tarjetaDecript.Substring(0, 8) : tarjetaDecript);
            //if (pParametros.correo.Contains("|"))
            //{
            //    pParametros.SubProducto = pParametros.correo.Split('|')[1];
            //    query.Parameters.AddWithValue("@subproducto", string.IsNullOrEmpty(pParametros.SubProducto)
            //                                                ? null
            //                                                : pParametros.SubProducto);
            //    pParametros.correo = pParametros.correo.Split('|')[0];
            //}
            query.Parameters.AddWithValue("@correo", pParametros.correo);
            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }


        public static DataSet insertarDatosParabiliaAltasV2(AltasRequest pParametros, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("[ws_Parabilium_AltaTarjetaBatch]", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@claveColectiva", pParametros.ClaveColectiva);
            query.Parameters.AddWithValue("@nombre", pParametros.Nombre);
           
            query.Parameters.AddWithValue("@nombreEmbozado", string.IsNullOrEmpty(pParametros.NomEmbozado)
                                                            ? null
                                                            : pParametros.NomEmbozado);
            query.Parameters.AddWithValue("@claveEmpresaPadre", pParametros.ClaveEmpresaPadre);
            query.Parameters.AddWithValue("@fechaVencimiento", pParametros.FechaVencimientoTarjeta);
            query.Parameters.AddWithValue("@correo", pParametros.correo);
            query.Parameters.AddWithValue("@telefono", pParametros.telefono);
            //tarjeta
            SqlParameter paramSSNTar = query.CreateParameter();
            paramSSNTar.ParameterName = "@tarjeta";
            paramSSNTar.DbType = DbType.AnsiStringFixedLength;
            paramSSNTar.Direction = ParameterDirection.Input;
            paramSSNTar.Value = SeguridadCifrado.descifrar(pParametros.Tarjeta);
            paramSSNTar.Size = SeguridadCifrado.descifrar(pParametros.Tarjeta).Length;
            query.Parameters.Add(paramSSNTar);

            //medio acceso
            if (!string.IsNullOrEmpty(pParametros.MedioAcceso)) {
                SqlParameter paramSSNMa = query.CreateParameter();
                paramSSNMa.ParameterName = "@medioAcceso";
                paramSSNMa.DbType = DbType.AnsiStringFixedLength;
                paramSSNMa.Direction = ParameterDirection.Input;
                paramSSNMa.Value = pParametros.MedioAcceso;
                paramSSNMa.Size = pParametros.MedioAcceso.Length;
                query.Parameters.Add(paramSSNMa);
            }
            query.Parameters.AddWithValue("@tipoMedioAcceso", string.IsNullOrEmpty(pParametros.TipoMedioAcceso)
                                                           ? null
                                                           : pParametros.TipoMedioAcceso);
            query.Parameters.AddWithValue("@subproducto", string.IsNullOrEmpty(pParametros.SubProducto)
                                                      ? null
                                                      : pParametros.SubProducto);

            query.Parameters.AddWithValue("@pApellido", string.IsNullOrEmpty(pParametros.PrimerApellido)
                                                     ? null
                                                     : pParametros.PrimerApellido);
            query.Parameters.AddWithValue("@sApellido", string.IsNullOrEmpty(pParametros.SegundoApellido)
                                                   ? null
                                                   : pParametros.SegundoApellido);
         
            query.Parameters.AddWithValue("@tipoManufactura", string.IsNullOrEmpty(pParametros.TipoManufactura)
                                                              ? null
                                                              : pParametros.TipoManufactura);

            //Direction Fields
            query.Parameters.AddWithValue("@calle", string.IsNullOrEmpty(pParametros.Calle)
                                                   ? null
                                                   : pParametros.Calle);
            query.Parameters.AddWithValue("@noExterior", string.IsNullOrEmpty(pParametros.NoExterior)
                                                   ? null
                                                   : pParametros.NoExterior);
            query.Parameters.AddWithValue("@noInterior", string.IsNullOrEmpty(pParametros.NoInterior)
                                                   ? null
                                                   : pParametros.NoInterior);
            query.Parameters.AddWithValue("@colonia", string.IsNullOrEmpty(pParametros.Colonia)
                                                   ? null
                                                   : pParametros.Colonia);
            query.Parameters.AddWithValue("@delegacionMun", string.IsNullOrEmpty(pParametros.DelegacionMun)
                                                   ? null
                                                   : pParametros.DelegacionMun);
            query.Parameters.AddWithValue("@ciudad", string.IsNullOrEmpty(pParametros.Ciudad)
                                                   ? null
                                                   : pParametros.Ciudad);
            query.Parameters.AddWithValue("@estado", string.IsNullOrEmpty(pParametros.Estado)
                                                   ? null
                                                   : pParametros.Estado);
            query.Parameters.AddWithValue("@cp", string.IsNullOrEmpty(pParametros.CP)
                                                   ? null
                                                   : pParametros.CP);
            query.Parameters.AddWithValue("@pais", string.IsNullOrEmpty(pParametros.Pais)
                                                   ? null
                                                   : pParametros.Pais);


            String hostName = "";
            try
            {
                // Get the local computer host name.
                hostName = Dns.GetHostName();
               
            }
            catch (Exception e)
            {
               
            }
            query.Parameters.AddWithValue("@usuarioServicio", hostName);
            query.Parameters.AddWithValue("@tarjetaBin", SeguridadCifrado.descifrar(pParametros.Tarjeta));
           

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }


        public static DataSet insertarDatosMA(string nuevacuenta, string idMedioAcceso, string tipoMedioAcceso, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("[ws_Parabilia_ObtenerDatosAltaMAParabilia]", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@idMedioAcceso", idMedioAcceso);
            query.Parameters.AddWithValue("@tipoMedioAcceso", tipoMedioAcceso);
       
            //tarjeta
            SqlParameter paramSSNTar = query.CreateParameter();
            paramSSNTar.ParameterName = "@medioAccesoNuevo";
            paramSSNTar.DbType = DbType.AnsiStringFixedLength;
            paramSSNTar.Direction = ParameterDirection.Input;
            paramSSNTar.Value = nuevacuenta;
            paramSSNTar.Size = nuevacuenta.Length;
            query.Parameters.Add(paramSSNTar);

           
            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }

        public static DataSet insertarDatosParabiliaAltasCredito(ParamParabiliaAltas pParametros, SqlConnection pConn, SqlTransaction transaccionSQL = null)
        {//Procnoc_Travel_AltaCreditoParabilia
            SqlCommand query = new SqlCommand("ws_Parabilia_AltaTarjetaOnbSinTrans", pConn, transaccionSQL);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@claveColectiva", pParametros.ClaveColectiva);
            query.Parameters.AddWithValue("@nombre", pParametros.Nombre);
            query.Parameters.AddWithValue("@tarjeta", SeguridadCifrado.descifrar(pParametros.Tarjeta));
            // query.Parameters.AddWithValue("@idArchivoDetalleAltas", pParametros.IdArchivoDetalle);
            query.Parameters.AddWithValue("@cuenta", SeguridadCifrado.descifrar(pParametros.NumeroCuenta));
            query.Parameters.AddWithValue("@claveEmpresaPadre", pParametros.IdEmpleadoraPadre);
            // query.Parameters.AddWithValue("@idEmpPadre", pParametros.IdEmpleadoraPadre);
            query.Parameters.AddWithValue("@fechaVencimiento", pParametros.FechaVencimientoTarjeta);
            query.Parameters.AddWithValue("@correo", pParametros.correo);
            query.Parameters.AddWithValue("@telefono", pParametros.telefono);
            query.Parameters.AddWithValue("@tipoMedioAcceso", string.IsNullOrEmpty(pParametros.tipoMedioAcceso)
                                                              ? null
                                                              : pParametros.tipoMedioAcceso);
            query.Parameters.AddWithValue("@medioAcceso", string.IsNullOrEmpty(pParametros.medioAcceso)
                                                              ? null
                                                              : pParametros.medioAcceso);
            query.Parameters.AddWithValue("@nombreEmbozado", string.IsNullOrEmpty(pParametros.nombreEmbozar)
                                                              ? null
                                                              : pParametros.nombreEmbozar);
            query.Parameters.AddWithValue("@tipoManufactura", string.IsNullOrEmpty(pParametros.tipoManufactura)
                                                              ? null
                                                              : pParametros.tipoManufactura);
            query.Parameters.AddWithValue("@cicloCorte", pParametros.CicloCorte == "00"
                                                             ? null
                                                             : pParametros.CicloCorte);
            query.Parameters.AddWithValue("@subProducto", pParametros.SubProducto);
            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }

        public static DataSet insertarDatosParabiliaBajas(ParamParabiliaBajas pParametros, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("Procnoc_Travel_BajaParabilia", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@claveColectiva", pParametros.ClaveColectiva);
            query.Parameters.AddWithValue("@numTarjeta", SeguridadCifrado.descifrar(pParametros.NumeroTarjeta));
            query.Parameters.AddWithValue("@idArchivoDetalleBajas", pParametros.ArchivoDetalleBajas);
            query.Parameters.AddWithValue("@claveEmpleadora", pParametros.ClaveEmpleadora);
            query.Parameters.AddWithValue("@claveTipoEmpleadora", pParametros.ClaveTipoEmpleadora);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }

        public static DataSet ObtenerDatosCuenta(string pCuenta, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("procnoc_travel_ObtieneDatosCuenta", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@numeroCuenta", SeguridadCifrado.descifrar(pCuenta));

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }


        public static Dictionary<String, String> ObtenerDatosContrato(String claveClectiva)
        {

            Hashtable htFile = new Hashtable();

            htFile.Add("@cveColectiva", claveClectiva);
            String conexionSting = PNConfig.Get("ALTAEMPLEADODNU", "BDReadAutorizador");
            Dictionary<String, String> _contrato = new Dictionary<string, string>();
            DataSet dt = BDsps.EjecutarSP("PROCNOC_Parabilia__ObtenerDatosContratoPorCveColectiva", htFile,
                conexionSting);

            if (dt == null)
            {
                throw new Exception(String.Format("No se localizo información del contrato con la colectiva {0}", claveClectiva));
            }

            if (dt.Tables[0].Rows.Count == 0)
            {
                throw new Exception(String.Format("No se localizo información del contrato con la colectiva {0}", claveClectiva));
            }

            foreach (DataRow item in
                dt.Tables[0].Rows)
            {
                if (item["Nombre"] == null || item["Valor"] == null)
                {
                    throw new Exception(String.Format("No se localizo información del contrato con la colectiva {0}", claveClectiva));
                }

                _contrato.Add(item["Nombre"].ToString(), item["Valor"].ToString());
            }

            return _contrato;
        }

        public static DataSet insertarDatosParabiliaAltasAdicional(ParamParabiliaAltas pParametros, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("Procnoc_Travel_AltaParabilia_Adicional", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@claveColectiva", pParametros.ClaveColectiva);
            query.Parameters.AddWithValue("@nombre", pParametros.Nombre);
            query.Parameters.AddWithValue("@tarjeta", SeguridadCifrado.descifrar(pParametros.Tarjeta));
            query.Parameters.AddWithValue("@idArchivoDetalleAltas", pParametros.IdArchivoDetalle);
            query.Parameters.AddWithValue("@cuenta", SeguridadCifrado.descifrar(pParametros.NumeroCuenta));
            query.Parameters.AddWithValue("@idEmpPadre", pParametros.IdEmpleadoraPadre);
            query.Parameters.AddWithValue("@fechaVencimiento", pParametros.FechaVencimientoTarjeta);
            query.Parameters.AddWithValue("@correo", pParametros.correo);
            query.Parameters.AddWithValue("@tarjetaTitular", pParametros.tarjetaTitular);
            query.Parameters.AddWithValue("@medioAcceso", pParametros.medioAcceso);
            query.Parameters.AddWithValue("@tipoMedioAcceso", pParametros.tipoMedioAcceso);
            query.Parameters.AddWithValue("@telefono", pParametros.telefono);
            query.Parameters.AddWithValue("@nombreEmbozar", string.IsNullOrEmpty(pParametros.nombreEmbozar)
                                                              ? null
                                                              : pParametros.nombreEmbozar);
            if (pParametros.correo.Contains("|"))
            {
                pParametros.SubProducto = pParametros.correo.Split('|')[1];
                query.Parameters.AddWithValue("@subproducto", string.IsNullOrEmpty(pParametros.SubProducto)
                                                            ? null
                                                            : pParametros.SubProducto);
            }

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }

        public static DataSet insertarDatosParabiliaAltasAdicionalCredito(ParamParabiliaAltas pParametros, SqlConnection pConn, SqlTransaction transaccionSQL = null)
        {
            SqlCommand query = new SqlCommand("Procnoc_Travel_AltaParabiliaCredito_Adicional", pConn, transaccionSQL);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@claveColectiva", pParametros.ClaveColectiva);
            query.Parameters.AddWithValue("@nombre", pParametros.Nombre);
            query.Parameters.AddWithValue("@tarjeta", SeguridadCifrado.descifrar(pParametros.Tarjeta));
            query.Parameters.AddWithValue("@idArchivoDetalleAltas", pParametros.IdArchivoDetalle);
            query.Parameters.AddWithValue("@cuenta", SeguridadCifrado.descifrar(pParametros.NumeroCuenta));
            query.Parameters.AddWithValue("@idEmpPadre", pParametros.IdEmpleadoraPadre);
            query.Parameters.AddWithValue("@fechaVencimiento", pParametros.FechaVencimientoTarjeta);
            query.Parameters.AddWithValue("@correo", pParametros.correo);
            query.Parameters.AddWithValue("@tarjetaTitular", pParametros.tarjetaTitular);
            query.Parameters.AddWithValue("@medioAcceso", pParametros.medioAcceso);
            query.Parameters.AddWithValue("@tipoMedioAcceso", pParametros.tipoMedioAcceso);
            query.Parameters.AddWithValue("@telefono", pParametros.telefono);
            query.Parameters.AddWithValue("@nombreEmbozar", string.IsNullOrEmpty(pParametros.nombreEmbozar)
                                                              ? null
                                                              : pParametros.nombreEmbozar);

            query.Parameters.AddWithValue("@subProducto", pParametros.SubProducto);
            query.Parameters.AddWithValue("@cicloCorte", pParametros.CicloCorte);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }



        public static void insertarRespuestaValidarTitular(string pCodidoResp, string pDescripcion,
                                SqlConnection pConn, string pIdDetalle, string pNombre)
        {


            SqlCommand query = new SqlCommand("procnoc_travel_InsertaRespuestaAltas", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@codRespuesta", pCodidoResp);
            query.Parameters.AddWithValue("@cuenta", null);
            query.Parameters.AddWithValue("@descRespuesta", pDescripcion);
            query.Parameters.AddWithValue("@fecha", "");
            query.Parameters.AddWithValue("@hora", "");
            query.Parameters.AddWithValue("@identificacion", "");
            query.Parameters.AddWithValue("@nombre", pNombre);
            query.Parameters.AddWithValue("@tarjeta", null);
            query.Parameters.AddWithValue("@idArchivoDetalle", pIdDetalle);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
        }


        public static DataSet validarTarjetaTitular(string pTarjetaTitular, string pTipoMATitular,
                                            string pMATitular, string pClaveEmpresa, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("Procnoc_Travel_ValidarAltaParabilia_Adicional", pConn);
            query.CommandType = CommandType.StoredProcedure;

            query.Parameters.AddWithValue("@tarjetaTitular", string.IsNullOrEmpty(pTarjetaTitular)
                                                              ? null
                                                              : pTarjetaTitular);
            query.Parameters.AddWithValue("@medioAccesoTitular", string.IsNullOrEmpty(pMATitular)
                                                              ? null
                                                              : pMATitular);
            query.Parameters.AddWithValue("@tipoMedioAccesoTitular", string.IsNullOrEmpty(pTipoMATitular)
                                                              ? null
                                                              : pTipoMATitular);
            query.Parameters.AddWithValue("@claveGrupo", pClaveEmpresa);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }
        public static DataSet obtenerDatosConsultaSubproducto(string subproducto, SqlConnection pConn)
        {
            SqlCommand query = new SqlCommand("ws_parabilia_verificaSubproducto", pConn);
            query.CommandType = CommandType.StoredProcedure;
            query.Parameters.AddWithValue("@subProducto", subproducto);


            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }
        public static DataSet insertarDatosParabiliaAltasCorte(ParamParabiliaAltas pParametros, string limiteCredito, SqlConnection pConn, SqlTransaction transaccionSQL = null)
        {
            SqlCommand query = new SqlCommand("ws_parabilia_generarCuentaCorteCredito", pConn, transaccionSQL);
            query.CommandType = CommandType.StoredProcedure;
            query.Parameters.AddWithValue("@tarjeta", SeguridadCifrado.descifrar(pParametros.Tarjeta));
            query.Parameters.AddWithValue("@limiteCredito", limiteCredito);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }

        public static DataSet obtenerDatosTarjetaEventoAsigLimCreditoParaExecute(ParamParabiliaAltas pParametros, SqlConnection pConn, SqlTransaction transaccionSQL = null)
        {
            SqlCommand query = new SqlCommand("ws_Parabilia_ObtenerDatosTarjetaAsignaLimiteCredito", pConn, transaccionSQL);
            query.CommandType = CommandType.StoredProcedure;
            query.Parameters.AddWithValue("@tarjeta", SeguridadCifrado.descifrar(pParametros.Tarjeta));
            query.Parameters.AddWithValue("@claveEvento", PNConfig.Get("ALTAEMPLEADODNU", "eventoAsignaLimiteCredito"));
            query.Parameters.AddWithValue("@importe", pParametros.LimiteCredito);

            SqlDataAdapter sda = new SqlDataAdapter(query);
            DataSet opcional = new DataSet();
            sda.Fill(opcional);
            return opcional.Tables.Count > 0 ? opcional : null;
        }
        public static Dictionary<String, Parametro> ObtenerDatosParametros(Bonificacion elAbono, DataTable datosDiccionario, Response respuesta, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {

                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                //Se consultan los parámetros del contrato
                if (transaccionSQL is null)
                {
                    //   losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                    //   (elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "");
                }
                else
                {
                    losParametros = ListaDeParamentrosContrato(elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "", respuesta, conn, transaccionSQL);
                }
                if (string.IsNullOrEmpty(elAbono.Concepto))
                {
                    if (elAbono.cuentaOrigen == null)
                    {
                        elAbono.Concepto = losParametros.Values.ElementAt(0).Valor;
                    }
                    else
                    {
                        try
                        {
                            string cuentas = ": Origen:***" + elAbono.cuentaOrigen.Substring((elAbono.cuentaOrigen.Length - 5), 5) + " Destino:***" + elAbono.cuentaDestino.Substring((elAbono.cuentaDestino.Length - 5), (5));
                            elAbono.Concepto = losParametros.Values.ElementAt(0).Valor + cuentas;
                        }
                        catch (Exception ex)
                        {
                            elAbono.Concepto = losParametros.Values.ElementAt(0).Valor;
                        }
                    }

                }
                if (string.IsNullOrEmpty(elAbono.Observaciones))
                {
                    elAbono.Observaciones = "";
                }
                if (elAbono.RefNumerica == null)
                {
                    elAbono.RefNumerica = 0;
                }

                foreach (DataRow fila in datosDiccionario.Rows)
                {
                    if (!string.IsNullOrEmpty(fila["idTipocolectiva"].ToString()))
                    {
                        losParametros[fila["nombre"].ToString()] = new Parametro()
                        {
                            Nombre = fila["nombre"].ToString(),
                            Valor = fila["valor"].ToString(),
                            Descripcion = fila["descripcion"].ToString(),
                            ID_TipoColectiva = Convert.ToInt32(fila["idTipocolectiva"].ToString())
                        };
                    }
                    else
                    {
                        losParametros[fila["nombre"].ToString()] = new Parametro()
                        {
                            Nombre = fila["nombre"].ToString(),
                            Valor = fila["valor"].ToString(),
                            Descripcion = fila["descripcion"].ToString(),
                        };


                    }

                }

                return losParametros;

            }

            catch (Exception err)
            {
                //transaccionSQLTraspaso.Rollback();
                throw err;
            }
        }
        public static Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta, String ClaveEvento, String elUsuario, Response respuesta, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {

            try
            {
                SqlCommand query = new SqlCommand("ws_Ejecutor_ObtieneValoresContratos", conn, transaccionSQL);
                query.CommandType = CommandType.StoredProcedure;
                query.Parameters.AddWithValue("@ClaveCadenaComercial", ClaveCadenaComercial);
                query.Parameters.AddWithValue("@ClaveEvento", ClaveEvento);
                query.Parameters.AddWithValue("@Tarjeta", Tarjeta);

                SqlDataAdapter sda = new SqlDataAdapter(query);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                DataTable Respuesta = opcional.Tables["Table"];
                //return opcional.Tables.Count > 0 ? opcional : null;

                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();


                //database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                //database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
                //database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);

                //laTabla = database.ExecuteDataSet(command, transaccionSQL).Tables[0];

                for (int k = 0; k < Respuesta.Rows.Count; k++)
                {
                    Parametro unParamentro = new Parametro();
                    unParamentro.Nombre = (Respuesta.Rows[k]["Nombre"]).ToString();
                    unParamentro.ID_TipoColectiva = Int16.Parse((Respuesta.Rows[k]["ID_TipoColectiva"]).ToString());
                    unParamentro.Valor = (Respuesta.Rows[k]["valor"]).ToString();
                    larespuesta.Add(unParamentro.Nombre, unParamentro);

                }

                return larespuesta;
            }
            catch (Exception ex)
            {
                respuesta.CodRespuesta = "9091";
                respuesta.DescRespuesta = "Error al obtener los contratos";
                throw ex;
            }
        }
        public static bool RealizarFondeoORetiroTraspaso(Bonificacion elAbono, Dictionary<String, Parametro> losParametros, String fecha, Response errores,
                                    SqlTransaction transaccionSQLTraspaso, SqlConnection conn, RespuestaPoliza respuestaPoliza)
        {
            bool polizaCreada = false;
            try
            {
                try
                {
                    Poliza laPoliza = null;

                    //Genera y Aplica la Poliza
                    Executer.EventoManual aplicador = new Executer.EventoManual(elAbono.IdEvento,
                    elAbono.Concepto, false, Convert.ToInt64(elAbono.RefNumerica), losParametros, elAbono.Observaciones, conn, transaccionSQLTraspaso);
                    laPoliza = aplicador.AplicaContablilidad();
                    respuestaPoliza.fechaPoliza = laPoliza.FechaCreacion;
                    respuestaPoliza.idPoliza = laPoliza.ID_Poliza.ToString();

                    if (laPoliza.CodigoRespuesta != 0)
                    {
                        if (laPoliza.CodigoRespuesta.ToString() == "1016")
                        {
                            errores.DescRespuesta = "Fondos Insuficientes";
                            errores.CodRespuesta = "1016";
                            return false;
                        }
                        errores.CodRespuesta = laPoliza.CodigoRespuesta.ToString();
                        errores.DescRespuesta = laPoliza.DescripcionRespuesta.Replace("[EventoManual]", "").Replace("NO", "No").Replace(laPoliza.CodigoRespuesta.ToString(), "").Replace("Error:", "").Trim();
                        throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                    }

                    else
                    {
                        polizaCreada = true;
                        Logueo.Evento("Se Realizó la Bonificación de Puntos a la Cuenta de la Colectiva: " + elAbono.IdColectiva.ToString());
                    }
                }

                catch (Exception err)
                {
                    throw err;
                }
            }

            catch (Exception err)
            {
                throw new Exception("RegistraEvManual_AbonoPuntos() " + err.Message);
            }

            return polizaCreada;
        }
    }
}
