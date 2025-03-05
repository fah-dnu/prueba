//using CommonProcesador;
using Dnu_ProcesadorSQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using DNU_Embozador;
using DNU_CompensadorParabiliumProcesador;
using DNU_CompensadorParabiliumMigration;

namespace Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            //ProcesarSQLite procesarSQLite = new ProcesarSQLite();
            ////procesarSQLite.Procesar();
            //Smartpoints smartpoints = new Smartpoints();
            //smartpoints.ProcesarTesteo();



            //ConfiguracionContexto.InicializarContexto();

            //IProcesoNocturno pg = new Smartpoints();

            //pg.Iniciar();
            //Console.WriteLine("Press 'q' to quit the sample.");
            //while (Console.Read() != 'q') ;
            //ConfiguracionContexto.InicializarContexto();

            //ParabiliumProcesadorDevoluciones.DevolucionRecursos();
            Migrate.Start();

            //Manager.Start();
            //ParabiliumProcesador.Start();
        }
    }
}
