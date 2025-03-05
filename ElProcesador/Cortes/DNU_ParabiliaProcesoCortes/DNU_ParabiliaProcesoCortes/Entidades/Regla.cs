using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class Regla
    {
        public String Nombre { get; set; }
        public StringBuilder CadenaEjecucion { get; set; }
        public String StoredProcedure { get; set; }
        public int OrdenEjecucion { get; set; }
        public Boolean esAccion { get; set; }
        public List<Parametro> Parametros { get; set; }

        /**
         * @return the _cadenaEjecucion
         * Genera la cadena con que se ejecutara el SP
         */
        public String getCadenaEjecucion()
        {

            int i = 0;
            //"{call trx_ObtieneReglasDePertenencia(?)}"
            CadenaEjecucion.Append("{call ");
            CadenaEjecucion.Append(this.Nombre);
            CadenaEjecucion.Append("( ");

            for (i = 0; i < this.Parametros.Count; i++)
            {
                CadenaEjecucion.Append("?,");
            }
            CadenaEjecucion.Append("?,? )}"); //se ponen otros 2 parametro que es el de salida @Respuesta y @Descripcion
            return CadenaEjecucion.ToString();


        }
    }


}
