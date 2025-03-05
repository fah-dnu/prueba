using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonProcesador
{
    public interface IProcesoNocturno
    {
        Boolean Procesar();
        void Iniciar();
        void Detener();

    }


    public enum enumEstatusEjecucion
    {
       EnProceso=1,
        Procesado=2,
        Error=3

    };
}
