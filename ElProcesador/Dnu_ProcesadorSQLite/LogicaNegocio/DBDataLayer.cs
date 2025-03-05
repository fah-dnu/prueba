using Dnu_ProcesadorSQLite.BaseDatos;
using Dnu_ProcesadorSQLite.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu_ProcesadorSQLite.LogicaNegocio
{
    public class DBDataLayer
    {
        private const string insertPromotions = "INSERT INTO Promociones (ID_Promocion," +
                 "ID_Cadena,Descripcion,EsDestacada,Restricciones,VigenciaInicio,VigenciaFin," +
                 "ClavePromocion,OrderBanner,OrdenPromoHome,TituloPromocion,TipoDescuento,ClaveTipoCupon,EsFavorito," +
                 "ServerOrder) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
        private const string insertBrands = "INSERT INTO Cadenas (ID_Cadena,ClaveCadena,RazonSocial," +
            "NombreComercial,ID_Giro,ClaveGiro,DescripcionGiro) VALUES(?,?,?,?,?,?,?)";
        private const string insertNetworks = "INSERT INTO Redes(ID_Cadena,ID_RedCadena,Clave,Descripcion,Valor)" +
            " VALUES(?,?,?,?,?)";
        private const string insertBranchOffices = "INSERT INTO Sucursales(ID_Cadena,id_sucursal,Clave,nombre," +
            "latitud,longitud,calle,colonia,ciudad,CveEstado,Estado,cp,telefono)" +
            " VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)";

        public void insertBrand(List<Brand> brands, string claveEstado , String path)
        {
            using (var ctx = SqliteDbContext.GetInstance(claveEstado, path))
            {
                foreach (Brand item in brands)
                {
                    using (var command = new SQLiteCommand(insertBrands, ctx))
                    {
                        command.Parameters.Add(new SQLiteParameter("ID_Cadena", item.brandId));
                        command.Parameters.Add(new SQLiteParameter("ClaveCadena", item.brandKey ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("RazonSocial", item.socialReason ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("NombreComercial", item.commercialName ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("ID_Giro", item.typeId ?? 0));
                        command.Parameters.Add(new SQLiteParameter("ClaveGiro", item.typeKey ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("DescripcionGiro", item.typeDescription ?? string.Empty));
                        command.ExecuteNonQuery();

                    }
                }
            }
        }


        public void insertPromotion(List<Promotion> promotions, string claveEstado, String path)
        {
            using (var ctx = SqliteDbContext.GetInstance(claveEstado, path))
            {
                foreach (Promotion item in promotions)
                {
                    using (var command = new SQLiteCommand(insertPromotions, ctx))
                    {
                        command.Parameters.Add(new SQLiteParameter("ID_Promocion", item.promotionId));
                        command.Parameters.Add(new SQLiteParameter("ID_Cadena", item.promotionBrandId ?? 0));
                        command.Parameters.Add(new SQLiteParameter("Descripcion", item.description ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("EsDestacada", item.isFeatured ?? 0));
                        command.Parameters.Add(new SQLiteParameter("Restricciones", item.restrictions ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("VigenciaInicio", item.validityStart ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("VigenciaFin", item.validityEnd ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("ClavePromocion", item.promotionKey ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("OrderBanner", item.bannerOrder ?? 0));
                        command.Parameters.Add(new SQLiteParameter("OrdenPromoHome", item.promoHomeOrder ?? 0));
                        command.Parameters.Add(new SQLiteParameter("TituloPromocion", item.title ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("TipoDescuento", item.discountType ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("ClaveTipoCupon", item.cuponTypeKey ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("EsFavorito", item.isFavorite ?? 0));
                        command.Parameters.Add(new SQLiteParameter("ServerOrder", item.serverOrder ?? 0));
                        command.ExecuteNonQuery();

                    }
                }
            }
        }


        public void insertNetwork(List<Network> networks, string claveEstado, String path)
        {
            using (var ctx = SqliteDbContext.GetInstance(claveEstado, path))
            {
                foreach (Network item in networks)
                {
                    using (var command = new SQLiteCommand(insertNetworks, ctx))
                    {
                        command.Parameters.Add(new SQLiteParameter("ID_Cadena", item.networkBrandId));
                        command.Parameters.Add(new SQLiteParameter("ID_RedCadena", item.networkId ?? 0));
                        command.Parameters.Add(new SQLiteParameter("Clave", item.key ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("Descripcion", item.description ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("Valor", item.value ?? string.Empty));
                        command.ExecuteNonQuery();

                    }
                }
            }
        }

        public void insertBranchOffice(List<Subsidiary> subsidiary, string claveEstado, String path)
        {
            using (var ctx = SqliteDbContext.GetInstance(claveEstado, path))
            {
                foreach (Subsidiary item in subsidiary)
                {
                    using (var command = new SQLiteCommand(insertBranchOffices, ctx))
                    {
                        command.Parameters.Add(new SQLiteParameter("ID_Cadena", item.subsidiaryBrandId));
                        command.Parameters.Add(new SQLiteParameter("id_sucursal", item.subsidiaryId ?? 0));
                        command.Parameters.Add(new SQLiteParameter("Clave", item.key ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("nombre", item.name ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("latitud", item.latitude ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("longitud", item.longitude ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("calle", item.street ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("colonia", item.neighborhood ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("ciudad", item.city ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("CveEstado", item.stateKey ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("Estado", item.state ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("cp", item.cp ?? string.Empty));
                        command.Parameters.Add(new SQLiteParameter("telefono", item.phone ?? string.Empty));

                        command.ExecuteNonQuery();

                    }
                }
            }
        }

    }
}
