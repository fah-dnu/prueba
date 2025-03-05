using CommonProcesador;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TELEVIP_ImportarTipoCuenta.Utilidades;

namespace TELEVIP_ImportarTipoCuenta.BaseDatos
{
    class DAODNU
    {
       
        public static Boolean ProcesaCambiodeTipoCuenta(Int32 elTipocuenta, String CuentaOrigen,String CuentaDestino,DateTime fechacambio, SqlConnection unaConexion, SqlTransaction transaccionSQL)
        {

            try
            {
                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("ProcNoct_Televia_ProcesaCambioPospagoPrepago", unaConexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;

                param = new SqlParameter("@ID_PrepagoOrPosPago", SqlDbType.Int);
                param.Value = elTipocuenta;
                comando.Parameters.Add(param);


                param = new SqlParameter("@CuentaOrigen", SqlDbType.VarChar);
                param.Value = CuentaOrigen;
                comando.Parameters.Add(param);


                param = new SqlParameter("@CuentaDestino", SqlDbType.VarChar);
                param.Value = CuentaDestino;
                comando.Parameters.Add(param);


                param = new SqlParameter("@FechaCambio", SqlDbType.DateTime);
                param.Value = fechacambio;
                comando.Parameters.Add(param);



                comando.ExecuteNonQuery();


                return true;
            }
            catch (Exception ex)
            {

                Logueo.Error(ex.Message);
                throw ex;
            }
        }
    }
}
