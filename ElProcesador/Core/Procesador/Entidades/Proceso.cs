using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcesadorNocturno.Entidades
{
    public class Proceso
    {
       public int ID_Proceso { get; set; }
       public int ID_Ejecucion { get; set; }
       public String Clave { get; set; }
       public String Nombre { get; set; }
       public String Descripcion { get; set; }
       public String Ensamblado { get; set; }
       public String Clase { get; set; }
       public Boolean Lun { get; set; }
       public Boolean Mar { get; set; }
       public Boolean Mie { get; set; }
       public Boolean Jue { get; set; }
       public Boolean Vie { get; set; }
       public Boolean Sab { get; set; }
       public Boolean Dom { get; set; }

    }
}
