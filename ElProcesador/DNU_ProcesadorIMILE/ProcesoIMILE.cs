using CommonProcesador;
using DNU_ProcesadorIMILE.Entidades.Request;
using DNU_ProcesadorIMILE.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ProcesadorIMILE
{
    public class ProcesoIMILE : IProcesoNocturno
    {
        void IProcesoNocturno.Detener()
        {
            throw new NotImplementedException();
        }

        void IProcesoNocturno.Iniciar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                LNArchivoListener.TokenWsAppConnnect();
                LNArchivoListener.EscucharDirectorios();
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
            }
        }

        public static void Iniciar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                LNArchivoListener.EscucharDirectorios();

            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
            }
        }

        bool IProcesoNocturno.Procesar()
        {
            try
            {
                Logueo.Evento("Iniciando..");
                ConfiguracionContexto.InicializarContexto();
                LNArchivoListener.EscucharDirectorios();

                return true;

            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                return false;
            }
        }

        public static void Start()
        {
            ConfiguracionContexto.InicializarContexto();
            LNArchivoListener.EscucharDirectorios();
        }


        public static void Listening()
        {
            ConfiguracionContexto.InicializarContexto();

            LNArchivoListener.TokenWsAppConnnect();

            LNArchivoListener.EscucharDirectorios();

            //Thread.Sleep(4400000);
        }
    }
}
