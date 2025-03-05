using CommonProcesador;
using DNU_ProcesadorT112.Entidades;
using DNU_ProcesadorT112.Utilidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DNU_ProcesadorT112.BaseDatos
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
                DataSet losDatos1 = (DataSet)database.ExecuteDataSet(command);



                return losDatos1;
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
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);



                return losDatos2;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public static DataTable ObtieneDatosPorProcesar(Int64 ID_Fichero)
        {



            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneRegistrosParaGenerarPolizas");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                //command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));


                //return database.ExecuteDataSet(command);
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);



                return losDatos2.Tables[0];
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                DAOFichero.GuardarErrorFicheroEnBD(ID_Fichero, " ERROR:" + ex.Message);
                DAOFichero.ActualizaEstatusFichero(ID_Fichero, EstatusFichero.ProcesadoConErrores);
                throw new Exception(ex.Message);
            }
        }

        public static DataTable ObtieneDatosSIArchivoSIBaseDatosParaAplicarMovimientos(Int64 ID_Fichero, String ConsultaWhere)
        {



            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneOperaciones_ArchivoSI_BDSIParaAplicarMovimientos");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));


                //return database.ExecuteDataSet(command);
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);



                return losDatos2.Tables[0];
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public static DataSet ObtieneDatosPara510Bis(Int64 ID_Fichero, String ConsultaWhere)
        {



            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneOperaciones_ArchivoSI_BDSI_510BIS");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));


                //return database.ExecuteDataSet(command);
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);



                return losDatos2;
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
                DataSet losDatos3 = (DataSet)database.ExecuteDataSet(command);



                return losDatos3;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }



        public static List<Movimiento> ObtieneMovimientosPorProcesar(Int64 ID_Fichero)
        {
            List<Movimiento> MisMovimientos = new List<Movimiento>();


            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneRegistrosParaGenerarPolizas");

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                //command.Parameters.Add(new SqlParameter("@ConsultaWhere", ConsultaWhere));


                //return database.ExecuteDataSet(command);
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);



                int regi = 0;

                foreach (DataRow row in losDatos2.Tables[0].Rows)
                {
                    try
                    {
                        regi++;
                        Movimiento dato = new Movimiento();
                        dato.ClaveMA = row["C002"].ToString();
                        dato.IdColectiva = 0;

                        dato.TipoMA = "TAR";
                        dato.ClaveEvento = row["C001"].ToString();
                        dato.Observaciones = "PROCESAMIENTO AUTOMATICO T112";
                        dato.Autorizacion = row["C018"].ToString();
                        dato.FechaOperacion = row["C005"].ToString();

                        dato.ReferenciaNumerica = row["ID_FicheroDetalle"].ToString();
                        dato.Ticket = "";
                        dato.MonedaOriginal = row["C009"].ToString();

                        dato.T112_CodigoMonedaLocal = row["C007"].ToString();
                        dato.T112_ImporteCompensadoLocal = (float.Parse(row["C006"].ToString()) / 100).ToString();

                        dato.T112_CuotaIntercambio = (float.Parse(row["C020"].ToString()) / 100).ToString();

                        if (row["C007"].ToString().Equals("484")) //es Pesos
                        {
                            dato.T112_ImporteCompensadoDolar = "0";
                            dato.T112_ImporteCompensadoPesos = (float.Parse(row["C006"].ToString()) / 100).ToString();

                            dato.Importe = (float.Parse(row["C006"].ToString()) / 100).ToString();
                            dato.ImporteMonedaOriginal = (float.Parse(row["C006"].ToString()) / 100).ToString();
                            dato.MonedaOriginal = row["C007"].ToString();
                        }
                        else
                        {

                            if (row["C009"].ToString().Equals("840")) //es Dolar
                            {
                                dato.T112_ImporteCompensadoDolar = (float.Parse(row["C008"].ToString()) / 100).ToString();
                                dato.T112_ImporteCompensadoPesos = "0";

                            }
                            else if (row["C009"].ToString().Equals("484")) //pesos
                            {
                                dato.T112_ImporteCompensadoDolar = "0";
                                dato.T112_ImporteCompensadoPesos = (float.Parse(row["C008"].ToString()) / 100).ToString();
                            }

                            dato.Importe = (float.Parse(row["C008"].ToString()) / 100).ToString();
                            dato.ImporteMonedaOriginal = (float.Parse(row["C008"].ToString()) / 100).ToString();
                            dato.MonedaOriginal = row["C009"].ToString();
                        }

                        try
                        {
                            dato.T112_IVA = (float.Parse(row["C050"].ToString().Substring(3, 12)) / 100).ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_IVA = "0";
                            Logueo.Error("ObtieneMovimientos(): error al Obtenr el Campo IVA [" + regi + "] Valor en T112: [" + row["C050"].ToString() + "];" + err.Message);

                        }

                        try
                        {
                            dato.T112_FechaPresentacion = row["FechaPresentacion"].ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_FechaPresentacion = DateTime.Now.ToString("yyyy-MM-dd");
                            Logueo.Error("ObtieneMovimientos(): error al Obtenr el Campo Fecha Presentacion" + err.Message);

                        }

                        try
                        {
                            dato.T112_NombreArchivo = row["NombreArchivo"].ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_NombreArchivo = "";
                            Logueo.Error("ObtieneMovimientos(): error al Obtenr el Campo Nombre de Archivo" + err.Message);

                        }

                        MisMovimientos.Add(dato);

                    }
                    catch (Exception err)
                    {
                        Logueo.Error("ObtieneMovimientos(): Formato de Registro no valido [" + regi + "] " + row.ToString() + ";" + err.Message);
                    }
                }

           

                return MisMovimientos;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                DAOFichero.GuardarErrorFicheroEnBD(ID_Fichero, " ERROR:" + ex.Message);
                DAOFichero.ActualizaEstatusFichero(ID_Fichero, EstatusFichero.ProcesadoConErrores);
                throw new Exception(ex.Message);
            }
        }



        public static List<Archivo> ObtienArchivosPendientes()
        {
            List<Archivo> MisArchivos = new List<Archivo>();


            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneArchivosPendientesPorProcesar");

                
                //return database.ExecuteDataSet(command);
                DataSet losDatos2 = (DataSet)database.ExecuteDataSet(command);
                
                int regi = 0;

                foreach (DataRow row in losDatos2.Tables[0].Rows)
                {
                    try
                    {
                        regi++;
                        Archivo unArchivo = new Archivo();

                        unArchivo.ID_Archivo = Int64.Parse(row["ID_Archivo"].ToString());
                        unArchivo.NombreArchivoDetectado = (row["NombreARchivo"].ToString());

                        MisArchivos.Add(unArchivo);

                    }
                    catch (Exception err)
                    {
                        Logueo.Error("ObtienArchivosPendientes(): Formato de Registro no valido [" + regi + "] " + row.ToString() + ";" + err.Message);
                    }
                }



                return MisArchivos;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

    }
}
