using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class Colectiva
    {
        private string m_idColectiva;
        private string m_cveColectiva;
        private string m_cveTipoColectiva;
        private string m_descColectiva;

        public string IdColectiva { get => m_idColectiva; set => m_idColectiva = value; }
        public string CveColectiva { get => m_cveColectiva; set => m_cveColectiva = value; }
        public string CveTipoColectiva { get => m_cveTipoColectiva; set => m_cveTipoColectiva = value; }
        public string DescColectiva { get => m_descColectiva; set => m_descColectiva = value; }
    }
}
