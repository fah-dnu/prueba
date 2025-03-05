using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MOSHI_CambioNivel.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace MOSHI_CambioNivel.BaseDatos
{
    /// <summary>
    /// Clase de control de objetos de acceso a datos del Autorizador
    /// </summary>
    class DAOAutorizador
    {
        /// <summary>
        /// Consulta el listado de cuentas de tipo Rewards que superaron el número de visitas
        /// para cambiar su nivel en base de datos
        /// </summary>
        /// <returns>DataSet con los registros</returns>
        public static List<Cuentas> ListaCuentasRewards()
        {
            try
            {
                DataSet dsCuentas = new DataSet();
                List<Cuentas> response = new List<Cuentas>();

                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ObtieneCuentasReward");

                dsCuentas = database.ExecuteDataSet(command);

                if (null != dsCuentas)
                {
                    for (int counter = 0; counter < dsCuentas.Tables[0].Rows.Count; counter++)
                    {
                        response.Add(new Cuentas(
                            Convert.ToInt32(dsCuentas.Tables[0].Rows[counter]["ID_Cuenta"].ToString()),
                            dsCuentas.Tables[0].Rows[counter]["Cuentahabiente"].ToString(),
                            dsCuentas.Tables[0].Rows[counter]["Email"].ToString(),
                            Convert.ToBoolean(dsCuentas.Tables[0].Rows[counter]["Procesada"].ToString()),
                            Convert.ToBoolean(dsCuentas.Tables[0].Rows[counter]["MailOK"].ToString())));
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("ListaCuentasRewards(): " + ex.Message);
            }
        }

        /// <summary>
        /// Cambia el nivel de la cuenta de Rewards a Fan y la marca con cambio exitoso
        /// en base de datos
        /// </summary>
        /// <param name="IdCuenta">Identificador de la cuenta por cambiar</param>
        public static void CambiaNivelDeCuenta(Int32 IdCuenta)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ActualizaCuentasReward");

                database.AddInParameter(command, "@IdCuenta", DbType.Int32, IdCuenta);

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("DAOAutorizador.CambiaNivelDeCuenta(): " + ex.Message);
            }
        }

        /// <summary>
        /// Marca la cuenta con correo enviado exitosamente en base de datos
        /// </summary>
        /// <param name="IdCuenta">Identificador de la cuenta por cambiar</param>
        public static void MarcaCuentaMailOK(Int32 IdCuenta)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ActualizaCuentaRewardMailEnviado");

                database.AddInParameter(command, "@IdCuenta", DbType.Int32, IdCuenta);

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("DAOAutorizador.MarcaCuentaMailOK(): " + ex.Message);
            }
        }

        /// <summary>
        /// Depura el histórico de cuentas procesadas con éxito en base de datos
        /// </summary>
        public static void DepuraHistoricoCuentasOK()
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_EliminaHistoricoCuentasRewardOK");

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("DAOAutorizador.DepuraHistoricoCuentasOK(): " + ex.Message);
            }
        }

        /// <summary>
        /// Consulta el listado de cuentas que no alcanzaron el número de visitas para
        /// cambiar su nivel en base de datos y a las cuales se les reiniciará su saldo
        /// </summary>
        /// <returns>DataSet con los registros</returns>
        public static List<Cuentas> ListaCuentasReset()
        {
            try
            {
                DataSet dsCuentasReset = new DataSet();
                List<Cuentas> response = new List<Cuentas>();

                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ObtieneCuentasReinicioVisitas");

                dsCuentasReset = database.ExecuteDataSet(command);

                if (null != dsCuentasReset)
                {
                    for (int counter = 0; counter < dsCuentasReset.Tables[0].Rows.Count; counter++)
                    {
                        response.Add(new Cuentas(
                            Convert.ToInt32(dsCuentasReset.Tables[0].Rows[counter]["ID_Cuenta"].ToString()),
                            Convert.ToBoolean(dsCuentasReset.Tables[0].Rows[counter]["Reseteada"].ToString())));
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("ListaCuentasReset(): " + ex.Message);
            }
        }

        /// <summary>
        /// Reinicia a ceros  el saldo de la cuenta de visitas y la marca con reinicio
        /// exitoso en base de datos
        /// </summary>
        /// <param name="IdCuenta">Identificador de la cuenta por reiniciar</param>
        public static void ReseteaSaldoCuenta(Int32 IdCuenta)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ReiniciaSaldoCuentaVisitas");

                database.AddInParameter(command, "@IdCuenta", DbType.Int32, IdCuenta);

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("ReseteaSaldoCuenta(): " + ex.Message);
            }
        }

        /// <summary>
        /// Depura el histórico de cuentas procesadas con éxito en base de datos
        /// </summary>
        public static void DepuraHistoricoCuentasResetOK()
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_EliminaHistoricoCuentasResetOK");

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("DAOAutorizador.DepuraHistoricoCuentasResetOK(): " + ex.Message);
            }
        }
    }
}
