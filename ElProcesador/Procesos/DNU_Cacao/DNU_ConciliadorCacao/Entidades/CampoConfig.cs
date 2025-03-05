using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DNU_ConciliadorCacao.Entidades
{
    public class CampoConfig
    {

       public  String Nombre {get; set;}
       public String Descripcion { get; set; }
       public Boolean EsClave { get; set; }
       public Int32 ID_TipoColectiva { get; set; }
       public Int32 PosicionInicial { get; set; }
        public Int32 PosicionFinal { get; set; }
        public Int32 Posicion { get; set; }
       public Int32 Longitud { get; set; }
       public String TipoDatoSQL { get; set; }
       public String TrimCaracteres { get; set; }


      

    }
}
