using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Common.Entidades
{
    public class FacturaTipo
    {
        public Int64 ID_FacturaTipo { get; set; }
        public String Descripcion { get; set; }
        public Int64 ID_ColectivaEmisora { get; set; }
        public Int64 ID_ColectiraReceptora { get; set; }
        public Int32 ID_TipoColectivaNivelDatos { get; set; }
        public Int32 ID_TipoColectivaReceptor { get; set; }
        public Int32 ID_TipoColectivaRE { get; set; }
        public Int32 ID_TipoColectivaEM { get; set; }
        public Int32 ID_Periodo { get; set; }
        public Int32 ID_TipoDocumento { get; set; }
        public Colectiva Emisor { get; set; }
        public Colectiva Receptor { get; set; }
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }
        public String FechaEmision { get; set; }
        public Boolean Foliada { get; set; }

        private Dictionary<String, Parametro> _parametrosCalculados = new Dictionary<string, Parametro>();


        public String ClaveTipoDocumento { get; set; }
        public String DescripcionTipoDocumento { get; set; }
        public Boolean Timbra { get; set; }
        public Boolean GeneraXML { get; set; }
        public Boolean CalculaIVA { get; set; }

        public Dictionary<String, Parametro> ParametrosCalculados
        {
            get { return _parametrosCalculados; }
            set { _parametrosCalculados = value; }
        }
        private List<DetalleFacturaTipo> _losDetalles = new List<DetalleFacturaTipo>();

        public List<DetalleFacturaTipo> losDetalles
        {
            get { return _losDetalles; }
            set { _losDetalles = value; }
        }
    }
}
