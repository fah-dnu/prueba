using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class Catalogo
    {
        private string m_cveTipoColectiva;
        private string m_tipoColectiva;
        private string m_cveColectiva;
        private string m_colectiva;
        private string m_cveTipoProducto;
        private string m_tipoProducto;
        private string m_cveProducto;
        private string m_producto;
        private string m_cveDivisa;
        private string m_grupoCuenta;
        private string m_bin;
        private string m_fechaActivacion;
        private string m_activo;
        private string m_longitud;
        /// <summary>
        /// Propiedad que obtiene el valor para saber si se enmascara
        /// </summary>
        public string CveTipoColectiva { get => m_cveTipoColectiva; set => m_cveTipoColectiva = value; }
        public string TipoColectiva { get => m_tipoColectiva; set => m_tipoColectiva = value; }
        public string CveColectiva { get => m_cveColectiva; set => m_cveColectiva = value; }
        public string Colectiva { get => m_colectiva; set => m_colectiva = value; }
        public string CveTipoProducto { get => m_cveTipoProducto; set => m_cveTipoProducto = value; }
        public string TipoProducto { get => m_tipoProducto; set => m_tipoProducto = value; }
        public string CveProducto { get => m_cveProducto; set => m_cveProducto = value; }
        public string Producto { get => m_producto; set => m_producto = value; }
        public string CveDivisa { get => m_cveDivisa; set => m_cveDivisa = value; }
        public string GrupoCuenta { get => m_grupoCuenta; set => m_grupoCuenta = value; }
        public string Bin { get => m_bin; set => m_bin = value; }
        public string FechaActivacion { get => m_fechaActivacion; set => m_fechaActivacion = value; }
        public string Activo { get => m_activo; set => m_activo = value; }
        public string Longitud { get => m_longitud; set => m_longitud = value; }

    }
}
