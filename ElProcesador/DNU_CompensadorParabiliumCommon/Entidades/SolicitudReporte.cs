using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class SolicitudReporte
    {
        private string m_ID_SolicitudReporte;
        private string m_ID_Reporte;
        private string m_ClaveReporte;
        private string m_ID_EstatusSolicitudReporte;
        private string m_ClaveEstatusSolicitudReporte;
        private string m_ClavePlugIn;
        private string m_FechaPresentacion;
        private string m_FechaGeneracion;
        /// <summary>
        /// Propiedad que obtiene el valor para saber si se enmascara
        /// </summary>
        public string ID_SolicitudReporte { get => m_ID_SolicitudReporte; set => m_ID_SolicitudReporte = value; }
        public string ID_Reporte { get => m_ID_Reporte; set => m_ID_Reporte = value; }
        public string ClaveReporte { get => m_ClaveReporte; set => m_ClaveReporte = value; }
        public string ID_EstatusSolicitudReporte { get => m_ID_EstatusSolicitudReporte; set => m_ID_EstatusSolicitudReporte = value; }
        public string ClaveEstatusSolicitudReporte { get => m_ClaveEstatusSolicitudReporte; set => m_ClaveEstatusSolicitudReporte = value; }
        public string ClavePlugIn { get => m_ClavePlugIn; set => m_ClavePlugIn = value; }
        public string FechaPresentacion { get => m_FechaPresentacion; set => m_FechaPresentacion = value; }
        public string FechaGeneracion { get => m_FechaGeneracion; set => m_FechaGeneracion = value; }
    }
}
