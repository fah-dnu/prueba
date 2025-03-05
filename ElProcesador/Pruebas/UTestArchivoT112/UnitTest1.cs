using System;
using DNU_ProcesadorT112;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UTestArchivoT112
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void EjecutaMetodo()
        {

            try
            {
                ProcesarT112 unProceso = new ProcesarT112();
                unProceso.ProcesarloTest();

            }
            catch (Exception err)
            {

            }
    

        }
    }
}
