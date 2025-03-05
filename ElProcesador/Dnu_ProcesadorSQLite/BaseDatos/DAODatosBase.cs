using CommonProcesador;
using Dnu_ProcesadorSQLite.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;


namespace Dnu_ProcesadorSQLite.BaseDatos
{
    public class DAODatosBase
    {
        internal static List<PromocionesDBResult> ObtenerListaPromociones(string connectionString, Estado estado, VersionDBSqlite newVersion)
        {

            List<PromocionesDBResult> promociones = new List<PromocionesDBResult>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_getPromociones";
                    cmd.Parameters.Add(new SqlParameter("@ClaveCadena", String.Empty));
                    cmd.Parameters.Add(new SqlParameter("@NombreComercial", String.Empty));
                    cmd.Parameters.Add(new SqlParameter("@Id_Giro", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@Estado", estado.ClaveEstado));
                    cmd.Parameters.Add(new SqlParameter("@EsDestacada", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@Longitud", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@Latitud", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@Distancia", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@ClavePromocion", String.Empty));
                    cmd.Parameters.Add(new SqlParameter("@Ciudad", String.Empty));
                    cmd.Parameters.Add(new SqlParameter("@Email", String.Empty));
                    cmd.Parameters.Add(new SqlParameter("@Programa", newVersion.Programa));
                    cmd.Parameters.Add(new SqlParameter("@Id_Cliente", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@EsFavorito", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@EsBanner", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@EsFrecuente", DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@NumeroComercio", DBNull.Value));

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    //DataTable dt = new DataTable();
                    //dt.Load(reader);
                    int i = 0;
                    while (reader.Read())
                    {
                        i++;
                        //PromocionesDBResult promocionesRes = new PromocionesDBResult();
                        promociones.Add(new PromocionesDBResult
                        {
                            calle = reader["calle"].ToString(),
                            ciudad = reader["ciudad"].ToString(),
                            Clave = reader["Clave"].ToString(),
                            ClaveCadena = reader["ClaveCadena"].ToString(),
                            ClavePromocion = reader["ClavePromocion"].ToString(),
                            ClaveTipoCupon = reader["ClaveTipoCupon"].ToString(),
                            colonia = reader["colonia"].ToString(),
                            cp = reader["cp"].ToString(),
                            crValor = reader["crValor"].ToString(),
                            CveEstado = reader["CveEstado"].ToString(),
                            EsDestacada = (int?)reader["EsDestacada"],
                            EsFavorito = Convert.ToBoolean(reader["EsFavorito"]),
                            Estado = reader["Estado"].ToString(),
                            gDescripcion = reader["gDescripcion"].ToString(),
                            ID_Cadena = (int?)reader["ID_Cadena"],
                            ID_Giro = (int?)reader["ID_Giro"],
                            ID_Promocion = (int?)reader["ID_Promocion"],
                            ID_RedCadena = (int?)reader["ID_RedCadena"],
                            id_sucursal = (int?)reader["id_sucursal"],
                            latitud = !String.IsNullOrEmpty(reader["latitud"].ToString()) ? (double?)reader["latitud"] : null,
                            longitud = !String.IsNullOrEmpty(reader["longitud"].ToString()) ? (double?)reader["longitud"] : null,
                            nombre = reader["nombre"].ToString(),
                            NombreComercial = reader["NombreComercial"].ToString(),
                            OrdenBanner = (int?)reader["OrdenBanner"],
                            OrdenPromoHome = (int?)reader["OrdenPromoHome"],
                            pDescripcion = reader["pDescripcion"].ToString(),
                            RazonSocial = reader["RazonSocial"].ToString(),
                            Restricciones = reader["Restricciones"].ToString(),
                            rsClave = reader["rsClave"].ToString(),
                            rsDescricpion = reader["rsDescricpion"].ToString(),
                            sClave = reader["sClave"].ToString(),
                            telefono = reader["telefono"].ToString(),
                            TipoDescuento = reader["TipoDescuento"].ToString(),
                            TituloPromocion = reader["TituloPromocion"].ToString(),
                            VigenciaFin = DateTime.Parse(reader["VigenciaFin"].ToString()),
                            VigenciaInicio = DateTime.Parse(reader["VigenciaInicio"].ToString()),
                        });
                    //promocionesRes.calle = reader["calle"].ToString();
                    //promocionesRes.ciudad = reader["ciudad"].ToString();
                    //promocionesRes.Clave = reader["Clave"].ToString();
                    //promocionesRes.ClaveCadena = reader["ClaveCadena"].ToString();
                    //promocionesRes.ClavePromocion = reader["ClavePromocion"].ToString();
                    //promocionesRes.ClaveTipoCupon = reader["ClaveTipoCupon"].ToString();
                    //promocionesRes.colonia = reader["colonia"].ToString();
                    //promocionesRes.cp = reader["cp"].ToString();
                    //promocionesRes.crValor = reader["crValor"].ToString();
                    //promocionesRes.CveEstado = reader["CveEstado"].ToString();
                    //promocionesRes.EsDestacada = (int?)reader["EsDestacada"];
                    //promocionesRes.EsFavorito = Convert.ToBoolean(reader["EsFavorito"]);
                    //promocionesRes.Estado = reader["Estado"].ToString();
                    //promocionesRes.gDescripcion = reader["gDescripcion"].ToString();
                    //promocionesRes.ID_Cadena = (int?)reader["ID_Cadena"];
                    //promocionesRes.ID_Giro = (int?)reader["ID_Giro"];
                    //promocionesRes.ID_Promocion = (int?)reader["ID_Promocion"];
                    //promocionesRes.ID_RedCadena = (int?)reader["ID_RedCadena"];
                    //promocionesRes.id_sucursal = (int?)reader["id_sucursal"];
                    //promocionesRes.latitud = !String.IsNullOrEmpty( reader["latitud"].ToString()) ? (double?)reader["latitud"] : null;
                    //promocionesRes.longitud = !String.IsNullOrEmpty(reader["longitud"].ToString()) ? (double?)reader["longitud"] : null;
                    //promocionesRes.nombre = reader["nombre"].ToString();
                    //promocionesRes.NombreComercial = reader["NombreComercial"].ToString();
                    //promocionesRes.OrdenBanner = (int?)reader["OrdenBanner"];
                    //promocionesRes.OrdenPromoHome = (int?)reader["OrdenPromoHome"];
                    //promocionesRes.pDescripcion = reader["pDescripcion"].ToString();
                    //promocionesRes.RazonSocial = reader["RazonSocial"].ToString();
                    //promocionesRes.Restricciones = reader["Restricciones"].ToString();
                    //promocionesRes.rsClave = reader["rsClave"].ToString();
                    //promocionesRes.rsDescricpion = reader["rsDescricpion"].ToString();
                    //promocionesRes.sClave = reader["sClave"].ToString();
                    //promocionesRes.telefono = reader["telefono"].ToString();
                    //promocionesRes.TipoDescuento = reader["TipoDescuento"].ToString();
                    //promocionesRes.TituloPromocion = reader["TituloPromocion"].ToString();
                    //promocionesRes.VigenciaFin = DateTime.Parse(reader["VigenciaFin"].ToString());
                    //promocionesRes.VigenciaInicio = DateTime.Parse(reader["VigenciaInicio"].ToString());
                    //promociones.Add(promocionesRes);
                }

                    //return GetPromociones(promociones);
                    return promociones;

                }
            }
        
        }

