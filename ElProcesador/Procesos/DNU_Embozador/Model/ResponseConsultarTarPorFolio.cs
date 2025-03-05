using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class ResponseConsultarTarPorFolio
    {
        private string m_IDUsuario;
        private string m_CodRespuesta;
        private string m_DescRespuesta;

        public string IDUsuario { set => m_IDUsuario = value; get => m_IDUsuario; }
        public string CodRespuesta { set => m_CodRespuesta = value; get => m_CodRespuesta; }
        public string DescRespuesta { set => m_DescRespuesta = value; get => m_DescRespuesta; }
    }
}
