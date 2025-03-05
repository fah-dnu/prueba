using CommonProcesador;
using Dnu_ProcesadorCacaoLogCleaner.LogicaNegocio;
using System;

namespace Dnu_ProcesadorCacaoLogCleaner
{
#if !DEBUG
    public class ProcesarCacaoLogClean : IProcesoNocturno
#endif
#if DEBUG
    public class ProcesarCacaoLogClean
#endif
    {
        public bool Procesar()
        {

            try
            {
                int resp;

                resp = 0;

                try
                {
                    ConfiguracionContexto.InicializarContexto();
                    var config = PNConfig.Get("PROCCACOLOGCLEAN", "BDReadLogs");


                    Boolean resp2 = LNArchivo.ProcesaLimpiezaLogs(config);
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


                if (resp == 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }


        }

        public void Iniciar()
        {

            try
            {
                //LNUsuario.EscucharDirectorio();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        public void Detener()
        {

            try
            {
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }
    }
}
