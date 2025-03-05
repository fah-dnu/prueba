using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using TELEVIP_ImportarTipoCuenta.BaseDatos;

namespace TELEVIP_ImportarTipoCuenta.LogicaNegocio
{
   public  class LNTipoCuenta
    {
        public static bool ActualizaTipoCuentaPrepagoPostpago()
        {
            Boolean laRespuesta = false;
            try
            {
                List<CuentaTeleVIA> lasCuentas =  DAOTelevia.ObtenerNuevasCuentasAsociadas();

                SqlConnection connTV = BDTelevip.BDEscritura;

                using (SqlConnection conn = new SqlConnection(BaseDatos.BDAutorizador.strBDEscritura))
                {
                    conn.Open();

                    foreach (CuentaTeleVIA laCuenta in lasCuentas)
                    {
                        using (SqlTransaction transaccionSQL = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            try
                            {
                                Logueo.Evento("IMPORTADO: Origen:" + laCuenta.CuentaOrigen + ", Destino:" + laCuenta.CuentaDestino);

                               if( DAODNU.ProcesaCambiodeTipoCuenta(
                                   laCuenta.NuevoTipoCuenta
                                  ,laCuenta.CuentaOrigen
                                   ,   laCuenta.CuentaDestino 
                                    , laCuenta.FechaMigracion
                                     ,conn
                                    ,transaccionSQL
                                  ))
                               {
                                   DAOTelevia.setImportado(laCuenta.ID_Registro, connTV);

                                   transaccionSQL.Commit();
                               }
                               else
                               {
                                   transaccionSQL.Rollback();
                               }

                                
                            }
                            catch (Exception err)
                            {
                                transaccionSQL.Rollback();

                                Logueo.Error("ERROR: ActualizaTipoCuentaPrepagoPostpago(): la Cuenta:" + laCuenta.CuentaOrigen + ", Destino" + laCuenta.CuentaDestino + err.Message);
                            }
                        }
                    }

                }

            }
            catch (Exception err)
            {
                Logueo.Error("ActualizaTipoCuentaPrepagoPostpago():" + err.Message);


            }
            return laRespuesta;
        }
    }

}