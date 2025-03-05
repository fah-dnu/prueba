using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNU_Ejecutor.Entidades
{
   public class Evento
    {
       public Int64 ID_ConfiguracionCorte { get; set; }

       public Int64 ID_AgrupadorEvento { get; set; }
       public String ClaveEvento {get;set;}
       public int ID_Evento { get; set; }
       public String Descripcion { get; set; }

       public String ClaveCadenaComercial { get; set; }

       public Int64 ID_CadenaComercial { get; set; }

       public Int64 ID_Contrato { get; set; }

       public Int64 Consecutivo { get; set; }


    }
}
