using CommonProcesador;
using CommonProcesador.Utilidades;
using Executer.Entidades;
using Interfases.Entidades;
using MOSHI_PuntosCumpleanyos.BaseDatos;
using MOSHI_PuntosCumpleanyos.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace MOSHI_PuntosCumpleanyos.LogicaNegocio
{
    /// <summary>
    /// Establece la lógica de negocio para el abono de puntos a los cuentahabientes 
    /// Moshi-Moshi que cumplen años
    /// </summary>
    public class LNAbonoPuntos
    {
        /// <summary>
        /// Controla el proceso y validaciones el cambio de nivel de la cuenta
        /// </summary>
        /// <returns>TRUE si fue exitoso</returns>
        public static bool AbonaPuntosACuentahabientes()
        {
            try
            {
                //Se consultan los clientes que cumplen años el día de hoy
                DataSet dsClientes = DAOEcommerce.ConsultaClientesCumple();
                Logueo.Evento("Se Consultaron los Clientes Cumpleaños a Ecommerce");

                //Si hay clientes con cumpleaños hoy...
                if (dsClientes.Tables[0].Rows.Count > 0)
                {
                    //...se insertan en la tabla de control
                    for (int fila = 0; fila < dsClientes.Tables[0].Rows.Count; fila ++)
                    {
                        InsertaClienteATablaControl( Convert.ToInt64(
                            dsClientes.Tables[0].Rows[fila]["ID_Colectiva"].ToString().Trim()));
                    }
                    Logueo.Evento("Se Insertaron las Colectivas a la Tabla de Control del Autorizador");


                    //Se obtienen los parámetros del evento
                    Bonificacion bonificacion = new Bonificacion();

                    DataSet dsEvento = DAOAutorizador.ConsultaEventoBonoCumpleanyos();
                    Logueo.Evento("Se Consultó el Evento en el Autorizador");

                    bonificacion.IdTipoColectiva = Convert.ToInt32(dsEvento.Tables[0].Rows[0]["IdTipoColectiva"].ToString());
                    bonificacion.ClaveColectiva = dsEvento.Tables[0].Rows[0]["ClaveColectiva"].ToString().Trim();
                    bonificacion.IdEvento = Convert.ToInt32(dsEvento.Tables[0].Rows[0]["ID_Evento"].ToString().Trim());
                    bonificacion.ClaveEvento = dsEvento.Tables[0].Rows[0]["ClaveEvento"].ToString().Trim();
                    bonificacion.Concepto = dsEvento.Tables[0].Rows[0]["Descripcion"].ToString().Trim();
                    bonificacion.Observaciones = "Generado por Proceso Automático";


                    //Se consulta el listado de cuentas por bonificar puntos
                    List<Cuentas> cuentas = DAOAutorizador.ListaCuentasCumple();
                    Logueo.Evento("Se Consultó la Lista de Cuentas en el Autorizador");


                    foreach (Cuentas cuenta in cuentas)
                    {
                        if (cuenta.Procesada == false)
                        {
                            try
                            {
                                bonificacion.IdColectiva = cuenta.ID_Colectiva;
                                bonificacion.Importe = cuenta.Puntos;

                                //Se ejecuta el abono de puntos
                                RegistraEvManual_AbonoPuntos(bonificacion);

                                //Se marca la cuenta con envío exitoso de correo
                                MarcaCuentaProcesadaOK(cuenta.ID_Colectiva);
                            }

                            catch (Exception err)
                            {
                                Logueo.Error("ERROR: Bonificación de Puntos a la Cuenta de la Colectiva con ID: " + cuenta.ID_Colectiva.ToString() + ": " + err.Message);
                            }
                        }

                        if (cuenta.MailOK == false)
                        {
                            try
                            {
                                //Se envía correo al cliente notificándole del abono de puntos
                                if (EnviaCorreoCliente(cuenta.Cuentahabiente, cuenta.Email))
                                {
                                    //Se marca la cuenta con envío exitoso de correo
                                    MarcaCuentaConCorreoEnviado(cuenta.ID_Colectiva);
                                }
                            }

                            catch (Exception err)
                            {
                                Logueo.Error("ERROR: Envío de Correo Electrónico : " + cuenta.Email + ": " + err.Message);
                            }
                        }
                    }
               }
                
               LimpiaHistoricoCuentas();

               return true;
            }

            catch (Exception err)
            {
                Logueo.Error("AbonaPuntosACuentahabientes():" + err.Message);
                return false;
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para la inserción del ID de colectiva del cliente
        /// a la tabla de control del proceso en base de datos
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva por insertar</param>
        public static void InsertaClienteATablaControl(Int64 IdColectiva)
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
                        DAOAutorizador.InsertaClienteTC(IdColectiva);
                        transaccionSQL.Commit();
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
                throw new Exception("InsertaClienteATablaControl() " + err.Message);
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
        /// Establece y configura los parámetros para la ejecución del evento manual
        /// "Bono Cumpleaños"
        /// </summary>
        public static void RegistraEvManual_AbonoPuntos(Bonificacion elAbono)
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

                        Executer.BaseDatos.DAOEvento lues = new Executer.BaseDatos.DAOEvento(conn, transaccionSQL);
                       
                        //Se consultan los parámetros del contrato
                        losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                            (elAbono.ClaveColectiva, "", elAbono.ClaveEvento, "");

                        losParametros["@ID_CuentaHabiente"] = new Parametro()
                        {
                            Nombre = "@ID_CuentaHabiente",
                            Valor = elAbono.IdColectiva.ToString(),
                            Descripcion = "ID CuentaHabiente",
                            ID_TipoColectiva = elAbono.IdTipoColectiva
                        };
                        losParametros["@Importe"] = new Parametro()
                        {
                            Nombre = "@Importe",
                            Valor = elAbono.Importe,
                            Descripcion = "Importe"
                        };
                        losParametros["@MedioAcceso"] = new Parametro()
                        {
                            Nombre = "@MedioAcceso"
                        };
                        losParametros["@TipoMedioAcceso"] = new Parametro()
                        {
                            Nombre = "@TipoMedioAcceso"
                        };


                        //Genera y Aplica la Poliza
                        Executer.EventoManual aplicador = new Executer.EventoManual(elAbono.IdEvento,
                            elAbono.Concepto, false, 0, losParametros, elAbono.Observaciones, conn, transaccionSQL);
                        laPoliza = aplicador.AplicaContablilidad();

                        if (laPoliza.CodigoRespuesta != 0)
                        {
                            transaccionSQL.Rollback();
                            throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                        }

                        else
                        {
                            transaccionSQL.Commit();
                            Logueo.Evento("Se Realizó la Bonificación de Puntos a la Cuenta de la Colectiva: " + elAbono.IdColectiva. ToString());
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
                throw new Exception("RegistraEvManual_AbonoPuntos() " + err.Message);
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
        /// Establece las condiciones de validación para la marca de puntos abonados con éxito
        /// en base de datos
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva por marcar</param>
        public static void MarcaCuentaProcesadaOK(Int64 IdColectiva)
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
                        DAOAutorizador.MarcaCuentaAbonoOK(IdColectiva);
                        transaccionSQL.Commit();
                        Logueo.Evento("Se Marcó la Colectiva " + IdColectiva.ToString() + " con Abono de Puntos OK en el Autorizador");
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

        /// <summary>
        /// Establece las condiciones de validación para enviar el correo informativo
        /// al cliente de su abono de puntos
        /// </summary>
        /// <param name="cliente">Nombre del cuentahabiente</param>
        /// <param name="mailCliente">Correo electrónico del cliente</param>
        public static bool EnviaCorreoCliente(string cliente, string mailCliente)
        {
            try
            {
                if (!File.Exists(PNConfig.Get("ABONOPTS", "HTMLCorreo")))
                    return false;

                StringBuilder emailBody = new StringBuilder(File.ReadAllText(PNConfig.Get("ABONOPTS", "HTMLCorreo")));

                emailBody = emailBody.Replace("[NOMBREUSUARIO]", cliente);
                emailBody = emailBody.Replace("[CORREO]", mailCliente);

                LNEmail.Send(mailCliente, PNConfig.Get("ABONOPTS", "RemitenteCorreo"),
                    emailBody.ToString(), PNConfig.Get("ABONOPTS", "AsuntoCorreo"));

                Logueo.Evento("Se envió el correo de Bonificación de Puntos a " + cliente + " , email " + mailCliente);

                return true;
            }

            catch (Exception ex)
            {
                throw new Exception("EnviaCorreoCliente() " + ex.Message);
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para la marca de correo enviado
        /// de la cuenta en base de datos
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva por marcar</param>
        public static void MarcaCuentaConCorreoEnviado(Int64 IdColectiva)
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
                        DAOAutorizador.MarcaCuentaMailOK(IdColectiva);
                        transaccionSQL.Commit();
                        Logueo.Evento("Se Marcó la Colectiva " + IdColectiva.ToString() + " con Correo Enviado OK en el Autorizador");
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
                throw new Exception("MarcaCuentaConCorreoEnviado() " + err.Message);
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
        /// Establece las condiciones de validación para la depuración del histórico de 
        /// cuentas rewards procesadas OK en base de datos
        /// </summary>
        public static void LimpiaHistoricoCuentas()
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
                        DAOAutorizador.DepuraHistoricoCuentasOK();
                        transaccionSQL.Commit();
                        Logueo.Evento("Se Realizó la Depuración del Histórico de Cuentas Procesadas en el Autorizador");
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
                throw new Exception("LimpiaHistoricoCuentas() " + err.Message);
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
