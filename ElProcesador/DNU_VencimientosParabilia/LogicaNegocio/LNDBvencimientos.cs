using CommonProcesador;
using DNU_VencimientosParabilia.BaseDatos;
using DNU_VencimientosParabilia.Entidades;
using DNU_VencimientosParabilia.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_VencimientosParabilia.LogicaNegocio
{
    public class LNDBvencimientos
    {
        public static IEnumerable<ColectivaContrato> obtenerColectivas()
        {
            return DAODatosBase.obtenerColectivas(BDAutorizador.strBDLectura);
        }

        public static void ActualizarMovimientosPorColectivaDB(ColectivaContrato poColectiva)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            Resultado res = DAODatosBase.actualizarMovimientosAVencerPorColectiva(BDAutorizador.strBDEscritura, poColectiva);
            if (!res.Codigo.Equals("00"))
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ActualizarMovimientosPorColectivaDB, No se actualizaron correctamente las operaciones de la colectiva : " + poColectiva.ClaveColectiva + " - " + res.Mensaje + "]");
             }
        }

        internal static DataTable obtieneMovimientosPorVencer()
        {////
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            DataTable res = DAODatosBase.obtieneMovimientosAVencer(BDAutorizador.strBDLectura);
            if (res.Rows.Count == 0)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [obtieneMovimientosPorVencer, No se obtuvieron movimientos por vencer]");
             }

            return res;
        }

        internal static bool ValidaSaldosInternos(Movimiento mov)
        {
            return DAODatosBase.ValidaOperacionSaldosInternos(BDAutorizador.strBDLectura, mov);

        }
    }
}
