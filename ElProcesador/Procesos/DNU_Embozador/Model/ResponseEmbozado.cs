using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class ResponseEmbozado
    {
        private string m_IDSolicitud;
        private string m_Tarjeta;
        private string m_CriptoPIN;
        private string m_PVV;
        private string m_CVV;
        private string m_CVV2;
        private string m_CodRespuesta;
        private string m_DescRespuesta;

        public string IDSOLICITUD { get => m_IDSolicitud; set => m_IDSolicitud = value; }
        public string TARJETA { get => m_Tarjeta; set => m_Tarjeta = value; }
        public string CRIPTOPIN { get => m_CriptoPIN; set => m_CriptoPIN = value; }
        public string PVV { get => m_PVV; set => m_PVV = value; }
        public string CVV { get => m_CVV; set => m_CVV = value; }
        public string CVV2 { get => m_CVV2; set => m_CVV2 = value; }
        public string CODRESPUESTA { get => m_CodRespuesta; set => m_CodRespuesta = value; }
        public string DESCRESPUESTA { get => m_DescRespuesta; set => m_DescRespuesta = value; }
    }
}
