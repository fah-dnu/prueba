using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class RegistroConsolidar
    {
        private string m_idTipoArchivo;
        private string m_cveTipoArchivo;
        private string m_tipoArchivo;
        private string m_idArchivo;
        private string m_cveArchivo;
        private string m_archivo;
        private string m_idFicheroTemp;
        private string m_ficheroTemp;
        private string m_idFicheroProvisional;
        private string m_cveEstatusFicheroProvisional;
        private string m_spConsolidacion;
        /// <summary>
        /// Propiedad que obtiene el valor para saber si se enmascara
        /// </summary>
        public string IdTipoArchivo { get => m_idTipoArchivo; set => m_idTipoArchivo = value; }
        public string CveTipoArchivo { get => m_cveTipoArchivo; set => m_cveTipoArchivo = value; }
        public string TipoArchivo { get => m_tipoArchivo; set => m_tipoArchivo = value; }
        public string IdArchivo { get => m_idArchivo; set => m_idArchivo = value; }
        public string CveArchivo { get => m_cveArchivo; set => m_cveArchivo = value; }
        public string Archivo { get => m_archivo; set => m_archivo = value; }
        public string IdFicheroTemp { get => m_idFicheroTemp; set => m_idFicheroTemp = value; }
        public string FicheroTemp { get => m_ficheroTemp; set => m_ficheroTemp = value; }
        public string IdFicheroProvisional { get => m_idFicheroProvisional; set => m_idFicheroProvisional = value; }
        public string CveEstatusFicheroProvisional { get => m_cveEstatusFicheroProvisional; set => m_cveEstatusFicheroProvisional = value; }
        public string SpConsolidacion { get => m_spConsolidacion; set => m_spConsolidacion = value; }
    }
}
