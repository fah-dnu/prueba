using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu_ProcesadorSQLite.Entidades
{

    public class PromocionesDBResult
    {
        public Nullable<int> ID_Cadena { get; set; }
        public string ClaveCadena { get; set; }
        public string RazonSocial { get; set; }
        public string NombreComercial { get; set; }
        public Nullable<int> ID_Giro { get; set; }
        public string Clave { get; set; }
        public string gDescripcion { get; set; }
        public Nullable<int> ID_RedCadena { get; set; }
        public string rsClave { get; set; }
        public string rsDescricpion { get; set; }
        public string crValor { get; set; }
        public Nullable<int> id_sucursal { get; set; }
        public string sClave { get; set; }
        public string nombre { get; set; }
        public Nullable<double> latitud { get; set; }
        public Nullable<double> longitud { get; set; }
        public string calle { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string CveEstado { get; set; }
        public string Estado { get; set; }
        public string cp { get; set; }
        public string telefono { get; set; }
        public Nullable<int> ID_Promocion { get; set; }
        public string pDescripcion { get; set; }
        public Nullable<int> EsDestacada { get; set; }
        public string Restricciones { get; set; }
        public Nullable<System.DateTime> VigenciaInicio { get; set; }
        public Nullable<System.DateTime> VigenciaFin { get; set; }
        public string ClavePromocion { get; set; }
        public Nullable<int> OrdenBanner { get; set; }
        public Nullable<int> OrdenPromoHome { get; set; }
        public string TituloPromocion { get; set; }
        public string TipoDescuento { get; set; }
        public string ClaveTipoCupon { get; set; }
        public Nullable<bool> EsFavorito { get; set; }
    }

    public class PromocionesResult
    {
        public string CodigoRespuesta { get; set; }
        public string Descripcion { get; set; }
        public Cadena[] Cadenas { get; set; }
    }

    public class Cadena
    {
        public int ID_Cadena { get; set; }
        public string ClaveCadena { get; set; }
        public object RazonSocial { get; set; }
        public string NombreComercial { get; set; }
        public Giro[] Giro { get; set; }
        public Rede[] Redes { get; set; }
        public Sucursale[] Sucursales { get; set; }
        public Promocione[] Promociones { get; set; }
    }

    public class Giro
    {
        public int ID_Giro { get; set; }
        public string Clave { get; set; }
        public string Descripcion { get; set; }
    }

    public class Rede
    {
        public int ID_RedCadena { get; set; }
        public string Clave { get; set; }
        public string Descricpion { get; set; }
        public string Valor { get; set; }
    }

    public class Sucursale
    {
        public int id_sucursal { get; set; }
        public string Clave { get; set; }
        public string nombre { get; set; }
        public float latitud { get; set; }
        public float longitud { get; set; }
        public string calle { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string CveEstado { get; set; }
        public string Estado { get; set; }
        public string cp { get; set; }
        public string telefono { get; set; }
    }

    public class Promocione
    {
        public int ID_Promocion { get; set; }
        public string Descripcion { get; set; }
        public int EsDestacada { get; set; }
        public string Restricciones { get; set; }
        public DateTime VigenciaInicio { get; set; }
        public DateTime VigenciaFin { get; set; }
        public string ClavePromocion { get; set; }
        public int OrderBanner { get; set; }
        public int OrdenPromoHome { get; set; }
        public string TituloPromocion { get; set; }
        public string TipoDescuento { get; set; }
        public string ClaveTipoCupon { get; set; }
        public bool EsFavorito { get; set; }
    }

    
}
