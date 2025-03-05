
using DNU_ParabiliaAltaTarjetasNominales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)  
        {
            string email = "akakak|110101";
            string email2 = "ooooo";
            string corte = email.Split('|')[0];
            string corte2 = email2.Split('|')[0];
            if (email.Contains("|")) {
                int nn = 22;
            }
            
                
            string monto = "1000000000";
            decimal montodeicmal = Convert.ToDecimal(100000000.50);
            monto = monto.Insert(monto.Length - 2,".");

            ////
            new Program().iniciar();
        }

        public void iniciar() {
            AltaTarjetasNominativas ac = new AltaTarjetasNominativas();
            ac.Iniciar();
        }
       
    }
}
