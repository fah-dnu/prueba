using CommonProcesador;
using Executer.Entidades;
using Interfases.Entidades;
using MOSHI_PuntosVencidos.BaseDatos;
using MOSHI_PuntosVencidos.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MOSHI_PuntosVencidos.LogicaNegocio
{
    /// <summary>
    /// Establece la lógica de negocio para el vencimiento de puntos expirados a las cuentas Moshi
    /// </summary>
    public class LNExpiraPuntos
    {
        /// <summary>
        /// Controla el proceso y validaciones del vecimiento de puntos de la cuenta
        /// </summary>
        /// <returns>TRUE si fue exitoso</returns>
        public static bool VencePuntosDeCuentas()
        {
            try
            {
                //Se consultan las cuentas 
                List<Cuentas> cuentas = DAOAutorizador.ListaCuentasConPuntosVencidos();
                Logueo.Evento("Se Consultaron las Cuentas con Puntos por Vencer en el Autorizador");

                if (cuentas.Count > 0)
                {
                    EventoManual elEvento = new EventoManual();

                    DataSet dsEvento = DAOAutorizador.ConsultaEventoPuntosVencidos();

                    elEvento.IdEvento = Convert.ToInt32(dsEvento.Tables[0].Rows[0]["ID_Evento"].ToString());
                    elEvento.ClaveEvento = dsEvento.Tables[0].Rows[0]["ClaveEvento"].ToString();
                    elEvento.Concepto = dsEvento.Tables[0].Rows[0]["Descripcion"].ToString();
                    elEvento.ClaveCadenaComercial = PNConfig.Get("VENCEPTS", "ClaveColectivaMoshi");

                    foreach (Cuentas cuenta in cuentas)
                    {
                        try
                        {
                            elEvento.IdColectiva = Convert.ToInt64(cuenta.ID_Colectiva);
                            elEvento.IdTipoColectiva = Convert.ToInt32(cuenta.ID_TipoColectiva);
                            elEvento.Importe = cuenta.PuntosAVencer.ToString();
                            elEvento.Observaciones = PNConfig.Get("VENCEPTS", "ObservacionesPuntosVencidos");

                            //Se realiza el cargo por los puntos vencidos
                            RegistraEvManual_PuntosVencidos(elEvento);

                            //Se marca la cuenta como procesada OK
                            MarcaCuentaProcesadaOK(cuenta.ID_Colectiva, cuenta.ID_Cuenta);
                        }

                        catch (Exception err)
                        {
                            Logueo.Error("ERROR: Puntos Vencidos de la Cuenta: " + cuenta.ID_Cuenta.ToString() + ": " + err.Message);
                        }
                    }
                }
                
               return true;
            }

            catch (Exception err)
            {
                Logueo.Error("VencePuntosDeCuentas():" + err.Message);
                return false;
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para la ejecución del evento manual
        /// "Puntos Vencidos", configurándolo
        /// </summary>
        /// <param name="elCargo">Datos del evento manual</param>
        public static void RegistraEvManual_PuntosVencidos(EventoManual elCargo)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(BDAutorizador.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        Poliza laPoliza = null;

                        Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                        //Se consultan los parámetros del contrato
                        losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                            (elCargo.ClaveCadenaComercial, "", elCargo.ClaveEvento, "");

                        losParametros["@ID_CuentaHabiente"].Valor = elCargo.IdColectiva.ToString();
                        losParametros["@Importe"] = new Parametro()
                        {
                            Nombre = "@Importe",
                            Valor = elCargo.Importe,
                            Descripcion = "Importe"
                        };

                        //Genera y aplica la Póliza
                        Executer.EventoManual aplicador = new Executer.EventoManual(elCargo.IdEvento,
                            elCargo.Concepto, false, 0, losParametros, elCargo.Observaciones, conn, transaccionSQL);
                        laPoliza = aplicador.AplicaContablilidad();

                        if (laPoliza.CodigoRespuesta != 0)
                        {
                            transaccionSQL.Rollback();
                            throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                        }

                        else
                        {
                            transaccionSQL.Commit();
                            Logueo.Evento("Se Realizó el Cargo de Puntos Vencidos a la Cuenta de la Colectiva: " + elCargo.IdColectiva.ToString());
                        }
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
                throw new Exception("RegistraEvManual_PuntosVencidos() " + err.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para la marca de una cuenta
        /// con vencimiento de puntos exitoso en base de datos
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        /// <param name="IdCuenta">Identificador de la cuenta</param>
        public static void MarcaCuentaProcesadaOK(Int64 IdColectiva, Int64 IdCuenta)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(BDAutorizador.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAOAutorizador.MarcaCuentaVencimientoPtsOK(IdColectiva, IdCuenta);
                        transaccionSQL.Commit();
                        Logueo.Evento("Se marcó la Cuenta " + IdCuenta.ToString() + " con vencimiento de puntos OK en el Autorizador");
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
                throw new Exception("MarcaCuentaProcesadaOK() " + err.Message);
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
