using DNU_ParabiliaProcesoCortes.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNCorteExterno
    {
        public static ClienteExterno obtenerCuentasPorcliente(DataTable tablaCuenta) {
            ClienteExterno cliente = new ClienteExterno();
            cliente.cuentas=new List<CuentaAhorroCLABE>();
            try {
                foreach (DataRow row in tablaCuenta.Rows) {
                    cliente.cuentas.Add(new CuentaAhorroCLABE{ cuentaAhorro = row["CuentaAhoID"].ToString(), clabe= row["CuentaClabe"].ToString()});
                }

            } catch (Exception ex) {
                
            }
            return cliente;
        }
    }
}
