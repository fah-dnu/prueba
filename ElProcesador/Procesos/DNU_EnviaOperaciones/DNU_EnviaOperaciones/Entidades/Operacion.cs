using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_EnviaOperaciones.Entidades
{
    class Operacion
    {

        public String ID_Operacion {get; set;}
        public String MCC { get; set;}
        public String Afiliacion { get; set;}
        public String DescripcionComercio { get; set;}
        public String Tarjeta { get; set;}
        public String Fecha { get; set;}
        public String Hora { get; set;}
        public String Importe { get; set;}
        public String Autorizacion { get; set;}
        public String GrupoTarjeta { get; set;}
        public String DescGrupoT { get; set;}
        public String FirmaElectronica { get; set;}

    }
}
