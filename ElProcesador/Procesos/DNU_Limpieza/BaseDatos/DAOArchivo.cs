using CommonProcesador;
using DNU_Limpieza.Entidades;
using DNU_Limpieza.Utilidades;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Limpieza.BaseDatos
{
    public class DAOArchivo
    {
        public static List<Archivo> ObtenerArchivosConfigurados()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            List<Archivo> losArchivos = new List<Archivo>();
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneArchivosaLimpiar");

                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);
                LogueoLimpieza.Info("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Proc_ObtieneArchivosaLimpiar]");

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {
                        Archivo unArchivo = new Archivo();

                        unArchivo.IdUrl = Convert.ToInt32(losDatos.Tables[0].Rows[k]["ID_URL"]);
                        unArchivo.URlOrigen = losDatos.Tables[0].Rows[k]["URlOrigen"].ToString();
                        unArchivo.DiasArchivo = Convert.ToInt32(losDatos.Tables[0].Rows[k]["DiasDisponibilidaddeArchivo"]);
                        unArchivo.DiasPapelera = Convert.ToInt32(losDatos.Tables[0].Rows[k]["DiasEnPapelera"]);
                        unArchivo.URLPapelera = losDatos.Tables[0].Rows[k]["URLPapelera"].ToString();
                     
                        losArchivos.Add(unArchivo);
                    }
                }
                
            }
            catch (Exception ex)
            {
                LogueoLimpieza.Error("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                throw new Exception(ex.Message);
            }

            return losArchivos;
        }

    }
}
