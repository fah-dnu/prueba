using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.Entidades
{
    public class Ficheros
    {
        public int IdFichero { get; set; }
        public DateTime FechaProceso { get; set; }
        public string NombreFichero { get; set; }
        public int IdConsulta { get; set; }
        public int IdArchivo { get; set; }
        public int ID_EstatusFichero { get; set; }     
    }
}
