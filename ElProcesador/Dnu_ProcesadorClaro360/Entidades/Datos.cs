using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNU_ProcesadorClaro360.Entidades
{
   public class Datos
    {

        private List<Fila> losDatos = new List<Fila>();

        public List<Fila> LosDatos
        {
            get { return losDatos; }
            set { losDatos = value; }
        }
    }
}
