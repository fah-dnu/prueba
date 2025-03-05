using DNU_ProcesadorArchivos.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DNU_ProcesadorArchivos.Entidades
{
   public class Fila
    {
       
        public Dictionary<Int32, String> losCampos = new Dictionary<Int32, String>();

        public TipoFilaConfig elTipoFila;

        public FilaConfig laConfigDeFila;

        public String DetalleCrudo;
        

            
    }
}
