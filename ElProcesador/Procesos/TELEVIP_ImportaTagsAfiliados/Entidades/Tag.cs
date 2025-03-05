using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TELEVIP_ImportaTagsAfiliados.Entidades
{
    class Tag
    {

       public String ID_Tag { get; set; }
       public String ID_cuentaTelevia { get; set; }
       public DateTime Fecha_Alta { get; set; }
       public Boolean Importado { get; set; }

    }
}
