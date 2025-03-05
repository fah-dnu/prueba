using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Executer.Entidades
{
   public class Saldo
    {
        public String TipoCuenta { get; set; }
        public String TipoMonto { get; set; }
        public String CodigoMoneda { get; set; }
        public String PosicionesDecimales { get; set; }
        public String CreditoDebito { get; set; }
        public String Monto { get; set; }
        public int ID_Colectiva { get; set; }
        public int ID_Cuenta { get; set; }


        public String toString()
        {
            StringBuilder saldo = new StringBuilder();

            saldo.Append(this.TipoCuenta.PadLeft(2, '0'));
            saldo.Append(this.TipoMonto.PadLeft(2, '0'));
            saldo.Append(this.CodigoMoneda.PadLeft(3, 'X'));
            saldo.Append(this.PosicionesDecimales.PadLeft(1, '0'));
            saldo.Append(this.CreditoDebito.PadLeft(1, '0'));
            saldo.Append(this.Monto.PadLeft(12, '0'));

            return saldo.ToString();
        }


    }
}
