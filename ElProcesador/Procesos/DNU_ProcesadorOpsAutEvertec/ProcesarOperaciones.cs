using CommonProcesador;
using DNU_ProcesadorOpsAutEvertec.LogicaNegocio;
using System;

namespace DNU_ProcesadorOpsAutEvertec
{
    public class ProcesarOperaciones : IProcesoNocturno
    {
     
        bool IProcesoNocturno.Procesar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                LNArchivo.EjecutaProceso();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
            return true;
        }

 
        void IProcesoNocturno.Iniciar()
        {
            try
            {
                LNArchivo.EscucharDirectorio();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        void IProcesoNocturno.Detener()
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
