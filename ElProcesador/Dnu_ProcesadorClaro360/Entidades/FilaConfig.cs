using DNU_ProcesadorClaro360.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DNU_ProcesadorClaro360.Entidades
{
    public class FilaConfig
    {
         Dictionary<Int32, CampoConfig> _losCampos = new Dictionary<int, CampoConfig>();

         public Dictionary<Int32, CampoConfig> LosCampos { get { return _losCampos; } set {} }

        TipoFilaConfig elTipoFila;


        public Int64 ID_Fila { get; set; }
        public String ClaveFila { get; set; }
        public String DescripcionFila { get; set; }
        public Boolean PorSeparador { get; set; }
        public String CaracterSeparacion { get; set; }
        public Boolean PorLongitud { get; set; }
        public Int64 ID_CampoFila { get; set; }
        public Boolean IsPadding { get; set; }
       // public Int32 Longitud { get; set; }
        //public Int32 Posicion { get; set; }
    }
}
