using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasAnonimas.Entidades
{
    public class ArchivoConfiguracion
    {

        public Int64 ID_Archivo { get; set; }
        public String ClaveArchivo { get; set; }
        public String DescripcionArchivo { get; set; }
        public Int64 ID_ConsultaBD { get; set; }
        public Int32 ID_TipoProceso { get; set; }
        public String Nombre { get; set; }
        public String Prefijo { get; set; }
        public String Sufijo { get; set; }
        public String FormatoFecha { get; set; }
        public String PosicionFecha { get; set; }
        public String TipoArchivo { get; set; }

    }
}
