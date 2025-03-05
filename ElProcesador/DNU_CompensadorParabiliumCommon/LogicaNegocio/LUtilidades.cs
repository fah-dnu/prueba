using DNU_CompensadorParabiliumCommon.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.LogicaNegocio
{
    public class LUtilidades
    {
        public static List<Dictionary<string, string>> GetDataTableDictionaryList(DataTable dt)
        {
            return dt.AsEnumerable().Select(
                row => dt.Columns.Cast<DataColumn>().ToDictionary(
                    column => column.ColumnName,
                    column => row[column].ToString()
                )).ToList();
        }
        public static string validarCadenasNulas(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return string.Empty;
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;
            return valor.ToString();
        }
        /// <summary>
        /// metodo que valida los contratos de base de datos
        /// </summary>
        /// <param name="tabla"></param>
        /// <returns></returns>
        public static Entidades.ValoresContratos obtenerDatosContrato(DataTable tabla)
        {
            ValoresContratos vc = new ValoresContratos();
            foreach (DataRow renglon in tabla.Rows)
            {
                if (renglon["Nombre"].ToString() == "@WS_EnmascararTarjeta")
                    vc.EnmascararTarjeta = validarCadenasNulas(renglon["Valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@reportePreliminar_CI")
                    vc.MostrarCI = validarCadenasNulas(renglon["Valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@MarkUpPorcentaje")
                    vc.MarkupPorcentaje = validarCadenasNulas(renglon["Valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@MarkUpFijo")
                    vc.MarkupFijo = validarCadenasNulas(renglon["Valor"].ToString());
            }
            return vc;
        }
    }
}
