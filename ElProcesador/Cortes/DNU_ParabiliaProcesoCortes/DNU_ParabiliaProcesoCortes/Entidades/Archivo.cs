using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class Archivo
    {
        public string Nombre { get; set; }
        public string Ruta { get; set; }
        public string Extension { get; set; }
        public Int64 Registros { get; set; }
        public int Columnas { get; set; }
        public bool Procesado { get; set; }
        public string FechaProcesamiento { get; set; }
    }
}
