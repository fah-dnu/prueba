using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dnu.Batch.Bonificacion.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var process = new BonificacionProcess();
            var res = process.Procesar();
            Console.ReadLine();
        }
    }
}