        internal static int ActualizaEstatusVersionSqlite(string config, VersionDBSqlite newVersion)
        {
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = "updVersionDBSqlite";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@IdVersion", newVersion.idVersion));
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        internal static int insertVersionDB(string connectionString, string claveEstado, string pathFile, VersionDBSqlite newVersion)
        {
            byte[] file;
            using (var stream = new FileStream(pathFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(stream))
                {
                    file = reader.ReadBytes((int)stream.Length);
                }
            }

            File.Delete(pathFile);
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    
                    cmd.Connection = con;
                    cmd.CommandText = "insVersionDBSqliteData";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@IdVersion", newVersion.idVersion));
                    cmd.Parameters.Add(new SqlParameter("@CveEstado", claveEstado));
                    cmd.Parameters.Add(new SqlParameter("@Size", file.Length));
                    //var param = new SqlParameter().Value = file;
                    cmd.Parameters.Add("@SQLiteDB", SqlDbType.VarBinary, file.Length).Value =  file;
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        internal static VersionDBSqlite obtenerNuevaVersion(string config)
        {
            VersionDBSqlite newVersion = new VersionDBSqlite();
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "getNewVersionDBSqlite";
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        newVersion.Version = reader["Version"].ToString();
                        newVersion.Programa = reader["Programa"].ToString();
                        newVersion.idVersion = Convert.ToInt32(reader["idVersion"]);
                    }
                }
            }
            return newVersion;
        }

