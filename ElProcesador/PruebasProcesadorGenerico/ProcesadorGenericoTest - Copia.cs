using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNU_ProcesadorGenerico;
using CommonProcesador;


namespace PruebasProcesadorGenerico
{
    [TestClass]
    public class ProcesadorGenericoTest
    {
        [TestMethod]
        public void Procesar()
        {
            ConfiguracionContexto.InicializarContexto();

            IProcesoNocturno pg = new ProcesoGenerico();

            pg.Procesar();
        }
    }
}
