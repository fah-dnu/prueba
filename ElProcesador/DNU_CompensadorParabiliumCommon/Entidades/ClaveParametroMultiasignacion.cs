using RestSharp.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    /// <summary>
    /// contiene propiedades de los parametros multiasignacion de los productos
    /// </summary>
    /// <Update>Gustavo Fuentes Sánchez - 22/09/2023</creation>
    /// 
    public class ClaveParametroMultiasignacion
    {
        public string idParametroMultiasignacion { get; set; }
        public string cveParametroMultiasignacion { get; set; }
        public string parametroMultiasignacion { get; set; }
        public ValorParametroMultiasignacion valorParametroMultiasignacion { get; set; }

    }
    public class ValorParametroMultiasignacion
    {
        public string NombreParametro { get; set; }
        public string ID_TipoColectiva { get; set; }
        public string TipoDato { get; set; }
        public string ClaveTipoColectiva { get; set; }
        public string Valor { get; set; }

    }
}
