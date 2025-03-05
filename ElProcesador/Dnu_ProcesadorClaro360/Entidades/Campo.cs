using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DNU_ProcesadorClaro360.Entidades
{
   public class Campo
    {

        public Boolean IsPadding {get; set;}
        public Int32 Longitud { get; set; }
        public Int32 Posicion { get; set; }
        public String Nombre { get; set; }
        public String Descripcion { get; set; }
        public Boolean EsClave { get; set; }
        public Int32 ID_TipoColectiva { get; set; }
        public String TrimCaracteres { get; set; }

    }
}
