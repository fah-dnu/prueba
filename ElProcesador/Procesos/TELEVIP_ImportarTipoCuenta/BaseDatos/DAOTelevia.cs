using CommonProcesador;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace TELEVIP_ImportarTipoCuenta.BaseDatos
{
    class DAOTelevia
    {


        public static List<CuentaTeleVIA> ObtenerNuevasCuentasAsociadas()
        {
            List<CuentaTeleVIA> losNuevosTags = new List<CuentaTeleVIA>();


            try
            {
                //  Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDTelevip.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("TVIP_ConsultaMigracionCuentas");


                laTabla = database.ExecuteDataSet(command).Tables[0];

                foreach (DataRow renglon in laTabla.Rows)
                {
                    try
                    {
                        CuentaTeleVIA unaCuenta = new CuentaTeleVIA();

                        unaCuenta.CuentaDestino = (renglon["CuentaDestino"].ToString());
                        unaCuenta.ID_Registro = Int32.Parse(renglon["id_reg"].ToString());

                        unaCuenta.CuentaOrigen = (renglon["CuentaOrigen"].ToString());

                        unaCuenta.FechaBaja = DateTime.Parse(renglon["FechaBaja"].ToString());
                        unaCuenta.FechaMigracion = DateTime.Parse(renglon["FechaMigracion"].ToString());
                        unaCuenta.NuevoTipoCuenta = Int32.Parse(renglon["TipoCuenta"].ToString());

                        losNuevosTags.Add(unaCuenta);
                    } catch(Exception err)
                    {
                        Logueo.Error("ObtenerNuevasCuentasAsociadas(): ID_Registro[" + renglon["id_reg"].ToString() + "]" + err.Message);
                    }
                }

                return losNuevosTags;
            }
            catch (Exception ex)
            {

                Logueo.Error("ObtenerNuevasCuentasAsociadas()" + ex.Message);
                throw ex;
            }
        }


        public static Boolean setImportado(Int64 elIdRegistro, SqlConnection conn)
        {
           
            try
            {

                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("TVIP_SetMigracionOK", conn);
                comando.CommandType = CommandType.StoredProcedure;


                param = new SqlParameter("@ID_Registro", SqlDbType.BigInt);
                param.Value = elIdRegistro;
                comando.Parameters.Add(param);


                comando.ExecuteNonQuery();

                return true;

            }
            catch (Exception ex)
            {

                Logueo.Error("setImportado()" + ex.Message);
                throw ex;
            }
        }


    }
}
