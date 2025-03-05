using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;
using MonitoreaCuentas.Utilidades;
using MonitoreaCuentas.Entidades;
using CommonProcesador;
using System.Data.SqlClient;

namespace MonitoreaCuentas.BaseDatos
{
    public class DAOCuentas
    {
      
        public static List<Cuenta> ListarCuentasBajoSaldo()
        {


            List<Cuenta> lasCuentas = new List<Cuenta>();
            try
            {
                SqlDatabase database = new SqlDatabase(BDEmpleados.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_MonitorCuentas");
                
                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {

                        Cuenta unaCuenta = new Cuenta();

                        unaCuenta.CuentaHabiente = (String)losDatos.Tables[0].Rows[k]["CuentaHabiente"];
                        unaCuenta.nombreCuenta = (String)losDatos.Tables[0].Rows[k]["nombreCuenta"];
                        unaCuenta.ID_Cuenta = (Int64)losDatos.Tables[0].Rows[k]["ID_Cuenta"];
                        unaCuenta.Saldo = (Decimal)losDatos.Tables[0].Rows[k]["Saldo"];
                        unaCuenta.ListaDistribucion = (String)losDatos.Tables[0].Rows[k]["ListaDistribucion"];
                        lasCuentas.Add(unaCuenta);

                    }
                }

                return lasCuentas;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

      
    }
}
