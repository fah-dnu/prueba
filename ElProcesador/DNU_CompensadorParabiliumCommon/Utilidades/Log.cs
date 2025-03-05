using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Utilidades
{
    public static class Log
    {
        private static readonly string process = "COMP-PARABILIUM";

        public static void Evento(String evt)
        {
            Logueo.Evento(String.Format("[{0}] - {1}", process,evt));
        }

        public static void Error(Exception ex)
        {
            Logueo.Error(String.Format("[{0}] - {1}", process, ex.Message));
        }

        public static void Error(String ex)
        {
            Logueo.Error(String.Format("[{0}] - {1}", process, ex));
        }

        public static void EntradaSalida(String ent, bool esEntrada)
        {
            Logueo.EntradaSalida(String.Format("[{0}] - {1}", process, ent)," PROCNOC", esEntrada);
        }

        public static void EventoDebug(String ent)
        {
            try
            {
                Logueo.EventoDebug(String.Format("[{0}] - {1}", process, ent));
            }
            catch(Exception ex)
            {
                Log.Error("Error al realizar loggeo de DEBUG "+ ex.Message);
            }
        }


        public static void EventoInfo(String ent)
        {
            Logueo.EventoInfo(String.Format("[{0}] - {1}", process, ent));
        }
    }
}
