using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_MovimientoManualMasivo.Entidades
{
    public class DMovimientoMasivo
    {
        private string m_Tarjeta;
        private string m_Importe;
        private string m_Referencia;
        private string m_Observaciones;        
        private string m_Concepto;
        private string m_Resultado;
        private string m_CodRespuesta;

        public string Tarjeta { set => m_Tarjeta = value; get => m_Tarjeta; }
        public string Importe { set => m_Importe = value; get => m_Importe; }
        public string Referencia { set => m_Referencia = value; get => m_Referencia; }
        public string Observaciones { set => m_Observaciones = value; get => m_Observaciones; }
        public string Concepto { set => m_Concepto = value; get => m_Concepto; }
        public string Resultado { set => m_Resultado = value; get => m_Resultado; }
        public string CodRespuesta { set => m_CodRespuesta = value; get => m_CodRespuesta; }
    }
}
