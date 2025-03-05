using CommonProcesador;
using CommonProcesador.Utilidades;
using MOSHI_CambioNivel.BaseDatos;
using MOSHI_CambioNivel.Entidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;


namespace MOSHI_CambioNivel.LogicaNegocio
{
    /// <summary>
    /// Establece la lógica de negocio para el cambio de nivel de los cuentahabientes Moshi-Moshi
    /// </summary>
    public class LNCambiaNivel
    {
        /// <summary>
        /// Controla el proceso y validaciones el cambio de nivel de la cuenta
        /// </summary>
        /// <returns>TRUE si fue exitoso</returns>
        public static bool CambiaNivelCuentahabientes()
        {
            try
            {
                //Se consultan las cuentas 
                List<Cuentas> cuentas = DAOAutorizador.ListaCuentasRewards();

                foreach (Cuentas cuenta in cuentas)
                {
                    if (cuenta.Procesada == false)
                    {
                        try
                        {
                            //Se cambia el nivel de la cuenta
                            CambiaNivelACuenta(cuenta.ID_Cuenta);
                        }

                        catch (Exception err)
                        {
                            Logueo.Error("ERROR: Cambio de Nivel de la Cuenta: " + cuenta.ID_Cuenta.ToString() + ": " + err.Message);
                        }
                    }

                    if (cuenta.MailOK == false)
                    {
                        try
                        {
                            //Se envía correo al cliente notificándole del cambio de su cuenta
                            EnviaCorreoCliente(cuenta.Cuentahabiente, cuenta.Email);

                            //Se marca la cuenta con envío exitoso de correo
                            MarcaCuentaConCorreoEnviado(cuenta.ID_Cuenta);
                        }

                        catch (Exception err)
                        {
                            Logueo.Error("ERROR: Envío de Correo Electrónico : " + cuenta.Email + ": " + err.Message);
                        }
                    }
                }

                LimpiaHistoricoCuentas();


                 //Se consultan las cuentas a las que se reiniciará su saldo en visitas 
                List<Cuentas> cuentasReset = DAOAutorizador.ListaCuentasReset();

                foreach (Cuentas cuentaReset in cuentasReset)
                {
                    if (cuentaReset.Reseteada == false)
                    {
                        try
                        {
                            //Se resetea el saldo de la cuenta
                            ReseteaSaldoVisitasACuenta(cuentaReset.ID_Cuenta);
                        }

                        catch (Exception err)
                        {
                            Logueo.Error("ERROR: Reinicio de Saldo de Visitas de la Cuenta: " + cuentaReset.ID_Cuenta.ToString() + ": " + err.Message);
                        }
                    }
                }

                LimpiaHistoricoCuentasReset();

                return true;
            }

            catch (Exception err)
            {
                Logueo.Error("CambiaNivelCuentahabientes():" + err.Message);
                return false;
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para el cambio de nivel de la cuenta
        /// en base de datos
        /// </summary>
        /// <param name="IdCuenta">Identificador de la cuenta por cambiar</param>
        public static void CambiaNivelACuenta(Int32 IdCuenta)
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
                        DAOAutorizador.CambiaNivelDeCuenta(IdCuenta);
                        transaccionSQL.Commit();
                        Logueo.Evento("Se Cambió la Cuenta " + IdCuenta.ToString() + " de Nivel en el Autorizador");
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
                throw new Exception("CambiaNivelACuenta() " + err.Message);
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
        /// al cliente de su cambio de nivel de lealtad
        /// </summary>
        /// <param name="cliente">Nombre del cuentahabiente</param>
        /// <param name="mailCliente">Correo electrónico del cliente</param>
        public static void EnviaCorreoCliente(string cliente, string mailCliente)
        {
            try
            {
                StringBuilder emailBody = new StringBuilder(File.ReadAllText(PNConfig.Get("CAMBIONVL", "HTMLCorreo")));

                emailBody = emailBody.Replace("[NOMBREUSUARIO]", cliente);
                emailBody = emailBody.Replace("[CORREO]", mailCliente);

                LNEmail.Send(mailCliente, PNConfig.Get("CAMBIONVL", "RemitenteCorreo"),
                        emailBody.ToString(), PNConfig.Get("CAMBIONVL", "AsuntoCorreo"));

                Logueo.Evento("Se envió el correo de Cambio de Nivel de Lealtad a " + cliente + " , email " + mailCliente);
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
        /// <param name="IdCuenta">Identificador de la cuenta por marcar</param>
        public static void MarcaCuentaConCorreoEnviado(Int32 IdCuenta)
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
                        DAOAutorizador.MarcaCuentaMailOK(IdCuenta);
                        transaccionSQL.Commit();
                        Logueo.Evento("Se Marcó la Cuenta " + IdCuenta.ToString() + " con Correo Enviado OK en el Autorizador");
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

        /// <summary>
        /// Establece las condiciones de validación para el reinicio a ceros del saldo
        /// en visitas de la cuenta indicada en base de datos
        /// </summary>
        /// <param name="IdCuenta">Identificador de la cuenta por reiniciar</param>
        public static void ReseteaSaldoVisitasACuenta(Int32 IdCuenta)
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
                        DAOAutorizador.ReseteaSaldoCuenta(IdCuenta);
                        transaccionSQL.Commit();
                        Logueo.Evento("Se estableció en ceros el saldo de la cuenta " + IdCuenta.ToString() + " en el Autorizador");
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
                throw new Exception("ReseteaSaldoVisitasACuenta() " + err.Message);
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
        /// cuentas cuyo saldo se reinició a ceros OK en base de datos
        /// </summary>
        public static void LimpiaHistoricoCuentasReset()
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
                        DAOAutorizador.DepuraHistoricoCuentasResetOK();
                        transaccionSQL.Commit();
                        Logueo.Evento("Se Realizó la Depuración del Histórico de Cuentas Reseteadas en el Autorizador");
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
                throw new Exception("LimpiaHistoricoCuentasReset() " + err.Message);
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
