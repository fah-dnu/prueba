using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImportarEmpleados.Entidades
{
    public class RespuestaTransaccional
    {

        public String CodigoRespuesta { get; set; }
        public String DescripcionRespuesta { get; set; }
        public String ResultadoOperacion { get; set; }
        public String Autorizacion { get; set; }
        public String Saldos { get; set; }
        public DateTime FechaHoraOperacion { get; set; }
        public Boolean IsTimeOut { get; set; }
        public Boolean IsAutorizada()
        {
            if (CodigoRespuesta == "0000")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
