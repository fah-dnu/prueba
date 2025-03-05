using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonProcesador.Entidades
{
   public class Propiedad
    {

        public Propiedad()
        {

        }

        public Propiedad(String nombre, String valor, String ClaveProceso, int ID_Config, bool esSecreto)
        {
            Nombre = nombre;
            Valor = valor;
            this.ClaveProceso = ClaveProceso;
            this.ID_Config = ID_Config;
            this.EsSecreto = esSecreto;
        }

        public String Nombre { get; set; }
        public String Valor { get; set; }
        public int ID_Config { get; set; }
        public String ClaveProceso { get; set; }
        public bool EsSecreto { get; set; }
    }
}
