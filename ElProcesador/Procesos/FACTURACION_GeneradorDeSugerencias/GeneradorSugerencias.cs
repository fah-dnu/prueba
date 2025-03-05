using CommonProcesador;
using DALCentralAplicaciones.LogicaNegocio;
using FACTURACION_Generador.LogicaNegocio;
using System;

namespace FACTURACION_Generador
{
    class GeneradorSugerencias : IProcesoNocturno
    {
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                Logueo.Evento("Se Inicializa Contexto");
                ValoresInicial.InicializarContexto();
                Logueo.Evento("Finaliza Carga de Contexto Inicial");

                try
                {
                    Logueo.Evento("Se Inicia Proceso de Generador de Facturas");
                    LNSugerencias.Sugiere();
                    Logueo.Evento("Finalizó el Proceso de Generador de Facturas");

                    return true;
                }
                catch (Exception err)
                {
                    Logueo.Error(err.Message);
                    return false;
                }
            }

            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {
        }

        void IProcesoNocturno.Detener()
        {
        }


    }
}
