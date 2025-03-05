using Dnu.Batch.DataContracts;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Dnu.Batch.DataBase
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
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionString];

            if (settings != null)
                returnValue = settings.ConnectionString;

            return returnValue;
        }

        public List<Ticket> GetTicketsByStatus(int status)
        {
            var respuesta = new Respuesta();
            string storedProcName = "spu_ObtenerTicketsPorEstatus";
            var tickets = new List<Ticket>();
            using (DbCommand sprocCmd = _sqlDatabase.GetStoredProcCommand(storedProcName))
            {
                _sqlDatabase.AddInParameter(sprocCmd, "@IdEstatusTicketCod", DbType.Int16, status);
                using (IDataReader sprocReader = _sqlDatabase.ExecuteReader(sprocCmd))
                {
                    while (sprocReader.Read())
                    {
                        var ticket = new Ticket()
                        {
                            IdTicketSucursal = sprocReader["IdTicketSucursal"].GetType() == typeof(DBNull) ? 0 : int.Parse(sprocReader["IdTicketSucursal"].ToString())
                            ,
                            IdTipoAcumulacion = sprocReader["IdTipoAcumulacion"].GetType() == typeof(DBNull) ? 0 : int.Parse(sprocReader["IdTipoAcumulacion"].ToString())
                            ,
                            TipoEntrega = sprocReader["TipoEntrega"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["TipoEntrega"].ToString()
                            ,
                            ClaveSucursal = sprocReader["ClaveSucursal"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["ClaveSucursal"].ToString()
                            ,
                            NumeroTicket = sprocReader["NumeroTicket"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["NumeroTicket"].ToString()
                            ,
                            FechaTicket = sprocReader["FechaTicket"].GetType() == typeof(DBNull) ? DateTime.MinValue : DateTime.Parse(sprocReader["FechaTicket"].ToString())
                            ,
                            Importe = sprocReader["Importe"].GetType() == typeof(DBNull) ? 0 : decimal.Parse(sprocReader["Importe"].ToString())
                            ,
                            Descuentos = sprocReader["Descuentos"].GetType() == typeof(DBNull) ? 0 : decimal.Parse(sprocReader["Descuentos"].ToString())
                            ,
                            Propina = sprocReader["Propina"].GetType() == typeof(DBNull) ? 0 : decimal.Parse(sprocReader["Propina"].ToString())
                            ,
                            CodigoUnicoFactura = sprocReader["CodigoUnicoFactura"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["CodigoUnicoFactura"].ToString()
                            ,
                            cliente_id = sprocReader["cliente_id"].GetType() == typeof(DBNull) ? 0 : int.Parse(sprocReader["cliente_id"].ToString())
                            ,
                            Email = sprocReader["Email"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["Email"].ToString()
                            ,
                            IdEstatusTicket = sprocReader["IdEstatusTicket"].GetType() == typeof(DBNull) ? 0 : int.Parse(sprocReader["IdEstatusTicket"].ToString())
                            ,
                            FechaInsert = sprocReader["FechaInsert"].GetType() == typeof(DBNull) ? DateTime.MinValue : DateTime.Parse(sprocReader["FechaInsert"].ToString())
                            ,
                            UsuarioInsert = sprocReader["UsuarioInsert"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["UsuarioInsert"].ToString()
                            ,
                            FechaModifico = sprocReader["FechaModifico"].GetType() == typeof(DBNull) ? DateTime.MinValue : DateTime.Parse(sprocReader["FechaModifico"].ToString())
                            ,
                            UsuarioModifico = sprocReader["UsuarioModifico"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["UsuarioModifico"].ToString()
                            ,
                            NumeroReintento = sprocReader["NumeroReintento"].GetType() == typeof(DBNull) ? 0 : int.Parse(sprocReader["NumeroReintento"].ToString())
                            ,
                            NumeroAutorizacion = sprocReader["NumeroAutorizacion"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["NumeroAutorizacion"].ToString()
                            ,
                            FechaBonificacion = sprocReader["FechaBonificacion"].GetType() == typeof(DBNull) ? DateTime.MinValue : DateTime.Parse(sprocReader["FechaBonificacion"].ToString())
                            ,
                            Afiliacion = sprocReader["afiliacion"].GetType() == typeof(DBNull) ? string.Empty : sprocReader["afiliacion"].ToString()

                        };
                        tickets.Add(ticket);
                    }
                }
                return tickets;
            }
        }

        public void RegistrarBonificacionTicket(int IdTicketSucursal, string numeroTicket, string codigoRespuesta, string autorizacion, int numeroMaxIntentos)
        {
            string storedProcName = "spu_RegistraRespuestaBonificacion";
            using (DbCommand sprocCmd = _sqlDatabase.GetStoredProcCommand(storedProcName))
            {
                
                _sqlDatabase.AddInParameter(sprocCmd, "IdTicketSucursal", DbType.Int32, IdTicketSucursal);
                _sqlDatabase.AddInParameter(sprocCmd, "NumeroTicket", DbType.String, numeroTicket);
                _sqlDatabase.AddInParameter(sprocCmd, "CodigoRespuesta", DbType.String, codigoRespuesta);
                _sqlDatabase.AddInParameter(sprocCmd, "NumeroAutorizacion", DbType.String, autorizacion);

                _sqlDatabase.AddInParameter(sprocCmd, "NumeroMaxIntentos", DbType.Int16, numeroMaxIntentos);
                
                _sqlDatabase.ExecuteNonQuery(sprocCmd);
               
            }
        }
        public string ObtenerInfoMail(string email)
        {

            string storedProcName = "spu_ObtenerInfoMail";
            using (DbCommand sprocCmd = _sqlDatabase.GetStoredProcCommand(storedProcName))
            {
                _sqlDatabase.AddInParameter(sprocCmd, "Email", DbType.String, email);
                
                _sqlDatabase.AddOutParameter(sprocCmd, "Nombre", DbType.String, 150);
                _sqlDatabase.ExecuteNonQuery(sprocCmd);

                return _sqlDatabase.GetParameterValue(sprocCmd, "@Nombre").ToString();
            }

        }
    }
}
