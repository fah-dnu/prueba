using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class RequestEmbozado
    {
        private string m_IDSolicitud;
        private string m_Tarjeta;
        private string m_FecExpiracion;
        private string m_ForPinBlock;

        public string IDSOLICITUD { set => m_IDSolicitud = value; get => m_IDSolicitud; }
        public string TARJETA { set => m_Tarjeta = value; get => m_Tarjeta; }
        public string FECEXPIRACION { set => m_FecExpiracion = value; get => m_FecExpiracion; }
        public string FORPINBLOCK { set => m_ForPinBlock = value; get => m_ForPinBlock; }
    }
}
