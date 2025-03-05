using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DNU_ProcesadorArchivos.Entidades
{
    public class DTOConfiguracionArchivo
    {
        public static readonly string SEPARADOR = "|";
        public static readonly string TAG_ENCABEZADO = "HDR";
        public static readonly string TAG_REGISTRO = "REG";
        public static readonly string TAG_PIE = "TRL";

        public static readonly Regex REGEX_ENTRADA = new Regex("^SOL_MEDA_[0-9a-zA-Z]+_[0-9]{8}.(TXT|txt)$");
        public static readonly string TAG_ENTRADA = "SOL_MEDA";
        public static readonly string TAG_SALIDA = "RES_MEDA";

        public static readonly string BD_LECTURA;
        public static readonly string BD_ESCRITURA;

        static DTOConfiguracionArchivo()
        {
            BD_LECTURA = System.Configuration.ConfigurationManager.ConnectionStrings["BDAutorizadorRead"].ConnectionString;
            BD_ESCRITURA = System.Configuration.ConfigurationManager.ConnectionStrings["BDAutorizadorWritte"].ConnectionString;
        }

        #region Encabezado

        //public static readonly DTOConfiguracionCampo ENC_IndicadorFila = new DTOConfiguracionCampo
        //{
        //    Tipo = typeof(string),
        //    Longitud = 3
        //};

        public static readonly DTOConfiguracionCampo ENC_Fecha = new DTOConfiguracionCampo
        {
            Tipo = typeof(DateTime),
            Longitud = 8,
            Formato = "yyyyMMdd"
        };

        #endregion
        #region Detalle

        //public static readonly DTOConfiguracionCampo DET_IndicadorFila = new DTOConfiguracionCampo
        //{
        //    Tipo = typeof(string),
        //    Longitud = 3
        //};
        public static readonly DTOConfiguracionCampo DET_Registro = new DTOConfiguracionCampo
        {
            Tipo = typeof(string),
            Longitud = 520
        };

       

        #region Respuesta

        public static readonly DTOConfiguracionCampo RESP_CodigoRespuesta = new DTOConfiguracionCampo
        {
            Tipo = typeof(string),
            Longitud = 4
        };

        public static readonly DTOConfiguracionCampo RESP_Autorizacion = new DTOConfiguracionCampo
        {
            Tipo = typeof(int),
            Longitud = 6
        };

        #endregion

        #endregion
        #region Pie

        //public static readonly DTOConfiguracionCampo PIE_IndicadorFila = new DTOConfiguracionCampo
        //{
        //    Tipo = typeof(string),
        //    Longitud = 3
        //};

        public static readonly DTOConfiguracionCampo PIE_NumeroRegistros = new DTOConfiguracionCampo
        {
            Tipo = typeof(int),
            Longitud = 10
        };

        public static readonly DTOConfiguracionCampo PIE_ImporteTotal = new DTOConfiguracionCampo
        {
            Tipo = typeof(decimal),
            Longitud = 12
        };

        #endregion

        public class DTOConfiguracionCampo
        {
            public Type Tipo { get; set; }
            public int Longitud { get; set; }
            public string Formato { get; set; }
        }
    }
}
