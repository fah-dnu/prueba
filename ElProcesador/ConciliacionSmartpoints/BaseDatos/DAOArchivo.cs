using CommonProcesador;
using ConciliacionSmartpoints.Entidades;
using ConciliacionSmartpoints.LogicaNegocio;
using ConciliacionSmartpoints.Utilidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.BaseDatos
{
    public class DAOArchivo
    {
        public static List<Archivo> ObtenerArchivosConfigurados()
        {

            List<Archivo> losArchivos = new List<Archivo>();
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneConfigArchivos");

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
                        //unArchivo.ID_ColectivaCCM = (Int64)losDatos.Tables[0].Rows[k]["ID_ColectivaCCM"];
                        unArchivo.ID_TipoProceso = (Int32)losDatos.Tables[0].Rows[k]["ID_TipoProceso"];
                        unArchivo.Nombre = (String)losDatos.Tables[0].Rows[k]["Nombre"];
                        unArchivo.Prefijo = (String)losDatos.Tables[0].Rows[k]["Prefijo"];
                        unArchivo.Sufijo = (String)losDatos.Tables[0].Rows[k]["Sufijo"];
                        unArchivo.FormatoFecha = (String)losDatos.Tables[0].Rows[k]["FormatoFecha"];
                        unArchivo.PosicionFecha = (String)losDatos.Tables[0].Rows[k]["PosicionFecha"].ToString();
                        unArchivo.TipoArchivo = (String)losDatos.Tables[0].Rows[k]["TipoArchivo"].ToString();
                        /*unArchivo.ID_Respuesta = (Int32)losDatos.Tables[0].Rows[k]["ID_Respuesta"];
                        unArchivo.FTPUser = (String)losDatos.Tables[0].Rows[k]["FTPUser"];
                        unArchivo.FTPPass = (String)losDatos.Tables[0].Rows[k]["FTPPass"];
                        unArchivo.FTPUbicacion = (String)losDatos.Tables[0].Rows[k]["FTPUbicacion"];
                        unArchivo.FTPIP = (String)losDatos.Tables[0].Rows[k]["FTPIP"];
                        unArchivo.FTPPuerto = (String)losDatos.Tables[0].Rows[k]["FTPPuerto"].ToString();
                        unArchivo.FTPTipoSeguridad = (String)losDatos.Tables[0].Rows[k]["FTPTipoSeguridad"];*/
                        unArchivo.DistribucionEmail = (String)losDatos.Tables[0].Rows[k]["DistribucionEmail"];
                        /*unArchivo.SoloOK = ((Boolean)losDatos.Tables[0].Rows[k]["SoloOK"]) == true ? "1" : "0";*/
                        if(losDatos.Tables[0].Rows[k]["EID_Header"].ToString() != "")
                        unArchivo.EID_Header = (Int64)losDatos.Tables[0].Rows[k]["EID_Header"];
                        unArchivo.EID_Detail = (Int64)losDatos.Tables[0].Rows[k]["EID_Detail"];
                        //var data = losDatos.Tables[0].Rows[k]["EID_Footer"];

                        long data = 0;
                        Int64.TryParse(losDatos.Tables[0].Rows[k]["EID_Footer"].ToString(), out data);
                        unArchivo.EID_Footer = data;


                        //(Int64?)losDatos.Tables[0].Rows[k]["EID_Footer"];

                        if (losDatos.Tables[0].Rows[k]["EID_Header"].ToString() != "")
                            unArchivo.LID_Header = (Int64)losDatos.Tables[0].Rows[k]["LID_Header"];
                        unArchivo.LID_Detail = (Int64)losDatos.Tables[0].Rows[k]["LID_Detail"];

                        data = 0;
                        Int64.TryParse(losDatos.Tables[0].Rows[k]["LID_Footer"].ToString(), out data);
                        unArchivo.LID_Footer = data;

                        //unArchivo.LID_Footer = (Int64?)losDatos.Tables[0].Rows[k]["LID_Footer"];


                        //unArchivo.LID_DetailExtra = (Int64)losDatos.Tables[0].Rows[k]["LID_DetailExtra"];


                        data = 0;
                        Int64.TryParse(losDatos.Tables[0].Rows[k]["LID_DetailExtra"].ToString(), out data);
                        unArchivo.LID_DetailExtra = data;


                        unArchivo.ClaveProceso = (String)losDatos.Tables[0].Rows[k]["ClaveProceso"];
                        unArchivo.DescripcionProceso = (String)losDatos.Tables[0].Rows[k]["DescripcionProceso"];
                        unArchivo.EventoTipoProceso = (String)losDatos.Tables[0].Rows[k]["EventoTipoProceso"];
                        unArchivo.SeparadorFecha = (String)losDatos.Tables[0].Rows[k]["SeparadorFecha"];
                        unArchivo.SumaDiasFechaOperaciones = (Int32)losDatos.Tables[0].Rows[k]["SumaDiasFechaOperaciones"];

                        unArchivo.ClaveHeader = (String)losDatos.Tables[0].Rows[k]["ClaveHeader"];
                        unArchivo.ClaveRegistro = (String)losDatos.Tables[0].Rows[k]["ClaveRegistro"].ToString();
                        unArchivo.ClaveFooter = (String)losDatos.Tables[0].Rows[k]["ClaveFooter"].ToString();

                        unArchivo.laConfiguracionHeaderLectura = ObtenerConfiguracionFila(unArchivo.LID_Header);
                        unArchivo.laConfiguracionDetalleLectura = ObtenerConfiguracionFila(unArchivo.LID_Detail);
                        if (unArchivo.LID_Footer != null)
                            unArchivo.laConfiguracionFooterLectura = ObtenerConfiguracionFila((Int64)unArchivo.LID_Footer);
                        unArchivo.laConfiguracionDetalleExtraLectura = ObtenerConfiguracionFila(unArchivo.LID_DetailExtra);

                        unArchivo.laConfiguracionHeaderEscritura = ObtenerConfiguracionFila(unArchivo.EID_Header);
                        unArchivo.laConfiguracionDetalleEscritura = ObtenerConfiguracionFila(unArchivo.EID_Detail);
                        if (unArchivo.EID_Footer != null)
                            unArchivo.laConfiguracionFooterEscritura = ObtenerConfiguracionFila((Int64)unArchivo.EID_Footer);

                        losArchivos.Add(unArchivo);

                    }
                }


            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }

            return losArchivos;
        }

      

        public static int ObtieneIdFichero(string filename1)
        {
            int idFichero = 0;
            List<Ficheros> losFicheros = new List<Ficheros>();
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneFicherosId");

                //return database.ExecuteDataSet(command);
                command.Parameters.Add(new SqlParameter("@NombreFichero", filename1));
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {
                        //Ficheros unFichero = new Ficheros();

                        //unFichero.IdFichero = Convert.ToInt32(losDatos.Tables[0].Rows[k]["ID_Fichero"]);

                        idFichero = Convert.ToInt32(losDatos.Tables[0].Rows[k]["ID_Fichero"]);
                        //unFichero.FechaProceso = Convert.ToDateTime(losDatos.Tables[0].Rows[k]["FechaProceso"]);
                        //unFichero.NombreFichero = losDatos.Tables[0].Rows[k]["NombreFichero"].ToString();
                        //unFichero.IdConsulta = (Int64)losDatos.Tables[0].Rows[k]["ID_Consulta"];
                        //unFichero.IdArchivo = (Int64)losDatos.Tables[0].Rows[k]["ID_Archivo"];
                        //unFichero.Estatus = Convert.ToBoolean(losDatos.Tables[0].Rows[k]["Estatus"]);


                        //losFicheros.Add(unFichero);

                    }
                }


            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }

            return idFichero;
        }

        public static FilaConfig ObtenerConfiguracionFila(Int64 ID_Fila)
        {

            FilaConfig unCampoConfFila = new FilaConfig();

            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneConfigFila");

                command.Parameters.Add(new SqlParameter("@ID_Fila", ID_Fila));


                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {

                        //  AgregaFila:

                        //FilaConfig unCampoConfFila = new FilaConfig();

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
                            //if (losDatos.Tables[0].Rows.Count == (i))
                            //{
                            //    break;
                            //}

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
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public static Int64 GuardarFicheroEnBD(Archivo elArchivo, String path, SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            try
            {

                SqlCommand command = //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                new SqlCommand("proc_InsertaFichero", connConsulta);
                //command.CommandText = "proc_InsertaFichero";
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                int resp = -1;

                command.Parameters.Add(new SqlParameter("@NombreFichero", path));
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
                return 0;
            }
        }

        public static bool GuardarFicheroDetallesEnBD(Archivo elArchivo, Int64 ID_Fichero, SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            try
            {
                Boolean laRespu = false;
                //guarda header

                laRespu = GuardarUnDetalle(elArchivo.Header, ID_Fichero, connConsulta, transaccionSQL);

                if (!laRespu)
                {
                    throw new Exception("GuardarUnDetalle(): " + elArchivo.Header.DetalleCrudo + ", ERROR: NO SE PUDO INSERTAR EL HEADER");
                }

                //guarda detalles
                foreach (Fila laFilita in elArchivo.LosDatos)
                {

                    laRespu = GuardarUnDetalle(laFilita, ID_Fichero, connConsulta, transaccionSQL);

                    if (!laRespu)
                    {
                        throw new Exception("GuardarUnDetalle(): " + laFilita.DetalleCrudo + ", ERROR: NO SE PUDO INSERTAR EL DETALLE");
                    }


                }

                //guarda trailer


                laRespu = GuardarUnDetalle(elArchivo.Footer, ID_Fichero, connConsulta, transaccionSQL);

                if (!laRespu)
                {
                    throw new Exception("GuardarUnDetalle(): " + elArchivo.Header.DetalleCrudo + ", ERROR: NO SE PUDO INSERTAR EL TRAILER");
                }


                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("GuardarFicheroDetallesEnBD(): ERROR:" + err.Message);
                return false;
            }
        }

        private static bool GuardarUnDetalle(Fila laFilita, Int64 ID_Fichero, SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            try
            {



                SqlCommand command = new SqlCommand("proc_InsertaFicheroDetalle", connConsulta);  //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                                                                                                  //command.CommandText = "proc_InsertaFicheroDetalle";
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                int resp = -1;

                command.Parameters.Add(new SqlParameter("@DetalleCompleto", laFilita.DetalleCrudo));
                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ID_FilaConfig", laFilita.laConfigDeFila.ID_Fila ));
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
                Logueo.Error("GuardarUnDetalle(): " + laFilita.DetalleCrudo + ", ERROR:" + err.Message);
                return false;
            }
        }

        public static bool GuardarFicheroDetallesEnBD(Archivo elArchivo, Int64 ID_Fichero, String NombreFMTKey, SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            try
            {
                Boolean laRespu = false;
                var timeoutGuardaDetalle = PNConfig.Get("ConciliacionSmart", "TimeOutGuardaDetalle") ?? "240";
                laRespu = GuardarUnDetalle(elArchivo.UrlArchivo, ID_Fichero, NombreFMTKey, connConsulta, transaccionSQL, Convert.ToInt32(timeoutGuardaDetalle));

                if (!laRespu)
                {
                    throw new Exception("GuardarUnDetalle(): " + elArchivo.UrlArchivo + ", ERROR: NO SE PUDO INSERTAR EL DETALLE");
                }


                // }

                //guarda trailer
                //if (elArchivo.Footer != null)
                //{
                //    laRespu = GuardarUnDetalle(elArchivo.Footer, ID_Fichero, connConsulta, transaccionSQL);

                //    if (!laRespu)
                //    {
                //        throw new Exception("GuardarUnDetalle(): " + elArchivo.Header.DetalleCrudo + ", ERROR: NO SE PUDO INSERTAR EL TRAILER");
                //    }
                //}


                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("GuardarFicheroDetallesEnBD(): ERROR:" + err.Message);
                //DAOFichero.GuardarErrorFicheroEnBD(ID_Fichero, " ERROR:" + err.Message);
                //DAOFichero.ActualizaEstatusFichero(ID_Fichero, EstatusFichero.ProcesadoConErrores);
                return false;
            }
        }

        private static bool GuardarUnDetalle(String ElArchivoURL, Int64 ID_Fichero, String FormatoFMTKey,
           SqlConnection connConsulta, SqlTransaction transaccionSQL, int timeoutDetalle)
        {
            try
            {
                String spInsertaFicheroDetalle = LNArchivo.GetInsertaFicheroDetalleBulkSPName();

                Logueo.Evento(String.Format("A llamar " + spInsertaFicheroDetalle + " con {0}", PNConfig.Get("ConciliacionSmart", FormatoFMTKey)));

                SqlCommand command = new SqlCommand(spInsertaFicheroDetalle, connConsulta);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                int resp = -1;

                command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Fichero));
                command.Parameters.Add(new SqlParameter("@ID_FTMConfig", PNConfig.Get("ConciliacionSmart", FormatoFMTKey)));
                command.CommandTimeout = timeoutDetalle;

                resp = command.ExecuteNonQuery();


                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("GuardarUnDetalle(): " + ElArchivoURL + ", ERROR:" + err.Message);
                return false;
            }
        }
    }
}
