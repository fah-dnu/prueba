using CommonProcesador;
using Dnu.Sincronizacion.Correo.DataContracts;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.Sincronizacion.Correo.DataBase
{
    public class DataBaseAccess
    {
        private string _myConnectionString;
        private SqlDatabase _sqlDatabase;
        public DataBaseAccess()
        {
            _myConnectionString = GetConnectionString("DataConnectionString");
            _sqlDatabase = new SqlDatabase(_myConnectionString);
        }
        private static string GetConnectionString(string connectionString)
        {
            string returnValue = null;

            returnValue = PNConfig.Get("CORREOSINCRONIZACION", "DataConnectionString");

            return returnValue;
        }

        public List<Cliente> GetClientesPendientesAutorizar()
        {
            string storedProcName = "spu_ObtenerClientesPendientesAutorizar";
            var clientes = new List<Cliente>();
            using (DbCommand sprocCmd = _sqlDatabase.GetStoredProcCommand(storedProcName))
            {
                sprocCmd.CommandTimeout = 120;
                using (IDataReader sprocReader = _sqlDatabase.ExecuteReader(sprocCmd))
                {
                    while (sprocReader.Read())
                    {
                        var ticket = new Cliente()
                        {
                            Nombre = sprocReader["Nombre"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Nombre"].ToString()
                            ,
                            ApellidoPat = sprocReader["Apellido_pat"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Apellido_pat"].ToString()
                            ,
                            Correo = sprocReader["Correo"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Correo"].ToString()
                            ,
                            FechaNacimiento = sprocReader["fecha_nacimiento"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["fecha_nacimiento"].ToString()
                            ,
                            Calle = sprocReader["calle"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["calle"].ToString()
                            ,
                            NumExterior = sprocReader["num_exterior"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["num_exterior"].ToString()
                            ,
                            NumInterior = sprocReader["num_interior"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["num_interior"].ToString()
                            ,
                            Colonia = sprocReader["Colonia"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Colonia"].ToString()
                            ,
                            Estado = sprocReader["Estado"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Estado"].ToString()
                            ,
                            CodigoPostal = sprocReader["codigo_postal"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["codigo_postal"].ToString()
                            ,
                            Telefono = sprocReader["Telefono"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Telefono"].ToString()
                             ,
                            Puntos = sprocReader["Puntos"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Puntos"].ToString()
                            ,
                            Visitas = sprocReader["Visitas"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Visitas"].ToString()
                            ,
                            Nivel = sprocReader["Nivel"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Nivel"].ToString()
                            ,
                            PuntosVenc = sprocReader["PuntosVencer"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["PuntosVencer"].ToString()
                            ,
                            FechaUltimaCompra = sprocReader["FechaUltimaCompra"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["FechaUltimaCompra"].ToString()
                            ,
                            FechaUltimaBonificacion = sprocReader["FechaUltimaBonificacion"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["FechaUltimaBonificacion"].ToString()


                        };
                        clientes.Add(ticket);
                    }
                }
                return clientes;
            }
        }
        public void ActualizaBonificacionTicket(string correo)
        {
            string storedProcName = "spu_ActualizaClientesPendientesAutorizar";
            using (DbCommand sprocCmd = _sqlDatabase.GetStoredProcCommand(storedProcName))
            {

                _sqlDatabase.AddInParameter(sprocCmd, "correo", DbType.String, correo);
                _sqlDatabase.ExecuteNonQuery(sprocCmd);

            }
        }

    }
}
