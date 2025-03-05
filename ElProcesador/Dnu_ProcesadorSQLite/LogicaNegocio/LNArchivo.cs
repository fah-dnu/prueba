using System;
using System.Collections.Generic;
using System.IO;
using CommonProcesador;
using Dnu_ProcesadorSQLite.BaseDatos;
using Dnu_ProcesadorSQLite.Entidades;

namespace Dnu_ProcesadorSQLite.LogicaNegocio
{
    public class LNArchivo
    {

        

        internal static bool ProcesaGeneracionSQLiteDB(string config, VersionDBSqlite newVersion, String path)
        {
            
            List<Estado> estados = DAODatosBase.ObtenerEstados(config);
            foreach (var estado in estados)
            {

                try
                {
                    List<Brand> brands = new List<Brand>();
                    List<Promotion> promotions = new List<Promotion>();
                    List<Network> networks = new List<Network>();
                    List<Subsidiary> subsidiary = new List<Subsidiary>();

                    List<PromocionesDBResult> result_ = DAODatosBase.ObtenerListaPromociones(config,estado,newVersion);
                    DBDataLayer dl = new DBDataLayer();

                    GetBrands(result_, brands, promotions, networks, subsidiary);
                    String pathFile = SqliteDbContext.Up(estado.ClaveEstado, path);
                    dl.insertBrand(brands, estado.ClaveEstado, path);
                    dl.insertPromotion(promotions, estado.ClaveEstado, path);
                    dl.insertNetwork(networks, estado.ClaveEstado, path);
                    dl.insertBranchOffice(subsidiary, estado.ClaveEstado, path);


                    var resultRows = DAODatosBase.insertVersionDB(config,estado.ClaveEstado, pathFile, newVersion);
                }
                catch (Exception err)
                {
                    Logueo.Error("ProcesaGeneracionSQLiteDB(): " + err.Message + " " + err.StackTrace);
                    return false;
                }

            }
            return true;

        }

        internal static int ActualizaEstatusVersionSqlite(string config, VersionDBSqlite newVersion)
        {
            return DAODatosBase.ActualizaEstatusVersionSqlite(config, newVersion);
        }

        internal static VersionDBSqlite ExisteVersionNueva(String config)
        {
            return  DAODatosBase.obtenerNuevaVersion(config);
        }

        private static void GetBrands(List<PromocionesDBResult> promociones,
            List<Brand> brands,
            List<Promotion> promotions,
            List<Network> networks,
            List<Subsidiary> subsidiary)
        {

            int i = 0;
            foreach (PromocionesDBResult item in promociones)
            {
                
                try
                {
                    var b = new Brand
                    {
                        brandId = item.ID_Cadena,
                        brandKey = item.ClaveCadena,
                        commercialName = item.NombreComercial,
                        socialReason = item.RazonSocial ?? string.Empty,
                        typeDescription = item.gDescripcion ?? string.Empty,
                        typeId = item.ID_Giro ?? 0,
                        typeKey = item.Clave
                    };
                    if (!brands.Exists(p => p.brandId == item.ID_Cadena && p.brandKey == item.ClaveCadena))
                        brands.Add(b);
                }
                catch (Exception ex)
                {

                }


                try
                {
                    var p = new Promotion
                    {
                        bannerOrder = item.OrdenBanner ?? 0,
                        cuponTypeKey = item.ClaveTipoCupon ?? string.Empty,
                        description = item.pDescripcion ?? string.Empty,
                        discountType = item.TipoDescuento ?? string.Empty,
                        isFavorite = item.EsFavorito.Value ? 1 : 0,
                        isFeatured = item.EsDestacada ?? 0,
                        orderPromotion = item.OrdenBanner ?? 0,
                        promoHomeOrder = item.OrdenPromoHome ?? 0,
                        promotionBrandId = item.ID_Cadena,
                        promotionId = item.ID_Promocion,
                        promotionKey = item.ClavePromocion ?? string.Empty,
                        restrictions = item.Restricciones ?? string.Empty,
                        title = item.TituloPromocion ?? string.Empty,
                        validityEnd = item.VigenciaFin.Value.ToString("yyyy-MM-dd"),
                        validityStart = item.VigenciaInicio.Value.ToString("yyyy-MM-dd"),
                        serverOrder = i++

                    };
                    if (!promotions.Exists(c => c.promotionBrandId == item.ID_Cadena && c.promotionId == item.ID_Promocion))
                        promotions.Add(p);
                }
                catch (Exception ex)
                {

                }


                try
                {
                    var n = new Network
                    {
                        description = item.rsDescricpion,
                        key = item.rsClave,
                        networkBrandId = item.ID_Cadena,
                        networkId = item.ID_RedCadena,
                        value = item.crValor
                    };
                    if (!networks.Exists(w => w.networkId == item.ID_RedCadena))
                    {
                        networks.Add(n);
                    }

                }
                catch (Exception ex)
                {

                }

                try
                {

                    var s = new Subsidiary
                    {
                        city = item.ciudad ?? string.Empty,
                        key = item.Clave ?? string.Empty,
                        latitude = item.latitud.ToString() ?? string.Empty,
                        longitude = item.longitud.ToString() ?? string.Empty,
                        name = item.nombre ?? string.Empty,
                        neighborhood = item.colonia ?? string.Empty,
                        phone = item.telefono ?? string.Empty,
                        cp = item.cp ?? string.Empty,
                        state = item.Estado ?? string.Empty,
                        stateKey = item.CveEstado ?? string.Empty,
                        street = item.calle ?? string.Empty,
                        subsidiaryBrandId = item.ID_Cadena,
                        subsidiaryId = item.id_sucursal
                    };

                    if (!subsidiary.Exists(d => d.subsidiaryId == item.id_sucursal && d.subsidiaryBrandId == item.ID_Cadena))
                    {
                        subsidiary.Add(s);
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
