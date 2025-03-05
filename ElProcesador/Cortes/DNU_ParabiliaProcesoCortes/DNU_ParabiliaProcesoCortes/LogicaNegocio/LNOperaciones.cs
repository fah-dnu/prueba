using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNOperaciones
    {
        //public string regresarNombreMes(int mes) {
        //    string nombreMes = "";
        //    if (mes == 1) {
        //        nombreMes = "Enero";
        //    }
        //}
        public static void crearDirectorio(string ruta) {
            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }
        }
        public static string EnmascararTarjeta(string tarjeta) {
            if (!string.IsNullOrEmpty(tarjeta)) {
                return tarjeta.Length > 4 ? "****" + tarjeta.Substring(tarjeta.Length - 4, 4) : tarjeta;
            }
            return "";
        }
    }
}