        internal static List<Estado> ObtenerEstados(string config)
        {
            List<Estado> estados = new List<Estado>();
            using (SqlConnection con = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_getEstados";
                    con.Open();
                    var reader =  cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var item = reader;
                        estados.Add(new Estado
                        {
                            ClaveEstado =  reader["CveEstado"].ToString(),
                            DescripcionEstado = reader["Estado"].ToString()
                        });
                    }
                }
            }

            return estados;
        }


        //internal static PromocionesResult GetPromociones(List<PromocionesDBResult> data)
        //{
        //    PromocionesResult respuesta_ = new PromocionesResult();
        //    respuesta_.CodigoRespuesta = "0";
        //    respuesta_.Descripcion = "AUTORIZADA";

        //    respuesta_ = (PromocionesResult) data.GroupBy(c => new
        //    {
        //        c.ID_Cadena,
        //        c.ClaveCadena,
        //        c.RazonSocial,
        //        c.NombreComercial,

        //    }).Select(g => new
        //    {
        //        ID_Cadena = g.Key.ID_Cadena,
        //        ClaveCadena = g.Key.ClaveCadena,
        //        RazonSocial = g.Key.RazonSocial,
        //        NombreComercial = g.Key.NombreComercial,

        //        Giro = g.GroupBy(p => new
        //        {
        //            p.ID_Giro,
        //            p.Clave,
        //            p.gDescripcion
        //        }).Select(h => new
        //        {
        //            ID_Giro = h.Key.ID_Giro,
        //            Clave = h.Key.Clave,
        //            Descripcion = h.Key.gDescripcion
        //        }),

        //        Redes = g.GroupBy(i => new
        //        {
        //            i.ID_RedCadena,
        //            i.rsClave,
        //            i.rsDescricpion,
        //            i.crValor,
        //        }).Select(j => new
        //        {
        //            ID_RedCadena = j.Key.ID_RedCadena,
        //            Clave = j.Key.rsClave,
        //            Descricpion = j.Key.rsDescricpion,
        //            Valor = j.Key.crValor
        //        }),

        //        Sucursales = g.GroupBy(k => new
        //        {
        //            k.id_sucursal,
        //            k.sClave,
        //            k.nombre,
        //            k.latitud,
        //            k.longitud,
        //            k.calle,
        //            k.colonia,
        //            k.ciudad,
        //            k.CveEstado,
        //            k.Estado,
        //            k.cp,
        //            k.telefono,
        //        }).Select(l => new
        //        {
        //            id_sucursal = l.Key.id_sucursal,
        //            Clave = l.Key.sClave,
        //            nombre = l.Key.nombre,
        //            latitud = l.Key.latitud,
        //            longitud = l.Key.longitud,
        //            calle = l.Key.calle,
        //            colonia = l.Key.colonia,
        //            ciudad = l.Key.ciudad,
        //            CveEstado = l.Key.CveEstado,
        //            Estado = l.Key.Estado,
        //            cp = l.Key.cp,
        //            telefono = l.Key.telefono
        //        }),

        //        Promociones = g.GroupBy(m => new
        //        {
        //            m.ID_Promocion,
        //            m.pDescripcion,
        //            m.EsDestacada,
        //            m.Restricciones,
        //            m.VigenciaInicio,
        //            m.VigenciaFin,
        //            m.ClavePromocion,
        //            m.OrdenBanner,
        //            m.OrdenPromoHome,
        //            m.TituloPromocion,
        //            m.TipoDescuento,
        //            m.ClaveTipoCupon,
        //            m.EsFavorito
        //        }).Select(n => new
        //        {
        //            ID_Promocion = n.Key.ID_Promocion,
        //            Descripcion = n.Key.pDescripcion,
        //            EsDestacada = n.Key.EsDestacada,
        //            Restricciones = n.Key.Restricciones,
        //            VigenciaInicio = n.Key.VigenciaInicio,
        //            VigenciaFin = n.Key.VigenciaFin,
        //            ClavePromocion = n.Key.ClavePromocion,
        //            OrderBanner = n.Key.OrdenBanner,
        //            OrdenPromoHome = n.Key.OrdenPromoHome,
        //            TituloPromocion = n.Key.TituloPromocion,
        //            TipoDescuento = n.Key.TipoDescuento,
        //            ClaveTipoCupon = n.Key.ClaveTipoCupon,
        //            EsFavorito = n.Key.EsFavorito
        //        })


        //    }
        //    );

        //    return respuesta_;
        //}

    }
}
