using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class Evento
    {
        private string m_idEvento;
        private string m_cveEvento;
        private string m_descEvento;

        public string IdEvento { get => m_idEvento; set => m_idEvento = value; }
        public string CveEvento { get => m_cveEvento; set => m_cveEvento = value; }
        public string DescEvento { get => m_descEvento; set => m_descEvento = value; }
    }
}
