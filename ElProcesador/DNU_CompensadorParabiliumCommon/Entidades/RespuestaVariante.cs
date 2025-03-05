using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    /// <summary>
    /// clase que contiene las definiciones para la respuesta de la variante
    /// </summary>
    /// <creation>Cruz Mejia Raul - 07/10/2022</creation>
    public class RespuestaVariante
    {
        private int m_codigoRespuesta;
        private string m_descripcionRespuesta;


        /// <summary>
        ///   propiedad que obtiene el codigo de respuesta de la variante
        /// </summary>
        public int CodigoRespuesta { get => m_codigoRespuesta; set => m_codigoRespuesta = value; }
        /// <summary>
        ///  propiedad que obtiene la descripcion de respuesta de la variante
        /// </summary>
        public string DescripcionRespuesta { get => m_descripcionRespuesta; set => m_descripcionRespuesta = value; }
    }

    /// <summary>
    /// clase que contiene las definiciones para la respuesta de la variante del Compensador six
    /// </summary>
    /// <creation>Gustavo Fuentes Sánchez - 28/09/2023</creation>
    public class RespuestaVarianteV6 : RespuestaGral
    {
    //    private string m_respuesta;
        private string m_cveVariante;
        private bool m_respuestaSP;
        private bool m_cumpleVariante;


        /// <summary>
        ///   propiedad que obtiene el valor true si el codigo de respuesta de la variante esta entre 200 y 299
        /// </summary>
        public string cveVariante { get => m_cveVariante; set => m_cveVariante = value; }
        /// <summary>
        ///   propiedad que obtiene el valor true si el codigo de respuesta de la variante esta entre 200 y 299
        /// </summary>
        public bool respuestaSP { get => m_respuestaSP; set => m_respuestaSP = value; }
        /// <summary>
        ///   propiedad que obtiene el valor de la ejecución del SP variante
        /// </summary>
        public bool cumpleVariante { get => m_cumpleVariante; set => m_cumpleVariante = value; }
    }
}
