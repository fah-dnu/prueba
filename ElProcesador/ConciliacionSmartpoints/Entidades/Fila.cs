using ConciliacionSmartpoints.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.Entidades
{
    public class Fila
    {
        public Dictionary<Int32, String> losCampos = new Dictionary<Int32, String>();

        public TipoFilaConfig elTipoFila;

        public FilaConfig laConfigDeFila;

        public String DetalleCrudo;

    }
}
