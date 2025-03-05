using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImportarEmpleados.Utilidades
{
    public enum EstatusEmpleado
    {
        SinProcesar = 1,
        ColectivaCreada = 2,
        CuentaCreada = 3,
        AbonoRealizado = 4,
        Procesada = 5,
        ErrorAlCrearColectiva = 8,
        ErrorAlCrearCuentas = 9,
        ErrorAlAbonar = 10,
        ErrorAlCrearClubEscala = 11

    };

    public enum EstatusArchivo
    {
        SinProcesar = 1,
        ParcialmenteProcesado = 2,
        Procesado = 3,
        Error = 4,
    };
}
