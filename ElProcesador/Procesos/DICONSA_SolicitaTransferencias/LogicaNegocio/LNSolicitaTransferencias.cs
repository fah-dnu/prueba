using CommonProcesador;
using DICONSA_SolicitaTransferencias.BaseDatos;
using DICONSA_SolicitaTransferencias.Entidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;



namespace DICONSA_SolicitaTransferencias.LogicaNegocio
{
    /// <summary>
    /// Establece la lógica de negocio para las solicitud de transferencias a las tiendas Diconsa
    /// </summary>
    public class LNSolicitaTransferencias
    {
        /// <summary>
        /// Realiza el proceso de solicitud de transferencias de todas las tiendas Diconsa válidas
        /// </summary>
        /// <returns>TRUE en caso de éxito</returns>
        public static bool SolicitudDeTransferencias()
        {
            try
            {
                Int64 idCuenta = 0;

                //Consultamos la lista de tiendas activas del día de hoy
                List<TiendaDiconsa> tiendasDiconsa = DAODNU.ListaTiendasActivas();

                foreach (TiendaDiconsa unaTienda in tiendasDiconsa)
                {
                    if (unaTienda.Procesada == false)
                    {
                        try
                        {
                            //Login
                            string token = WebService.WSLogin(unaTienda.ID_Colectiva);

                            //Obtiene la cuenta bancaria
                            WSJsonResponses.GetBankAccounts wsBankAccounts = WebService.WSGetBankAccounts(token);

                            //Se procesan las cuentas y se obtiene la primera activa y default
                            WSJsonResponses.GetBankAccountsResult[] lasCuentas = JsonConvert.DeserializeObject<WSJsonResponses.GetBankAccountsResult[]>(wsBankAccounts.Result.ToString());
                            foreach (WSJsonResponses.GetBankAccountsResult cuenta in lasCuentas)
                            {
                                if (cuenta.Active && cuenta.Default)
                                {
                                    idCuenta = cuenta.Id;
                                    break;
                                }
                            }

                            //Se solicita la transferencia
                            WebService.WSPostTransfer(token, idCuenta, unaTienda.ID_Colectiva);

                            //Actualiza el estatus de transferencia de la tienda
                            ActualizaEstatusTienda(unaTienda.ID_Colectiva);
                        }

                        catch (Exception err)
                        {
                            Logueo.Error("ERROR: Tienda: " + unaTienda.ID_Colectiva.ToString() + ": " + err.Message);
                        }
                    }
                }

                return true;
            }

            catch (Exception err)
            {
                Logueo.Error("SolicitudDeTransferencias():" + err.Message);
                return false;
            }
        }

        /// <summary>
        /// Actualiza el estatus de transferencia de una tienda
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        public static void ActualizaEstatusTienda(Int32 IdColectiva)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection (BDAutorizador.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAODNU.ActualizaEstatusTienda(IdColectiva);
                        transaccionSQL.Commit();
                        Logueo.Evento("Se Actualizó el Estatus de Transferencia de la Tienda " + IdColectiva.ToString() + " en el Autorizador");
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        throw err;
                    }
                }
            }

            catch (Exception err)
            {
                throw new Exception("LNSolicitaTransferencias.ActualizaEstatusTienda() " + err.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }
        }
    }
}
