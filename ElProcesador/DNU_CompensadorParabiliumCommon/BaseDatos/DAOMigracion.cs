using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Entidades.Response;
using DNU_CompensadorParabiliumCommon.Utilidades;
using Interfases.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    public class DAOMigracion
    {
        public static List<Catalogo> ObtenerCatalogos(string connConsulta, string etiquetaLogueo)
        {
            List<Catalogo> lstCatalogos = new List<Catalogo>();
            Catalogo catalogo;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_SNC_ObtieneInformacion";

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
                                catalogo = new Catalogo()
                                {
                                    CveTipoColectiva = reader["cveTipoColectiva"].ToString(),
                                    TipoColectiva = reader["tipoColectiva"].ToString(),
                                    CveColectiva = reader["cveColectiva"].ToString(),
                                    Colectiva = reader["colectiva"].ToString(),
                                    CveTipoProducto = reader["cveTipoProducto"].ToString(),
                                    TipoProducto = reader["tipoProducto"].ToString(),
                                    CveProducto = reader["cveProducto"].ToString(),
                                    Producto = reader["producto"].ToString(),
                                    CveDivisa = reader["cveDivisa"].ToString(),
                                    GrupoCuenta = reader["grupoCuenta"].ToString(),
                                    Bin = reader["bin"].ToString(),
                                    FechaActivacion = reader["fechaActivacion"].ToString(),
                                    Activo = reader["activo"].ToString(),
                                    Longitud = reader["longitud"].ToString()
                                };

                                lstCatalogos.Add(catalogo);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }
                return lstCatalogos;

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
                return lstCatalogos;
            }
        }

        public static void ActualizarCatalogos(string connConsulta, List<Catalogo> lstCatalogos, string etiquetaLogueo)
        {
            try
            {
                string sp = "PN_SNC_SincronizaInformacion";
                RespuestaGral respuestaGral = new RespuestaGral();

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();

                    foreach (var catalogo in lstCatalogos)
                    {
                        using (SqlCommand command = new SqlCommand(sp, conn))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                            param2.Direction = ParameterDirection.Output;
                            command.Parameters.Add(param2);

                            SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar,5000);
                            param3.Direction = ParameterDirection.Output;
                            command.Parameters.Add(param3);

                            command.Parameters.Add(new SqlParameter("@cveTipoColectiva", catalogo.CveTipoColectiva));
                            command.Parameters.Add(new SqlParameter("@tipoColectiva", catalogo.TipoColectiva));
                            command.Parameters.Add(new SqlParameter("@cveColectiva", catalogo.CveColectiva));
                            command.Parameters.Add(new SqlParameter("@colectiva", catalogo.Colectiva));
                            command.Parameters.Add(new SqlParameter("@cveTipoProducto", catalogo.CveTipoProducto));
                            command.Parameters.Add(new SqlParameter("@tipoProducto", catalogo.TipoProducto));
                            command.Parameters.Add(new SqlParameter("@cveProducto", catalogo.CveProducto));
                            command.Parameters.Add(new SqlParameter("@producto", catalogo.Producto));
                            command.Parameters.Add(new SqlParameter("@cveDivisa", catalogo.CveDivisa));
                            command.Parameters.Add(new SqlParameter("@grupoCuenta", catalogo.GrupoCuenta));
                            command.Parameters.Add(new SqlParameter("@bin", catalogo.Bin));
                            command.Parameters.Add(new SqlParameter("@fechaActivacion", Convert.ToDateTime(catalogo.FechaActivacion).ToString("yyyy-MM-dd")));
                            command.Parameters.Add(new SqlParameter("@activo", catalogo.Activo));


                            command.ExecuteNonQuery();

                            respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                            respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                            LogueoSPS(respuestaGral, etiquetaLogueo);
                        }
                    }
                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        public static void RestablecerRegistros(string connConsulta, int timeOut, string etiquetaLogueo)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_RRE_RestableceRegistros";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeOut;

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
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        public static List<RegistroConsolidar> ObtenerRegistrosConsolidar(string connConsulta, int timeout, string etiquetaLogueo)
        {
            List<RegistroConsolidar> lstRegConsolidar = new List<RegistroConsolidar>();
            RegistroConsolidar regConsolidar;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CNS_ObtieneRegistrosPorConsolidar";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

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
                                regConsolidar = new RegistroConsolidar()
                                {
                                    IdTipoArchivo = reader["idTipoArchivo"].ToString(),
                                    CveTipoArchivo = reader["cveTipoArchivo"].ToString(),
                                    TipoArchivo = reader["tipoArchivo"].ToString(),
                                    IdArchivo = reader["idArchivo"].ToString(),
                                    CveArchivo = reader["cveArchivo"].ToString(),
                                    Archivo = reader["archivo"].ToString(),
                                    IdFicheroTemp = reader["idFicheroTemp"].ToString(),
                                    FicheroTemp = reader["ficheroTemp"].ToString(),
                                    IdFicheroProvisional = reader["idFicheroProvisional"].ToString(),
                                    CveEstatusFicheroProvisional = reader["cveEstatusFicheroProvisional"].ToString(),
                                    SpConsolidacion = reader["spConsolidacion"].ToString()
                                };

                                lstRegConsolidar.Add(regConsolidar);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }
                return lstRegConsolidar;

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
                return lstRegConsolidar;
            }
        }

        public static void ConsolidarRegistro(string connConsulta, RegistroConsolidar registro, int timeout, string etiquetaLogueo)
        {
            try
            {
                string sp = "PN_CNS_ConsolidaRegistro";
                RespuestaGral respuestaGral = new RespuestaGral();

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;
                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        command.Parameters.Add(new SqlParameter("@idFicheroProvisional", registro.IdFicheroProvisional));
                        command.Parameters.Add(new SqlParameter("@spConsolidacion", registro.SpConsolidacion));

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

        public static void RestablecerRegistrosPorConsolidar(string connConsulta, int timeout, string etiquetaLogueo)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_CNS_RestableceRegistrosPorConsolidar";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;
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
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        public static List<RegistroHomologar> ObtenerRegistrosHomologar(string connConsulta, int timeout, string etiquetaLogueo)
        {
            List<RegistroHomologar> lstRegHomologar = new List<RegistroHomologar>();
            RegistroHomologar regHomologar;
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_HML_ObtieneRegistrosPorHomologar";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;
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
                                regHomologar = new RegistroHomologar()
                                {
                                    IdTipoArchivo = reader["idTipoArchivo"].ToString(),
                                    CveTipoArchivo = reader["cveTipoArchivo"].ToString(),
                                    TipoArchivo = reader["tipoArchivo"].ToString(),
                                    IdArchivo = reader["idArchivo"].ToString(),
                                    CveArchivo = reader["cveArchivo"].ToString(),
                                    Archivo = reader["archivo"].ToString(),
                                    IdFicheroTemp = reader["idFicheroTemp"].ToString(),
                                    FicheroTemp = reader["ficheroTemp"].ToString(),
                                    IdFicheroDetalleTemp = reader["idFicheroDetalleTemp"].ToString(),
                                    CveEstatusFicheroDetalleTemp = reader["cveEstatusFicheroDetalleTemp"].ToString(),
                                    Alias = reader["alias"].ToString(),
                                    CveTipoAlias = reader["cveTipoAlias"].ToString()
                                };

                                lstRegHomologar.Add(regHomologar);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }
                return lstRegHomologar;

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
                return lstRegHomologar;
            }
        }

        public static void HomologarRegistro(string connConsulta, RegistroHomologar registro, int timeout, string etiquetaLogueo)
        {
            try
            {
                string sp = "PN_HML_HomologaRegistro";
                RespuestaGral respuestaGral = new RespuestaGral();

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;
                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        command.Parameters.Add(new SqlParameter("@idFicheroDetalleTemp", registro.IdFicheroDetalleTemp));

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

        public static void RestablecerRegistrosPorHomologar(string connConsulta, int timeout, string etiquetaLogueo)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_HML_RestableceRegistrosPorHomologar";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;
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
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        public static List<SolicitudReporte> ObtenerSolicitudesReportes(string connConsulta, int timeout, string etiquetaLogueo)
        {
            List<SolicitudReporte> lstSolicitudReporte = new List<SolicitudReporte>();
            SolicitudReporte regReporte;
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
                        command.CommandTimeout = timeout;
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
                                regReporte = new SolicitudReporte()
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

                                lstSolicitudReporte.Add(regReporte);
                            }
                        }

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }
                return lstSolicitudReporte;

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
                return lstSolicitudReporte;
            }
        }

        public static RespuestaGral InsertarReporte(string connConsulta, SolicitudReporte registro, ref string folioReporte, string etiquetaLogueo)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_RPT_InsertaSolicitudReporte";

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

                        SqlParameter param4 = new SqlParameter("@idGeneracionReporte", SqlDbType.BigInt);
                        param4.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param4);

                        command.Parameters.Add(new SqlParameter("@cveReporte", registro.ClaveReporte));
                        command.Parameters.Add(new SqlParameter("@cvePlugIn", registro.ClavePlugIn));
                        SqlParameter param5 = new SqlParameter("@fechaPresentacion", SqlDbType.Date);
                        param5.Value = registro.FechaPresentacion;
                        command.Parameters.Add(param5);
                        
                        SqlParameter param6 = new SqlParameter("@fechaGeneracion", SqlDbType.Date);
                        param6.Value = registro.FechaGeneracion;
                        command.Parameters.Add(param6);

                        command.ExecuteNonQuery();

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();
                        folioReporte = command.Parameters["@idGeneracionReporte"].Value.ToString();
                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }

            return respuestaGral;
        }

        public static RespuestaGral ActualizarEstatusReporte(string connConsulta, string mensaje, string folioReporte
                                        , string idSolicitudReporte, string etiquetaLogueo)
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

                        SqlParameter param2 = new SqlParameter("@respuesta", SqlDbType.VarChar, 3);
                        param2.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param2);

                        SqlParameter param3 = new SqlParameter("@descripcion", SqlDbType.VarChar, 5000);
                        param3.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param3);

                        command.Parameters.Add(new SqlParameter("@idSolicitudReporte", idSolicitudReporte));
                        command.Parameters.Add(new SqlParameter("@cveEstatus_Destino", mensaje));
                        command.Parameters.Add(new SqlParameter("@folio", folioReporte));

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

            return respuestaGral;
        }

        public static void RestablecerRegistrosReportes(string connConsulta, int timeout, string etiquetaLogueo)
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
                        command.CommandTimeout = timeout;
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
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }
        }

        public static string ObtenerTarjeta(string connConsulta, string cveAlias, string alias
                        , string etiquetaLogueo, ref string medioAcceso, ref string bin)
        {
            try
            {
                string sp = "PN_HML_ObtieneTarjeta";
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

                        SqlParameter param4 = new SqlParameter("@tarjeta", SqlDbType.VarChar, 50);
                        param4.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param4);

                        SqlParameter param5 = new SqlParameter("@bin", SqlDbType.VarChar, 16);
                        param5.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param5);

                        command.Parameters.Add(new SqlParameter("@tarjetaAlias", alias));
                        command.Parameters.Add(new SqlParameter("@cveTipoAlias", cveAlias));

                        command.ExecuteNonQuery();

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();
                        medioAcceso = command.Parameters["@tarjeta"].Value.ToString();
                        bin = command.Parameters["@bin"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);
                    }

                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }

            return medioAcceso;
        }

        public static bool SustituirAlias(string connConsulta, string idFicheroDetalleTemp, string tarjeta, string etiquetaLogueo
                                , string bin, ref string descRespuesta)
        {
            bool respuesta = false;

            try
            {
                string sp = "PN_HML_SustituyeAliasTarjeta";
                RespuestaGral respuestaGral = new RespuestaGral();

                string parte1 = tarjeta.Substring(0, 4);
                string parte2 = "****";
                string parte3 = $"**{tarjeta.Substring(10, 2)}";
                string parte4 = tarjeta.Substring(12, 4);
                string tarjetaEnmascarada = $"{parte1} {parte2} {parte3} {parte4}";

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

                        command.Parameters.Add(new SqlParameter("@idFicheroDetalleTemp", idFicheroDetalleTemp));

                        SqlParameter paramSSN = command.CreateParameter();
                        paramSSN.ParameterName = "@tarjeta";
                        paramSSN.DbType = DbType.AnsiStringFixedLength;
                        paramSSN.Direction = ParameterDirection.Input;
                        paramSSN.Value = tarjeta;
                        paramSSN.Size = tarjeta.Length;
                        command.Parameters.Add(paramSSN);

                        command.Parameters.Add(new SqlParameter("@tarjetaLongitud", tarjeta.Trim().Length));
                        command.Parameters.Add(new SqlParameter("@tarjetaEnmascarada", tarjetaEnmascarada));
                        command.Parameters.Add(new SqlParameter("@bin", bin));

                        command.ExecuteNonQuery();

                        respuestaGral.CodRespuesta = command.Parameters["@respuesta"].Value.ToString();
                        respuestaGral.DescRespuesta = command.Parameters["@descripcion"].Value.ToString();

                        LogueoSPS(respuestaGral, etiquetaLogueo);

                        return EvaluarRespuestaSPS(respuestaGral);
                    }

                }

            }
            catch (Exception err)
            {
                Log.Error(etiquetaLogueo + $" [{NombreMetodo()}:  ERROR: " + err.Message);
            }

            return respuesta;
        }

        public static void ActualizarEstatusFicheroDetalleTemp(string idFicheroDetalleTemp, string cveEstatus_Destino, string connConsulta, string etiquetaLogueo, string codigoRespuesta = null, string error = null)
        {
            RespuestaGral respuestaGral = new RespuestaGral();
            try
            {
                string sp = "PN_HML_ActualizaEstatusFicheroDetalleTemp";

                using (SqlConnection conn = new SqlConnection(connConsulta))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand(sp, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@idFicheroDetalleTemp", SqlDbType.BigInt);
                        param.Value = idFicheroDetalleTemp;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@cveEstatus_Destino", SqlDbType.VarChar, 10);
                        param.Value = cveEstatus_Destino;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@error", SqlDbType.VarChar, 5000);
                        param.Value = error;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@codigoRespuesta", SqlDbType.VarChar, 10);
                        param.Value = codigoRespuesta;
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

        private static void LogueoSPS(RespuestaGral respuestaGral, string etiquetaLogueo)
        {
            int valCodigo = Convert.ToInt32(respuestaGral.CodRespuesta);
            if (valCodigo >= 500 && valCodigo <= 599)
                Log.Error(etiquetaLogueo + "[" + respuestaGral.DescRespuesta + "]");
            else
                Log.Evento(etiquetaLogueo + "[" + respuestaGral.DescRespuesta + "]");

        }

        private static bool EvaluarRespuestaSPS(RespuestaGral respuestaGral)
        {
            try
            {
                int valCodigo = Convert.ToInt32(respuestaGral.CodRespuesta);
                if (valCodigo >= 200 && valCodigo <= 299)
                    return true;       //Regresa true ya que es una respuesta exitosa
                else
                    return false;       //Regresa false ya que es una respuesta de error
            }
            catch (Exception e)
            {
                return false;           //Regresa false por que hubo un error en la respuesta
            }

        }

        private static string NombreMetodo([CallerMemberName] string metodo = null)
        {
            return metodo;
        }
    }
}
