using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    /// <summary>
    /// contiene propiedades de los contratos del cliente
    /// </summary>
    /// <Update>Cruz Mejia Raul - 06/O4/2022</creation>
    /// 
    public class ValoresContratos
    {
        private string m_enmascararTarjeta;
        private string m_mostrarCI;
        private string m_markupPorcentaje;
        private string m_markupFijo;
        /// <summary>
        /// Propiedad que obtiene el valor para saber si se enmascara
        /// </summary>
        public string EnmascararTarjeta { get => m_enmascararTarjeta; set => m_enmascararTarjeta = value; }
        /// <summary>
        /// Propiedad que obtiene porcentaje de comision
        /// </summary>
        public string MostrarCI { get => m_mostrarCI; set => m_mostrarCI = value; }

        /// <summary>
        /// propiedad que obtiene el porcentaje de markup
        /// </summary>
        public string MarkupPorcentaje { get => m_markupPorcentaje; set => m_markupPorcentaje = value; }

        /// <summary>
        /// propiedad que obtiene el markupFjo
        /// </summary>
        public string MarkupFijo { get => m_markupFijo; set => m_markupFijo = value; }
        public ValoresContratos()
        {
            m_enmascararTarjeta = "0";
            m_mostrarCI = "0.0";
            MarkupFijo = "0";
            MarkupPorcentaje = "0";
        }
    }
}
