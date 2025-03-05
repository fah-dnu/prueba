using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProcesadorNocturno.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;
using System.Data;
using ProcesadorNocturno;
using System.Data.SqlClient;
using CommonProcesador;
using CommonProcesador.BaseDatos;
using System.Configuration;
using log4net;

namespace ProcesadorNocturno.BaseDatos
{
    public class DAOProcesos
    {
        public static List<Proceso> ObtieneProcesosAEjecutar()
        {
            List<Proceso> losProcesos = new List<Proceso>();
            try
            {
                string direccionIP = System.Net.Dns.GetHostName();
                Guid idLog = Guid.NewGuid();
                ThreadContext.Properties["log"] = idLog;
                ThreadContext.Properties["ip"] = direccionIP;

                string log = ThreadContext.Properties["log"].ToString();
                string ip = ThreadContext.Properties["ip"].ToString();
               



                SqlDatabase database = new SqlDatabase(DBProceso.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneProcesos");
                database.AddInParameter(command, "@Instancia", DbType.String, ConfigurationManager.AppSettings["Instancia"].ToString());
                DataTable unaTabla = database.ExecuteDataSet(command).Tables[0];

                int proceso = 0;

                if (unaTabla.Rows.Count != 0)
                {
                    foreach (DataRow Registro in unaTabla.Rows)
                    {
                        Proceso unProceso = new Proceso();

                        unProceso.ID_Proceso = int.Parse(unaTabla.Rows[proceso]["ID_Proceso"].ToString());
                        unProceso.ID_Ejecucion = int.Parse(unaTabla.Rows[proceso]["ID_Ejecucion"].ToString());
                        unProceso.Nombre = unaTabla.Rows[proceso]["Nombre"].ToString();
                        unProceso.Descripcion = unaTabla.Rows[proceso]["Descripcion"].ToString();
                        unProceso.Ensamblado = unaTabla.Rows[proceso]["ProcAssembly"].ToString();
                        unProceso.Clase = unaTabla.Rows[proceso]["ProClase"].ToString();
                        unProceso.Clave = unaTabla.Rows[proceso]["Clave"].ToString();
                        unProceso.Descripcion = unaTabla.Rows[proceso]["Descripcion"].ToString();
                        unProceso.Dom = Boolean.Parse(unaTabla.Rows[proceso]["Dom"].ToString());
                        unProceso.Lun = Boolean.Parse(unaTabla.Rows[proceso]["Lun"].ToString());
                        unProceso.Mar = Boolean.Parse(unaTabla.Rows[proceso]["Mar"].ToString());
                        unProceso.Mie = Boolean.Parse(unaTabla.Rows[proceso]["Mie"].ToString());
                        unProceso.Jue = Boolean.Parse(unaTabla.Rows[proceso]["Jue"].ToString());
                        unProceso.Vie = Boolean.Parse(unaTabla.Rows[proceso]["Vie"].ToString());
                        unProceso.Sab = Boolean.Parse(unaTabla.Rows[proceso]["Sab"].ToString());

                        losProcesos.Add(unProceso);
                    }
                }
                return losProcesos;
            }
            catch (Exception ex)
            {
                AppLogger.Error("[Procesador] [PROCESADORNOCTURNO] [ObtieneProcesosAEjecutar] [" + ex.Message + "]");
                // throw ex;
                return losProcesos;
            }
        }

        public static int ActualizarEstatusEjecucion(Proceso unProceso, enumEstatusEjecucion elEstatus)
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                using (SqlConnection laConn = DBProceso.BDEscritura)
                {
                    DbCommand command = laConn.CreateCommand();
                    command.CommandText = "Proc_ActualizaEjecucion";
                    AppLogger.Info("[" + ip + "] [Procesador] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Proc_ActualizaEjecucion " + unProceso.ID_Ejecucion + ": " + elEstatus + "]");

                    command.CommandType = CommandType.StoredProcedure;


                    command.Parameters.Add(new SqlParameter("@ID_Ejecucion", unProceso.ID_Ejecucion));
                    command.Parameters.Add(new SqlParameter("@ID_Estatus", (int)elEstatus));


                    command.ExecuteNonQuery();
                }

                return 0;

            }
            catch (Exception ex)
            {

                AppLogger.Error("[" + ip + "] [Procesador] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                throw new Exception(ex.Message);
            }
        }

        public static List<Proceso> ObtieneTodosProcesos()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                List<Proceso> losProcesos = new List<Proceso>();
                SqlDatabase database = new SqlDatabase(DBProceso.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneTodosProcesos");
                //database.AddInParameter(command, "@ID_Cita", DbType.Int32, ID_Cita);
                database.AddInParameter(command, "@Instancia", DbType.String, ConfigurationManager.AppSettings["Instancia"].ToString());
                DataTable unaTabla = database.ExecuteDataSet(command).Tables[0];

                if (unaTabla.Rows.Count != 0)
                {
                    int fila = 0;
                    foreach (DataRow Registro in unaTabla.Rows)
                    {
                        Proceso unProceso = new Proceso();

                        unProceso.ID_Proceso = int.Parse(unaTabla.Rows[fila]["ID_Proceso"].ToString());
                        unProceso.ID_Ejecucion = int.Parse(unaTabla.Rows[fila]["ID_Ejecucion"].ToString());
                        unProceso.Nombre = unaTabla.Rows[fila]["Nombre"].ToString();
                        unProceso.Descripcion = unaTabla.Rows[fila]["Descripcion"].ToString();
                        unProceso.Ensamblado = unaTabla.Rows[fila]["ProcAssembly"].ToString();
                        unProceso.Clase = unaTabla.Rows[fila]["ProClase"].ToString();
                        unProceso.Clave = unaTabla.Rows[fila]["Clave"].ToString();
                        unProceso.Descripcion = unaTabla.Rows[fila]["Descripcion"].ToString();
                        unProceso.Dom = Boolean.Parse(unaTabla.Rows[fila]["Dom"].ToString());
                        unProceso.Lun = Boolean.Parse(unaTabla.Rows[fila]["Lun"].ToString());
                        unProceso.Mar = Boolean.Parse(unaTabla.Rows[fila]["Mar"].ToString());
                        unProceso.Mie = Boolean.Parse(unaTabla.Rows[fila]["Mie"].ToString());
                        unProceso.Jue = Boolean.Parse(unaTabla.Rows[fila]["Jue"].ToString());
                        unProceso.Vie = Boolean.Parse(unaTabla.Rows[fila]["Vie"].ToString());
                        unProceso.Sab = Boolean.Parse(unaTabla.Rows[fila]["Sab"].ToString());
                        fila++;

                        losProcesos.Add(unProceso);
                    }
                }
                return losProcesos;
            }
            catch (Exception ex)
            {
                AppLogger.Error("[" + ip + "] [Procesador] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                throw ex;
            }
        }

    }
}
