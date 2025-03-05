
using Dnu.AutorizadorParabilia_NCliente;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU.AutorizadorParabilia_NCliente.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            AltaEmpleado altEmp = new AltaEmpleado();
            BajaEmpleado bajEmp = new BajaEmpleado();

            //altEmp.Iniciar();
            bajEmp.Iniciar();
        }
    }
}
