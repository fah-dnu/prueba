using System;
using System.Collections.Generic;
using Interfases.Entidades;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    /// <summary>
    /// clase que contiene la entidad para la ejecucucion de la variante
    /// </summary>
    /// <creation>Cruz Mejia Raul - 07/10/2022</creation>
    public class ReglaVariante
    {
        private string m_nombre;
        private string m_storedProcedure;
        private int m_ordenEjecucion;
        private Boolean m_esAccion;
        private List<Parametro> m_parametros;


        /// <summary>
        /// Propiedad que obtiene el noombre de la variante
        /// </summary>
        public string Nombre { get => m_nombre; set => m_nombre = value; }

        /// <summary>
        /// Propiedad que obtiene el sp de la vaiante a ejecutar
        /// </summary>
        public string StoredProcedure { get => m_storedProcedure; set => m_storedProcedure = value; }

        /// <summary>
        /// Propiedad que obtiene el orden de ejecucion de l vari
        /// </summary>
        public int OrdenEjecucion { get => m_ordenEjecucion; set => m_ordenEjecucion = value; }

        /// <summary>
        /// 
        /// </summary>
        public bool EsAccion { get => m_esAccion; set => m_esAccion = value; }

        /// <summary>
        ///  propiedad que obtiene la lista de los parametros para la ejecucion de la variante
        /// </summary>
        public List<Parametro> Parametros { get => m_parametros; set => m_parametros = value; }


        public ReglaVariante()
        {
            Parametros = new List<Parametro>();
            m_nombre = string.Empty;
            m_storedProcedure = string.Empty;

        }
    }
}
