using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VencimientoOperNoConciliadas;

namespace TestServiciosParabilia
{
    public class Program
    {
        static void Main(string[] args)
        {
            var process = new OperNoConciliadas();
            process.Iniciar();
            Console.ReadLine();
        }
    }
}
