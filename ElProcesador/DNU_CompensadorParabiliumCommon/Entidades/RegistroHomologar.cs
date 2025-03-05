using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class RegistroHomologar
    {
        private string m_idTipoArchivo;
        private string m_cveTipoArchivo;
        private string m_tipoArchivo;
        private string m_idArchivo;
        private string m_cveArchivo;
        private string m_archivo;
        private string m_idFicheroTemp;
        private string m_ficheroTemp;
        private string m_idFicheroDetalleTemp;
        private string m_cveEstatusFicheroDetalleTemp;
        private string m_cveTipoAlias;
        private string m_alias;
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
        public string IdFicheroDetalleTemp { get => m_idFicheroDetalleTemp; set => m_idFicheroDetalleTemp = value; }
        public string CveEstatusFicheroDetalleTemp { get => m_cveEstatusFicheroDetalleTemp; set => m_cveEstatusFicheroDetalleTemp = value; }
        public string CveTipoAlias { get => m_cveTipoAlias; set => m_cveTipoAlias = value; }
        public string Alias { get => m_alias; set => m_alias = value; }
    }
}
