using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAsignarLimiteCredito.Entidades
{
    public class DLimiteCred
    {
        private string m_Tarjeta;
        private string m_LimiteCredito;
        private string m_Resultado;
        private string m_EstatusTarjeta;
        private string m_CuentaOrigen;
        private string m_Concepto;
        private string m_CodRespuesta;
        private string m_Usuario;

        public string Tarjeta { set => m_Tarjeta = value; get => m_Tarjeta; }
        public string LimiteCredito { set => m_LimiteCredito = value; get => m_LimiteCredito; }
        public string Resultado { set => m_Resultado = value; get => m_Resultado; }
        public string EstatusTarjeta { set => m_EstatusTarjeta = value; get => m_EstatusTarjeta; }
        public string CuentaOrigen { set => m_CuentaOrigen = value; get => m_CuentaOrigen; }
        public string Concepto { set => m_Concepto = value; get => m_Concepto; }
        public string CodRespuesta { set => m_CodRespuesta = value; get => m_CodRespuesta; }
        public string Usuario { set => m_Usuario = value; get => m_Usuario; }
    }
}
