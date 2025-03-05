using CommonProcesador;
using DNU_ConciliadorCacao.Entidades;
using DNU_ConciliadorCacao.Utilidades;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DNU_ConciliadorCacao.BaseDatos
{
    public class DAOComparador
    {

        public static List<ComparadorConfig> ObtenerConfiguracionFila(Int64 ID_Fichero)
        {

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            FilaConfig unCampoConfFila = new FilaConfig();

            List<ComparadorConfig> lasComparaciones = new List<ComparadorConfig>();

            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneConfiguracionDeComparadores");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_ObtieneConfiguracionDeComparadores " + ID_Fichero + "]");
                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));


                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {

                        ComparadorConfig laConfig = new ComparadorConfig();

                        Campo elBDAT = new Campo();
                        Campo elARCH = new Campo();

                        elBDAT.Nombre = (String)losDatos.Tables[0].Rows[k]["NombreConsulta"];
                        elBDAT.Posicion = (Int32)losDatos.Tables[0].Rows[k]["PosicionConsulta"];
                        laConfig.elCampoBD = elBDAT;

                        laConfig.elComparador = (String)losDatos.Tables[0].Rows[k]["Comparador"];


                        elARCH.Nombre = (String)losDatos.Tables[0].Rows[k]["NombreArchivo"];
                        elARCH.Posicion = (Int32)losDatos.Tables[0].Rows[k]["PosicionArchivo"];

                        laConfig.elCampoArchivo = elARCH;

                        lasComparaciones.Add(laConfig);

                    }
                }

                return lasComparaciones;
            }
            catch (Exception ex)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ObtenerConfiguracionFila {0} - Stack: {1}", ex.Message, ex.StackTrace) + "]");
                throw new Exception(ex.Message);
            }
        }

        public static DataSet ObtieneDatosNOArchivoSIBaseDatos(Int64 ID_Fichero, String ConsultaWhere)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            List<DataRow> rowsTodelete = new List<DataRow>();

            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneOperaciones_ArchivoNO_BDSIParaConciliacionCacaoSISCARD");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_ObtieneOperaciones_ArchivoNO_BDSIParaConciliacionCacaoSISCARD" + ID_Fichero, ConsultaWhere + "]");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));
                command.CommandTimeout = 600;

                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);
                return losDatos;
            }
            catch (Exception ex)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ObtieneDatosNOArchivoSIBaseDatos: {0} - Stack: {1}", ex.Message, ex.StackTrace) + "]");
                 throw new Exception(ex.Message);
            }
        }

        public static DataSet ObtieneDatosSIArchivoSIBaseDatos(Int64 ID_Fichero, String ConsultaWhere)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneOperaciones_ArchivoSI_BDSIParaConciliacionCacaoSISCARD");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_ObtieneOperaciones_ArchivoSI_BDSIParaConciliacionCacaoSISCARD" + ID_Fichero, ConsultaWhere + "]");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));
                command.CommandTimeout = 600;

                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);



                return losDatos;
            }
            catch (Exception ex)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ObtieneDatosSIArchivoSIBaseDatos: {0} - Stack: {1}", ex.Message, ex.StackTrace) + "]");
                 throw new Exception(ex.Message);
            }
        }

        public static DataSet ObtieneDatosSIArchivoNOBaseDatos(Int64 ID_Fichero, String ConsultaWhere)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();


            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneOperaciones_ArchivoSI_BDNOParaConciliacionCacaoSISCARD");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_ObtieneOperaciones_ArchivoSI_BDNOParaConciliacionCacaoSISCARD" + ID_Fichero, ConsultaWhere + "]");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));
                command.CommandTimeout = 600;

                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);



                return losDatos;
            }
            catch (Exception ex)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ObtieneDatosSIArchivoNOBaseDatos: {0} - Stack: {1}", ex.Message, ex.StackTrace) + "]");
                 throw new Exception(ex.Message);
            }
        }
    }
}
