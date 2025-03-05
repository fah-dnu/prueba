using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_VencimientosParabilia.Entidades
{
    public class ColectivaContrato
    {
        public String ID_Colevtiva { get; set; }
        public String ClaveColectiva { get; set; }
        public List<ValorContrato> lstValoresContrato { get; set; }
    }


    public class ValorContrato
    {
        public String Nombre { get; set; }
        public String Valor { get; set; }
    }
}
