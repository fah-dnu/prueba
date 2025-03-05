using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Limpieza.Entidades
{
    public class Archivo
    {
        public int IdUrl { get; set; }
        public string URlOrigen { get; set; }
        public int DiasArchivo { get; set; }
        public int DiasPapelera { get; set; }
        public string URLPapelera { get; set; }
    }
}
