using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MOSHI_PuntosVencidos.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace MOSHI_PuntosVencidos.BaseDatos
{
    /// <summary>
    /// Clase de control de objetos de acceso a datos del Autorizador
    /// </summary>
    class DAOAutorizador
    {
        /// <summary>
        /// Consulta el listado de cuentas que tienen puntos por vencer en base de datos
        /// </summary>
        /// <returns>DataSet con los registros</returns>
        public static List<Cuentas> ListaCuentasConPuntosVencidos()
        {
            try
            {
                DataSet dsCuentas = new DataSet();
                List<Cuentas> response = new List<Cuentas>();

                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ObtieneCuentasPuntosVencidos");

                dsCuentas = database.ExecuteDataSet(command);

                if (null != dsCuentas)
                {
                    for (int counter = 0; counter < dsCuentas.Tables[0].Rows.Count; counter++)
                    {
                        response.Add(new Cuentas(
                            Convert.ToInt64(dsCuentas.Tables[0].Rows[counter]["ID_Cuenta"].ToString()),
                            Convert.ToInt64(dsCuentas.Tables[0].Rows[counter]["ID_Colectiva"].ToString()),
                            int.Parse(dsCuentas.Tables[0].Rows[counter]["ID_TipoColectiva"].ToString()),
                            dsCuentas.Tables[0].Rows[counter]["Email"].ToString(),
                            float.Parse(dsCuentas.Tables[0].Rows[counter]["PuntosAVencer"].ToString()),
                            Convert.ToBoolean(dsCuentas.Tables[0].Rows[counter]["ProcesadaOK"].ToString())));
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("ListaCuentasConPuntosVencidos(): " + ex.Message);
            }
        }

        /// <summary>
        /// Consulta en base de datos los datos básicos del evento "Puntos Vencidos" 
        /// </summary>
        /// <param name="elUsuario">Usuario en sesión</param>
        /// <param name="AppID">Identificador de la aplicación</param>
        /// <returns>DataSet con los registros</returns>
        public static DataSet ConsultaEventoPuntosVencidos()
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ObtieneEventoPuntosVencidos");

                return database.ExecuteDataSet(command);
            }
            catch (Exception ex)
            {
                throw new Exception("ConsultaEventoPuntosVencidos(): " + ex.Message);
            }
        }

        /// <summary>
        /// Marca la cuenta como procesada (puntos vencidos) exitosamente en base de datos
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        /// <param name="IdCuenta">Identificador de la cuenta</param>
        public static void MarcaCuentaVencimientoPtsOK(Int64 IdColectiva, Int64 IdCuenta)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ActualizaCuentaPtsVencidosOK");

                database.AddInParameter(command, "@IdCuenta", DbType.Int64, IdCuenta);
                database.AddInParameter(command, "@IdColectiva", DbType.Int64, IdColectiva);

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("DAOAutorizador.MarcaCuentaProcesadaOK(): " + ex.Message);
            }
        }
    }
}
