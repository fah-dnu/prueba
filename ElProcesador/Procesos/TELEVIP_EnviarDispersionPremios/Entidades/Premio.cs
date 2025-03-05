using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TELEVIP_EnviarDispersionPremios.Entidades
{
   public  class Premio
    {

       public Int64 id { get; set; }
       public Int64 ID_Recompensa { get; set; }
       public String id_tag { get; set; }
       public float importe { get; set; }
       public DateTime f_ingreso { get; set; }
       public DateTime f_aplicacion { get; set; }
       public Int32 status { get; set; }

       public Int32 intentos { get; set; }
       public Int32 id_motivo { get; set; }
       public Boolean enviada { get; set; }

    }
}
