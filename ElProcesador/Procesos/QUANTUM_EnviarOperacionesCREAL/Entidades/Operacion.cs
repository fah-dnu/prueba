using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QUANTUM_EnviarOperacionesCREAL.Entidades
{
   public class Operacion
    {

            public Int64 ID_Operacion {get;set;}
            public Int64 ID_OperacionActualizar { get; set; }
            public String Beneficiario {get;set;}
            public String Tarjeta {get;set;}
            public String ID_Cliente {get;set;}
            public DateTime FechaRegistro {get;set;}
            public float Importe {get;set;}
            public String CodigoMoneda {get;set;}
            public String Sucursal {get;set;}
            public String Afiliacion {get;set;}
            public String Terminal {get;set;}
            public String Ticket {get;set;}
            public String Operador {get;set;}
            public String Autorizacion {get;set;}
            public String Mensualidades {get;set;}
            public String TipoOperacion {get;set;}
            public String CodigoProceso {get;set;}
            public String PagoInicial { get; set; }
            public DateTime FechaAlta { get; set; }

    }
}
