using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitoreaOperaciones.Entidades
{
    public class DetalleOperaciones
    {
        public Int32 ID_CadenaComercial { get; set; }
        public Int32 Activas { get; set; }
        public Int32 Declinadas { get; set; }
        public String NombreCadenaComercial { get; set; }
        public String Codigos { get; set; }

    }
}
