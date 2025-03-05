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
using System.Globalization;
using System.Linq;
using System.Text;


namespace DNU_ConciliadorCacao.BaseDatos
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
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneConfigArchivos");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Proc_ObtieneConfigArchivos]");

                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {
                        Archivo unArchivo = new Archivo();

                        unArchivo.ID_Archivo = (Int64)losDatos.Tables[0].Rows[k]["ID_Archivo"];
                        unArchivo.ID_ConsultaBD = (Int64)losDatos.Tables[0].Rows[k]["ID_ConsultaBD"];
                        unArchivo.ClaveArchivo = (String)losDatos.Tables[0].Rows[k]["ClaveArchivo"];
                        unArchivo.DescripcionArchivo = (String)losDatos.Tables[0].Rows[k]["DescripcionArchivo"];
                        unArchivo.ID_CodificacionRead = (Int32)losDatos.Tables[0].Rows[k]["ID_CodificacionRead"];
                        unArchivo.ID_CodificacionWrite = (Int32)losDatos.Tables[0].Rows[k]["ID_CodificacionWrite"];
                        unArchivo.URLEscritura = (String)losDatos.Tables[0].Rows[k]["URLEscritura"];
                        unArchivo.ID_TipoProceso = (Int32)losDatos.Tables[0].Rows[k]["ID_TipoProceso"];
                        unArchivo.Nombre = (String)losDatos.Tables[0].Rows[k]["Nombre"];
                        unArchivo.Prefijo = (String)losDatos.Tables[0].Rows[k]["Prefijo"];
                        unArchivo.Sufijo = (String)losDatos.Tables[0].Rows[k]["Sufijo"];
                        unArchivo.FormatoFecha = (String)losDatos.Tables[0].Rows[k]["FormatoFecha"];
                        unArchivo.PosicionFecha = (String)losDatos.Tables[0].Rows[k]["PosicionFecha"].ToString();
                        unArchivo.TipoArchivo = (String)losDatos.Tables[0].Rows[k]["TipoArchivo"].ToString();

                        unArchivo.DistribucionEmail = (String)losDatos.Tables[0].Rows[k]["DistribucionEmail"];
                        unArchivo.EID_Detail = (Int64)losDatos.Tables[0].Rows[k]["EID_Detail"];

                        unArchivo.LID_Detail = (Int64)losDatos.Tables[0].Rows[k]["LID_Detail"];

                        unArchivo.ClaveProceso = (String)losDatos.Tables[0].Rows[k]["ClaveProceso"];
                        unArchivo.DescripcionProceso = (String)losDatos.Tables[0].Rows[k]["DescripcionProceso"];
                        unArchivo.EventoTipoProceso = (String)losDatos.Tables[0].Rows[k]["EventoTipoProceso"];
                        unArchivo.SeparadorFecha = (String)losDatos.Tables[0].Rows[k]["SeparadorFecha"];
                        unArchivo.SumaDiasFechaOperaciones = (Int32)losDatos.Tables[0].Rows[k]["SumaDiasFechaOperaciones"];

                        unArchivo.ClaveHeader = (String)losDatos.Tables[0].Rows[k]["ClaveHeader"];
                        unArchivo.ClaveRegistro = (String)losDatos.Tables[0].Rows[k]["ClaveRegistro"].ToString();
                        unArchivo.ClaveFooter = (String)losDatos.Tables[0].Rows[k]["ClaveFooter"].ToString();

                        unArchivo.laConfiguracionDetalleLectura = ObtenerConfiguracionFila(unArchivo.LID_Detail);


                        unArchivo.laConfiguracionDetalleEscritura = ObtenerConfiguracionFila(unArchivo.EID_Detail);


                        losArchivos.Add(unArchivo);

                    }
                }


            }
            catch (Exception ex)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [ObtenerArchivosConfigurados] {0} - Stack : {1}" + ex.Message, ex.StackTrace + "]");
                 throw new Exception(ex.Message);
            }

            return losArchivos;
        }

        internal static DataTable GeneraDataTableDetalle(Archivo elArchivo, long iD_Fichero)
        {
            DataTable dt = new DataTable("Detalle");
            dt.Columns.Add(new DataColumn("ID_FicheroDetalle"));
            dt.Columns.Add(new DataColumn("ID_Fichero"));
            dt.Columns.Add(new DataColumn("ID_Fila"));
            dt.Columns.Add(new DataColumn("DetalleCompleto"));
            dt.Columns.Add(new DataColumn("C001"));
            dt.Columns.Add(new DataColumn("C002"));
            dt.Columns.Add(new DataColumn("C003"));
            dt.Columns.Add(new DataColumn("C004"));
            dt.Columns.Add(new DataColumn("C005"));
            dt.Columns.Add(new DataColumn("C006"));
            dt.Columns.Add(new DataColumn("C007"));
            dt.Columns.Add(new DataColumn("C008"));
            dt.Columns.Add(new DataColumn("C009"));
            dt.Columns.Add(new DataColumn("C010"));
            dt.Columns.Add(new DataColumn("C011"));
            dt.Columns.Add(new DataColumn("C012"));
            dt.Columns.Add(new DataColumn("C013"));
            dt.Columns.Add(new DataColumn("C014"));
            dt.Columns.Add(new DataColumn("C015"));
            dt.Columns.Add(new DataColumn("C016"));

            foreach (Fila laFilita in elArchivo.LosDatos)
            {
                DataRow row = dt.NewRow();
                if (String.IsNullOrEmpty(laFilita.DetalleCrudo))
                    continue;

                row["ID_FicheroDetalle"] = null;
                row["ID_Fichero"] = iD_Fichero;
                row["ID_Fila"] = laFilita.laConfigDeFila.ID_Fila;
                row["DetalleCompleto"] = laFilita.DetalleCrudo;
                row["C001"] = laFilita.losCampos[1];
                row["C002"] = laFilita.losCampos[2];
                row["C003"] = laFilita.losCampos[3];
                row["C004"] = laFilita.losCampos[4];
                row["C005"] = laFilita.losCampos[5];
                row["C006"] = laFilita.losCampos[6];
                row["C007"] = laFilita.losCampos[7];
                row["C008"] = laFilita.losCampos[8];
                row["C009"] = laFilita.losCampos[9];
                row["C010"] = laFilita.losCampos[10];
                row["C011"] = laFilita.losCampos[11];
                row["C012"] = laFilita.losCampos[12];
                row["C013"] = laFilita.losCampos[13];
                row["C014"] = laFilita.losCampos[14];
                row["C015"] = laFilita.losCampos[15];
                row["C016"] = laFilita.losCampos[16];

                dt.Rows.Add(row);
            }
            return dt;
        }

        public static FilaConfig ObtenerConfiguracionFila(Int64 ID_Fila)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            FilaConfig unCampoConfFila = new FilaConfig();

            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneConfigFila");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_ObtieneConfigFila " + ID_Fila + "]");

                command.Parameters.Add(new SqlParameter("@ID_Fila", ID_Fila));


                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {

                        //  AgregaFila:


                        unCampoConfFila.ID_Fila = (Int64)losDatos.Tables[0].Rows[k]["ID_Fila"];
                        unCampoConfFila.ClaveFila = (String)losDatos.Tables[0].Rows[k]["ClaveFila"];
                        unCampoConfFila.DescripcionFila = (String)losDatos.Tables[0].Rows[k]["DescripcionFila"];
                        unCampoConfFila.PorSeparador = (Boolean)losDatos.Tables[0].Rows[k]["PorSeparador"];
                        unCampoConfFila.CaracterSeparacion = (String)losDatos.Tables[0].Rows[k]["CaracterSeparacion"];
                        unCampoConfFila.PorLongitud = (Boolean)losDatos.Tables[0].Rows[k]["PorLongitud"];
                        unCampoConfFila.ID_CampoFila = (Int64)losDatos.Tables[0].Rows[k]["ID_CampoFila"];
                        unCampoConfFila.IsPadding = (Boolean)losDatos.Tables[0].Rows[k]["IsPadding"];



                        for (int i = 0; unCampoConfFila.ID_Fila == (Int64)losDatos.Tables[0].Rows[k]["ID_Fila"]; i++)
                        {

                            CampoConfig unaConfiguracion = new CampoConfig();

                            unaConfiguracion.Descripcion = (String)losDatos.Tables[0].Rows[k]["FilaCampoDescripcion"];
                            unaConfiguracion.EsClave = (Boolean)losDatos.Tables[0].Rows[k]["EsClave"];
                            unaConfiguracion.ID_TipoColectiva = (Int32)losDatos.Tables[0].Rows[k]["ID_TipoColectiva"];
                            unaConfiguracion.Nombre = (String)losDatos.Tables[0].Rows[k]["Nombre"];
                            //unaConfiguracion.TipoDatoSQL = (String)losDatos.Tables[0].Rows[k]["TipoDatoSQL"];
                            unaConfiguracion.TrimCaracteres = (String)losDatos.Tables[0].Rows[k]["TrimCaracteres"];
                            unaConfiguracion.PosicionInicial = (Int32)losDatos.Tables[0].Rows[k]["PosicionInicial"];
                            unaConfiguracion.PosicionFinal = (Int32)losDatos.Tables[0].Rows[k]["PosicionFinal"];
                            unaConfiguracion.Posicion = (Int32)losDatos.Tables[0].Rows[k]["Posicion"];
                            unaConfiguracion.Longitud = (Int32)losDatos.Tables[0].Rows[k]["Longitud"];

                            unCampoConfFila.LosCampos.Add(unaConfiguracion.Posicion, unaConfiguracion);


                            k++;

                            if (losDatos.Tables[0].Rows.Count == (k))
                            {
                                break;
                            }
                        }



                        //goto AgregaFila;
                    }
                }

                return unCampoConfFila;
            }
            catch (Exception ex)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[ObtenerConfiguracionFila] {0} - Stack: {1}", ex.Message, ex.StackTrace) + "]");
                 throw new Exception(ex.Message);
            }
        }


        public static Int64 GuardarFicheroEnBD(Archivo elArchivo, SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {

                SqlCommand command = //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                new SqlCommand("proc_InsertaFichero", connConsulta);
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_InsertaFichero]");
                //command.CommandText = "proc_InsertaFichero";
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                int resp = -1;

                command.Parameters.Add(new SqlParameter("@NombreFichero", elArchivo.Nombre));
                command.Parameters.Add(new SqlParameter("@ID_Archivo", elArchivo.ID_Archivo));
                command.Parameters.Add(new SqlParameter("@ID_Consulta", elArchivo.ID_ConsultaBD));

                SqlParameter param = new SqlParameter("@ID_Fichero", SqlDbType.BigInt);
                param.Value = 0;
                param.Direction = ParameterDirection.Output;
                command.Parameters.Add(param);


                resp = command.ExecuteNonQuery();

                Int64 elID = Convert.ToInt64(command.Parameters["@ID_Fichero"].Value);

                return elID;

            }
            catch (Exception err)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return 0;
            }
        }

        private static bool GuardarUnDetalle(Fila laFilita, Int64 ID_Fichero, SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {

                SqlCommand command = new SqlCommand("proc_InsertaFicheroDetalle", connConsulta);  //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                                                                                                  //command.CommandText = "proc_InsertaFicheroDetalle";
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_InsertaFicheroDetalle" + ID_Fichero + "]");
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                int resp = -1;

                command.Parameters.Add(new SqlParameter("@DetalleCompleto", laFilita.DetalleCrudo));
                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ID_FilaConfig", laFilita.laConfigDeFila.ID_Fila));
                //command.Parameters.Add(new SqlParameter("@ID_Fichero" , SqlDbType.BigInt,));

                for (int laPosicion = 1; laPosicion <= 80; laPosicion++)
                {

                    if (laFilita.losCampos.ContainsKey(laPosicion))
                    {
                        command.Parameters.Add(new SqlParameter("@C" + laPosicion.ToString().PadLeft(3, '0'), laFilita.losCampos[laPosicion]));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@C" + laPosicion.ToString().PadLeft(3, '0'), ""));
                    }

                }

                resp = command.ExecuteNonQuery();


                return true;

            }
            catch (Exception err)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [GuardarUnDetalle(): " + laFilita.DetalleCrudo + ", ERROR:" + err.Message + "]");
                return false;
            }
        }


        public static bool GuardarFicheroDetallesEnBD(DataTable dt)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    bulkCopy.DestinationTableName =
                        "dbo.FicheroDetalle";

                    try
                    {
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(dt);
                    }
                    catch (Exception ex)
                    {
                        LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
                        Console.WriteLine(ex.Message);
                    }
                }

                return true;

            }
            catch (Exception err)
            {  LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [GuardarFicheroDetallesEnBD(): ERROR:" + err.Message + "]");
                return false;
            }
        }

    }
}
