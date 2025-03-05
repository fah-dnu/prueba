using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Executer.Entidades
{
    public class DetallePoliza
    {
        public int ID_Colectiva { get; set; }
        public int ID_Cuenta { get; set; }
        public int ID_TipoCuenta { get; set; }
        public int ID_Poliza { get; set; }
        public long ID_Polizadetalle { get; set; }
        public float Cargo { get; set; }
        public float Abono { get; set; }
        public ScriptContable Script { get; set; }

    }
}
