using Interfases.Entidades;
using Interfases.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class Variante
    {
        private string m_regla;
        private string m_idPertenenciaVariante;
        private string m_cvePertenenciaVariante;
        private string m_pertenenciaTipo;
        private string m_idPertenencia;
        private string m_idTipoColectiva;
        private string m_cveTipoColectiva;
        private string m_cveTipoVariante;
        private string m_tipoVariante;
        private string m_respuesta;
        private string m_descripcion;
        private string m_ejecucion;
        private bool m_varianteFinal;

        public string Regla { get => m_regla; set => m_regla = value; }
        
        public string IdPertenenciaVariante { get => m_idPertenenciaVariante; set => m_idPertenenciaVariante = value; }
        public string CvePertenenciaVariante { get => m_cvePertenenciaVariante; set => m_cvePertenenciaVariante = value; }
        public string PertenenciaTipo { get => m_pertenenciaTipo; set => m_pertenenciaTipo = value; }
        public string IdPertenencia { get => m_idPertenencia; set => m_idPertenencia = value; }
        public string IdTipoColectiva { get => m_idTipoColectiva; set => m_idTipoColectiva = value; }
        public string CveTipoColectiva { get => m_cveTipoColectiva; set => m_cveTipoColectiva = value; }
        public string CveTipoVariante { get => m_cveTipoVariante; set => m_cveTipoVariante = value; }
        public string TipoVariante { get => m_tipoVariante; set => m_tipoVariante = value; }
        public string respuesta { get => m_respuesta; set => m_respuesta = value; }
        public string descripcion { get => m_descripcion; set => m_descripcion = value; }
        public string Ejecucion { get => m_ejecucion; set => m_ejecucion = value; }
        public bool varianteFinal { get => m_varianteFinal; set => m_varianteFinal = value; }
        public List<ParametroV6> Parametros { get; set; }
    }

    public class ParametroV6 : Parametro
    {
        private string m_posicion;
        private string m_inputOuput;
        private TipoDatoSQL m_tipoDato;
        private string m_esAccion;
        private string m_valor;

        public string Posicion { get => m_posicion; set => m_posicion = value; }
        public string InputOuput { get => m_inputOuput; set => m_inputOuput = value; }
        //public TipoDatoSQL TipoDato { get => m_tipoDato; set => m_tipoDato = value; }
        public string EsAccion { get => m_esAccion; set => m_esAccion = value; }
        //public string Valor { get => m_valor; set => m_valor = value; }
    }

}
