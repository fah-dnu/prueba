using CommonProcesador;
using DICONSA_ImportarTXTiendas.BaseDatos;
using DICONSA_ImportarTXTiendas.Entidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;


namespace DICONSA_ImportarTXTiendas.LogicaNegocio
{
    /// <summary>
    /// Establece la lógica de negocio para las importación de transacciones Diconsa
    /// </summary>
   public  class LNImportarTransacciones
    {
        /// <summary>
        /// Realiza el proceso de importación de operaciones de todas las tiendas Diconsa válidas
        /// </summary>
        /// <returns></returns>
        public static bool ImportaOperaciones()
        {
            try
            {
                //Consultamos la lista de tiendas activas del día de hoy
                List<TiendaDiconsa> tiendasDiconsa = DAODNU.ListaTiendasActivas();

                foreach (TiendaDiconsa laTienda in tiendasDiconsa)
                {
                    if (laTienda.Procesada == false)

                    try
                    {
                        //Login
                        string token = WebService.WSLogin(laTienda.ID_Colectiva);

                        //Obtiene operaciones
                        WSJsonResponses.GetOperations opers = WebService.WSGetOperations(token, laTienda.ID_Colectiva);

                        //Se procesan las operaciones
                        WSJsonResponses.GetOperationsResult wsOperations = JsonConvert.DeserializeObject<WSJsonResponses.GetOperationsResult>(opers.Result.ToString());

                        //Se procesa detalle de operaciones (de JSON a DataTable) y se almacenan en BD
                        DataTable dtOperaciones = WebService.procesaOperaciones(laTienda.ID_Colectiva, wsOperations);

                        //Si hay operaciones del día, se guardan en BD
                        if (dtOperaciones.Rows.Count > 0)
                        {
                            InsertaOperacionesEnBD(dtOperaciones);    
                        }

                        ActualizaTienda(laTienda.ID_Colectiva);
                    }

                    catch (Exception err)
                    {
                        Logueo.Error("ERROR: Tienda: " + laTienda.ID_Colectiva.ToString() + ": " + err.Message);
                    }
                }

                return true;
            }

            catch (Exception err)
            {
                Logueo.Error("ImportaOperaciones():" + err.Message);
                return false;
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para insertar los registros de operaciones
        /// </summary>
        /// <param name="dtDetalleOp">Datos del detalle de operaciones</param>
        public static void InsertaOperacionesEnBD(DataTable dtDetalleOp)
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
                            Logueo.Evento("Insertando a base de datos operaciones. ID_Colectiva: " + dtDetalleOp.Rows[0]["ID_Colectiva"]);
                            DAODNU.InsertaOperaciones(dtDetalleOp, conn, transaccionSQL);

                            transaccionSQL.Commit();
                            Logueo.Evento("Se insertaron a base de datos las operaciones de la Colectiva con ID: " + dtDetalleOp.Rows[0]["ID_Colectiva"]);
                        }

                        catch (Exception err)
                        {
                            transaccionSQL.Rollback();
                            Logueo.Error("ERROR: InsertaOperacionesEnBD() ID_Colectiva: " + dtDetalleOp.Rows[0]["ID_Colectiva"] + ". Mensaje error: " + err.Message);
                        }
                    }
                }
            }

            catch (Exception err)
            {
                Logueo.Error("InsertaOperacionesEnBD():" + err.Message);
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
                throw new Exception("LNImportarTransaccciones() " + err.Message);
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