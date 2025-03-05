
// -----------------------------------------------------------------------
// <copyright file="ArchivoConfiguracion.cs" company="DNU">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DNU_ParabiliaTarjetasStock.Entidades
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
        
    /// <summary>
    /// Clase que contiene la entidad para leer la configuracion del archivo
    /// </summary>
    public class ArchivoConfiguracion
    {

        private Int64 m_ID_Archivo;
        private string m_claveArchivo;
        private string m_descripcionArchivo;
        private Int64 m_ID_ConsultaBD;
        private Int32 m_ID_TipoProceso;
        private string m_nombre;
        private string m_prefijo;
        private string m_sufijo;
        private string m_formatoFecha;
        private string m_posicionFecha;
        private string m_tipoArchivo;

        /// <summary>
        ///  propiedad que obtiene el id_archivo
        /// </summary>
        public long ID_Archivo { get => m_ID_Archivo; set => m_ID_Archivo = value; }
        /// <summary>
        /// propiedad que obtiene la clave archivo
        /// </summary>
        public string ClaveArchivo { get => m_claveArchivo; set => m_claveArchivo = value; }
        /// <summary>
        /// propiedad que obtiene la descripcion del archivo
        /// </summary>
        public string DescripcionArchivo { get => m_descripcionArchivo; set => m_descripcionArchivo = value; }
        /// <summary>
        /// propiedad que obtiene el id consulta bd
        /// </summary>
        public long ID_ConsultaBD { get => m_ID_ConsultaBD; set => m_ID_ConsultaBD = value; }
        /// <summary>
        /// propiedad que obtiene el id tipo proceso
        /// </summary>
        public int ID_TipoProceso { get => m_ID_TipoProceso; set => m_ID_TipoProceso = value; }
        /// <summary>
        /// propiedad que obtiene el nombre
        /// </summary>
        public string Nombre { get => m_nombre; set => m_nombre = value; }
        /// <summary>
        /// propiedad que obtiene el prefijo
        /// </summary>
        public string Prefijo { get => m_prefijo; set => m_prefijo = value; }
        /// <summary>
        /// propiedad que obtiene el sufijo
        /// </summary>
        public string Sufijo { get => m_sufijo; set => m_sufijo = value; }
        /// <summary>
        /// propiedad que obtiene el formato fecha
        /// </summary>
        public string FormatoFecha { get => m_formatoFecha; set => m_formatoFecha = value; }
        /// <summary>
        /// propiedad que obtiene posicion final fecha
        /// </summary>
        public string PosicionFecha { get => m_posicionFecha; set => m_posicionFecha = value; }
        /// <summary>
        /// propiedad que obtiene el tipo archivo
        /// </summary>
        public string TipoArchivo { get => m_tipoArchivo; set => m_tipoArchivo = value; }
    }

}
