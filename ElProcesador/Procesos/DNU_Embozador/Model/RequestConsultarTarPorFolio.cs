using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class RequestConsultarTarPorFolio
    {
        private string m_ClaveEmpresa;
        private string m_Folio;

        public string ClaveEmpresa { set => m_ClaveEmpresa = value; get => m_ClaveEmpresa; }
        public string Folio { set => m_Folio = value; get => m_Folio; }
    }
}
