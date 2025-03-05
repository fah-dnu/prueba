using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class EMBOZO_Parametros
    {
        public int ID_EMBOZO_Parametros { get; set; }
        public int ID_EMBOZO_Instancia { get; set; }
        public int ID_EMBOZO_ParameterType { get; set; }
        public string form { get; set; }
        public int longitud { get; set; }
        public int initial_index { get; set; }
        public int final_index { get; set; }
        public string f_key { get; set; }
        public string content { get; set; }
        public string formato { get; set; }
        public ORIGIN origin { get; set; }

    }
}
