using DNU_CompensadorParabiliumCommon.Entidades;
using Executer.Model;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.LogicaNegocio
{
    public static class LCompensacion
    {
        public static CompensacionModel GetCompensacionDTO(Movimiento elMovimiento, float importeUSD, Dictionary<string, Parametro> losParametros)
        {
            Int64 llop = losParametros.ContainsKey("@ID_OperacionOriginal") ? Convert.ToInt64(losParametros["@ID_OperacionOriginal"].Valor) : 
                    losParametros.ContainsKey("@ID_Operacion") ? Convert.ToInt64(losParametros["@ID_Operacion"].Valor) : 0;

            return new CompensacionModel
            {
                Tarjeta = elMovimiento.ClaveMA,
                Importe = float.Parse(elMovimiento.Importe),
                Autorizacion = elMovimiento.Autorizacion,
                FechaOperacion = elMovimiento.FechaOperacion,
                Ticket = elMovimiento.Ticket,
                T112_CodigoMonedaLocal = elMovimiento.T112_CodigoMonedaLocal,
                T112_CuotaIntercambio = elMovimiento.T112_CuotaIntercambio,
                T112_ImporteCompensacionDolares = elMovimiento.T112_ImporteCompensadoDolar,
                T112_ImporteCompensacionLocal = elMovimiento.T112_ImporteCompensadoLocal,
                T112_ImporteCompensacionPesos = elMovimiento.T112_ImporteCompensadoPesos,
                T112_IVA = elMovimiento.T112_IVA,
                ID_Operacion = llop,
                CodigoProceso = losParametros["@T112_ProcessingCode"].Valor,
                T112_FechaPresentacion = elMovimiento.T112_FechaPresentacion,
                T112_NombreArchivo = elMovimiento.T112_NombreArchivo,
                //Nuevos Campos
                T112_CodigoTx = elMovimiento.ClaveEvento ?? "",
                T112_Comercio = elMovimiento.T112_Comercio ?? "",
                T112_Ciudad = elMovimiento.T112_Ciudad ?? "",
                T112_Pais = elMovimiento.T112_Pais ?? "",
                T112_MCC = elMovimiento.T112_MCC ?? "",
                T112_Moneda1 = elMovimiento.MonedaOriginal ?? "",
                T112_Moneda2 = elMovimiento.MonedaDestino ?? "",
                T112_Referencia = elMovimiento.T112_Referencia ?? "",
                T112_FechaProc = elMovimiento.T112_FechaProc ?? "",
                T112_FechaJuliana = elMovimiento.T112_FechaJuliana ?? "",
                T112_FechaConsumo = elMovimiento.T112_FechaConsumo ?? "",
                T112_Ciclo = elMovimiento.T112_Ciclo ?? ""
            };
        }
    }
}
