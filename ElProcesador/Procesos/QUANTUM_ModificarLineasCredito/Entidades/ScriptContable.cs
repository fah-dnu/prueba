using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Executer.Entidades
{
    public class ScriptContable
    {
        public int ID_TipoColectiva { get; set; }
        public int ID_Divisa { get; set; }
        public int ID_TipoCuenta { get; set; }
        public String Formula { get; set; }
        public Boolean esAbono { get; set; }
        public Boolean GeneraDetalle { get; set; }
        public Boolean ValidaSaldo { get; set; }

    }
}
