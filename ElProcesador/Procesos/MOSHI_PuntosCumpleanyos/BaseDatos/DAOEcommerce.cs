using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MOSHI_PuntosCumpleanyos.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace MOSHI_PuntosCumpleanyos.BaseDatos
{
    /// <summary>
    /// Clase de control de objetos de acceso a datos a Ecommerce
    /// </summary>
    class DAOEcommerce
    {
        /// <summary>
        /// Consulta en base de datos los clientes que cumplen años en la fecha actual
        /// </summary>
        /// <returns>DataSet con los registros</returns>
        public static DataSet ConsultaClientesCumple()
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDEcommerce.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ObtieneClientesCumpleanyos");

                return database.ExecuteDataSet(command);
            }

            catch (Exception ex)
            {
                throw new Exception("ConsultaClientesCumple(): " + ex.Message);
            }
        }
    }
}
