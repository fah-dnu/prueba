using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MOSHI_PuntosCumpleanyos.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace MOSHI_PuntosCumpleanyos.BaseDatos
{
    /// <summary>
    /// Clase de control de objetos de acceso a datos del Autorizador
    /// </summary>
    class DAOAutorizador
    {
        /// <summary>
        /// Inserta en base de datos la colectiva del cliente a la tabla de control de
        /// los que cumplen años el día de hoy
        /// </summary>
        public static void InsertaClienteTC(Int64 Id_Colectiva)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_InsertaCuentasCumpleanyos");

                database.AddInParameter(command, "@IdColectiva", DbType.Int64, Id_Colectiva);

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("ListaCuentasCumple(): " + ex.Message);
            }
        }

        /// <summary>
        /// Consulta en base de datos el listado de cuentas/clientes que cumplen años el día de hoy
        /// </summary>
        /// <returns>DataSet con los registros</returns>
        public static List<Cuentas> ListaCuentasCumple()
        {
            try
            {
                DataSet dsCuentas = new DataSet();
                List<Cuentas> response = new List<Cuentas>();

                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ObtieneListaCumples");

                dsCuentas = database.ExecuteDataSet(command);

                if (null != dsCuentas)
                {
                    for (int counter = 0; counter < dsCuentas.Tables[0].Rows.Count; counter++)
                    {
                        response.Add(new Cuentas(
                            Convert.ToInt64(dsCuentas.Tables[0].Rows[counter]["ID_Colectiva"].ToString()),
                            dsCuentas.Tables[0].Rows[counter]["Cuentahabiente"].ToString(),
                            dsCuentas.Tables[0].Rows[counter]["Email"].ToString(),
                            dsCuentas.Tables[0].Rows[counter]["Puntos"].ToString(),
                            Convert.ToBoolean(dsCuentas.Tables[0].Rows[counter]["Procesada"].ToString()),
                            Convert.ToBoolean(dsCuentas.Tables[0].Rows[counter]["MailOK"].ToString())));
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("ListaCuentasCumple(): " + ex.Message);
            }
        }

        /// <summary>
        /// Consulta en base de datos la información necesaria para el evento manual
        /// "Bono de Cumpleaños" 
        /// </summary>
        /// <returns>DataSet con los registros</returns>
        public static DataSet ConsultaEventoBonoCumpleanyos()
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ObtieneEventoAbonoPuntos");

                return database.ExecuteDataSet(command);
            }

            catch (Exception ex)
            {
                throw new Exception("ConsultaEventoDepositoRecolectorDiconsa(): " + ex.Message);
            }
        }

        /// <summary>
        /// Marca la cuenta con correo abono de puntos exitoso en base de datos
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva por cambiar</param>
        public static void MarcaCuentaAbonoOK(Int64 IdColectiva)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ActualizaCuentaAbonoOK");

                database.AddInParameter(command, "@IdColectiva", DbType.Int64, IdColectiva);

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("DAOAutorizador.MarcaCuentaAbonoOK(): " + ex.Message);
            }
        }

        /// <summary>
        /// Marca la cuenta con correo enviado exitosamente en base de datos
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva por cambiar</param>
        public static void MarcaCuentaMailOK(Int64 IdColectiva)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ActualizaCuentaCumpleanyosMailEnviado");

                database.AddInParameter(command, "@IdColectiva", DbType.Int64, IdColectiva);

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
                DbCommand command = database.GetStoredProcCommand("ProcNoct_EliminaHistoricoCuentasCumpleanyosOK");

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("DAOAutorizador.DepuraHistoricoCuentasOK(): " + ex.Message);
            }
        }
    }
}
