using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class RespuestaGral
    {
        private string m_CodRespuesta;
        private string m_DescRespuesta;

        public string CodRespuesta { get => m_CodRespuesta; set => m_CodRespuesta = value; }
        public string DescRespuesta { get => m_DescRespuesta; set => m_DescRespuesta = value; }
    }
}
