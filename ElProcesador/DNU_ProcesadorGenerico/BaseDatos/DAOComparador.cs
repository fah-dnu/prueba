using CommonProcesador;
using DNU_ProcesadorGenerico.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DNU_ProcesadorGenerico.BaseDatos
{
    public class DAOComparador
    {

        public static List<ComparadorConfig> ObtenerConfiguracionFila(Int64 ID_Fichero)
        {

            FilaConfig unCampoConfFila = new FilaConfig();

            List<ComparadorConfig> lasComparaciones = new List<ComparadorConfig>();

            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneConfiguracionDeComparadores");

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
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public static DataSet ObtieneDatosNOArchivoSIBaseDatos(Int64 ID_Fichero, String ConsultaWhere)
        {

           

            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneOperaciones_ArchivoNO_BDSI");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));


                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);



                return losDatos;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public static DataSet ObtieneDatosSIArchivoSIBaseDatos(Int64 ID_Fichero, String ConsultaWhere)
        {



            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneOperaciones_ArchivoSI_BDSI");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));


                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);



                return losDatos;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public static DataSet ObtieneDatosSIArchivoNOBaseDatos(Int64 ID_Fichero, String ConsultaWhere)
        {



            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneOperaciones_ArchivoSI_BDNO");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));


                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);



                return losDatos;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        } 
    }
}
