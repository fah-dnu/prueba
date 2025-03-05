using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class Producto
    {
        private string m_idProducto;
        private string m_cveProducto;
        private string m_cveColectiva;
        private string m_cveDivisa;
        private string m_descProducto;
        private string m_grupoCuenta;

        public string IdProducto { get => m_idProducto; set => m_idProducto = value; }
        public string CveProducto { get => m_cveProducto; set => m_cveProducto = value; }
        public string CveColectiva { get => m_cveColectiva; set => m_cveColectiva = value; }
        public string CveDivisa { get => m_cveDivisa; set => m_cveDivisa = value; }
        public string DescProducto { get => m_descProducto; set => m_descProducto = value; }
        public string GrupoCuenta { get => m_grupoCuenta; set => m_grupoCuenta = value; }
    }
}
