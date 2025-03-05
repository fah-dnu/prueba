using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Common.Funciones
{
    public class FuncionesTablas
    {
        public static DataTable onbtenerValoresTablaEncriptadosAE(DataTable tabla)
        {
            try
            {
                foreach (DataRow fila in tabla.Rows)
                {
                    if (!string.IsNullOrEmpty(fila["encriptado"].ToString()))
                    {
                        fila["valor"] = fila["encriptado"].ToString();
                    }
                }
                tabla.AcceptChanges();
            }
            catch (Exception ex) {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al descencriptar columna"+ ex.Message);

            }
           
            return tabla;

        }
    }
}
