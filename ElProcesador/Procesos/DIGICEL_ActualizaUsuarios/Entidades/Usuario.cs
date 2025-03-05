using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIGICEL_ActualizaUsuarios.Entidades
{
    public class Usuario
    {
        public String ClaveOperador { get; set; }
        public String Password { get; set; }
        public String Nombre { get; set; }
        public String ApPaterno { get; set; }
        public String ApMaterno { get; set; }
        public String ClaveTienda { get; set; }
        public String TipoDigicel { get; set; }
        public String Telefono { get; set; }
        public Boolean Baja { get; set; }

    }
}
