using RestSharp.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    /// <summary>
    /// contiene propiedades de los contratos del cliente
    /// </summary>
    /// <Update>Gustavo Fuentes Sánchez - 22/09/2023</creation>
    /// 
    public class ClaveContrato
    {
        public string idValorContrato { get; set; }
        public string cveValorContrato { get; set; }
        public string descripcionContrato { get; set; }
        public ValorContrato valorContrato { get; set; }

    }
    public class ValorContrato
    {
        public string NombreParametro { get; set; }
        public string ID_TipoColectiva { get; set; }
        public string TipoDato { get; set; }
        public string ClaveTipoColectiva { get; set; }
        public string Valor { get; set; }

    }
}
