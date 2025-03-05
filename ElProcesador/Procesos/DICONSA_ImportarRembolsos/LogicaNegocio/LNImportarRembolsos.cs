using CommonProcesador;
using DICONSA_ImportarRembolsos.BaseDatos;
using DICONSA_ImportarRembolsos.Entidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace DICONSA_ImportarRembolsos.LogicaNegocio
{
    /// <summary>
    /// Establece la lógica de negocio para las importación de rembolsos Diconsa
    /// </summary>
   public  class LNImportarRembolsos
    {
        /// <summary>
        /// Realiza el proceso de importación de rembolsos de todas las tiendas Diconsa válidas
        /// </summary>
        /// <returns></returns>
        public static bool ImportaRembolsos()
        {
            try
            {
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

                            //Obtiene rembolsos
                            WSJsonResponses.GetWithdrawal rembolsos = WebService.WSGetWithdrawal(token, unaTienda.ID_Colectiva);

                            //Se deserializan los rembolsos obtenidos a un objeto JSON
                            WSJsonResponses.GetWithdrawalResult wsRembolsos = JsonConvert.DeserializeObject<WSJsonResponses.GetWithdrawalResult>(rembolsos.Result.ToString());

                            //Se procesan los rembolsos de JSON a DataTable
                            DataTable dtRembolsos = WebService.procesaRembolsos(wsRembolsos, unaTienda.ID_Colectiva);

                            //Si hay rembolsos, se almacenan registros en base de datos
                            if (dtRembolsos.Rows.Count > 0)
                            {
                                InsertaRembolsosEnBD(dtRembolsos);
                            }

                            //Se actualiza estatus de procesamiento de la tienda
                            ActualizaTienda(unaTienda.ID_Colectiva);
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
                Logueo.Error("ImportaRembolsos():" + err.Message);
                return false;
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para insertar los registros de rembolsos
        /// </summary>
        /// <param name="dtRembolsos">Registros de rembolsos</param>
        public static void InsertaRembolsosEnBD(DataTable dtRembolsos)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(BDServicios.strBDEscritura))
                {
                    conn.Open();

                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {
                            Logueo.Evento("Insertando a base de datos rembolsos. Tienda: " + dtRembolsos.Rows[0]["ID_Colectiva"]);
                            DAODNU.InsertaRembolsos(dtRembolsos, conn, transaccionSQL);
                            
                            transaccionSQL.Commit();
                            Logueo.Evento("Se insertaron a base de datos los rembolsos de la tienda: " + dtRembolsos.Rows[0]["ID_Colectiva"]);
                        }

                        catch (Exception err)
                        {
                            transaccionSQL.Rollback();
                            throw err;
                        }
                    }
                }
            }

            catch (Exception err)
            {
                throw new Exception("InsertaRembolsosEnBD() " + err.Message);
            }
        }

        /// <summary>
        /// Actualiza el estatus de procesado de una tienda
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        public static void ActualizaTienda(Int32 IdColectiva)
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
                        Logueo.Evento("Se Marcó como Procesada la Tienda " + IdColectiva.ToString() + " en el Autorizador");
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
                throw new Exception("LNImportarRembolsos.ActualizaEstatusTienda() " + err.Message);
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