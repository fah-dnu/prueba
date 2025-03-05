using Dnu.Sincronizacion.Correo.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.Sincronizacion.Correo.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var process = new SincronizacionCorreoProcess();
            var res = process.Procesar();
            Console.ReadLine();
        }
    }
}
