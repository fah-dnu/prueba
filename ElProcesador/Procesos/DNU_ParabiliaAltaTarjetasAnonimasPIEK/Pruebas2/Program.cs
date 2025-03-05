
using DNU_ParabiliaAltaTarjetasAnonimasPIEK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pruebas2
{
    class Program
    {

        static void Main(string[] args)
        {

            ////
            new Program().iniciar();
        }

        public void iniciar()
        {
            try
            {
                ProcesarAltaTarjetasAnonimas ac = new ProcesarAltaTarjetasAnonimas();
                ac.Iniciar();
            }
            catch (Exception ex)
            {
                string s = "";
            }
        }
    }
}
