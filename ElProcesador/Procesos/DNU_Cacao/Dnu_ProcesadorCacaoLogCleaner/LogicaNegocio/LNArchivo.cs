using System;
using CommonProcesador;
using Dnu_ProcesadorCacaoLogCleaner.BaseDatos;

namespace Dnu_ProcesadorCacaoLogCleaner.LogicaNegocio
{
    class LNArchivo
    {

        public static Boolean ProcesaLimpiezaLogs(String connectionString)
        {
            try
            {
                DAODatosBase.EjecutaLimpiezaCacaoLogs(connectionString);
                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("getUsuarios(): " + err.Message);
                return false;
            }
        }

    }
}
