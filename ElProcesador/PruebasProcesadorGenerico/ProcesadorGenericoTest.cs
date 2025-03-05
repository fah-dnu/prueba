using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CommonProcesador;
using DNU_ProcesadorClaro360;

namespace PruebasProcesadorGenerico
{
    [TestClass]
    public class ProcesadorGenericoTest2
    {
        [TestMethod]
        public void Procesar()
        {


            ConfiguracionContexto.InicializarContexto();

            IProcesoNocturno pg = new ProcesarClaro360();

            pg.Procesar();
        }
    }
}
