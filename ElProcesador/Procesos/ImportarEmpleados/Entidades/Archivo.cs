using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImportarEmpleados.Entidades
{
   public class Archivo
    {
        public Int64 ID_Archivo { get; set; }
        public Int64 ID_Estatus { get; set; }
        public String Nombre { get; set; }
        public String Fecha { get; set; }
        public String CA_Usuario { get; set; }
        public Int64 ID_CadenaComercial { get; set; }
        public String ResultadoProceso { get; set; }
        public String UrlArchivo { get; set; }
        public List<Empleado> Empleados { get; set; }
    }
}
