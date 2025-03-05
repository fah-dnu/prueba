using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class EMBOZO_OrigenDatos
    {
        private int m_ID_EMBOZO_OrigenDatos { get; set; }
        private string m_ID_EMBOZO_Instancia { get; set; }
        private string m_q_text { get; set; }
        private string m_q_type { get; set; }
        private string m_q_connStringConfigurationKey { get; set; }
        private int m_q_timeOut { get; set; }
        private string m_ID_Colectiva { get; set; }
        private string m_GeneraNIP { get; set; }
        private string m_ObtieneDireccionEntregaAuto { get; set; }

        public int ID_EMBOZO_OrigenDatos { get => m_ID_EMBOZO_OrigenDatos; set => m_ID_EMBOZO_OrigenDatos = value; }
        public string ID_EMBOZO_Instancia { get => m_ID_EMBOZO_Instancia; set => m_ID_EMBOZO_Instancia = value; }
        public string q_text { get => m_q_text; set => m_q_text = value; }
        public string q_type { get => m_q_type; set => m_q_type = value; }
        public string q_connStringConfigurationKey { get => m_q_connStringConfigurationKey; set => m_q_connStringConfigurationKey = value; }
        public int q_timeOut { get => m_q_timeOut; set => m_q_timeOut = value; }
        public string ID_Colectiva { get => m_ID_Colectiva; set => m_ID_Colectiva = value; }
        public string GeneraNIP { get => m_GeneraNIP; set => m_GeneraNIP = value; }
        public string ObtieneDireccionEntregaAuto { get => m_ObtieneDireccionEntregaAuto; set => m_ObtieneDireccionEntregaAuto = value; }

    }
}
