using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.Utilidades
{
    public enum TipoFilaConfig
    {
        Header=1,
        Detail=2,
        Footer=3
    }

    public enum EstatusFichero
    {
        Procesado = 1,
        ProcesadoConErrores = 2,
        EnProceso = 3,
        PorProcesar = 4

    }
}
