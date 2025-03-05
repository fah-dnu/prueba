using CommonProcesador;
using Dnu_ProcesadorCaCaoLog.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;


namespace Dnu_ProcesadorCaCaoLog.BaseDatos
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
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }

            return losArchivos;
        }


        public static FilaConfig ObtenerConfiguracionFila(Int64 ID_Fila)
        {

            FilaConfig unCampoConfFila = new FilaConfig();

            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("proc_ObtieneConfigFila");

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
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }


        public static Int64 GuardarFicheroEnBD(Archivo elArchivo, SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            try
            {

                SqlCommand command = //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                new SqlCommand("proc_InsertaFichero", connConsulta);  
                //command.CommandText = "proc_InsertaFichero";
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                int resp = -1;

                command.Parameters.Add(new SqlParameter("@NombreFichero", elArchivo.Nombre));
                command.Parameters.Add(new SqlParameter("@ID_Archivo", elArchivo.ID_Archivo));
                command.Parameters.Add(new SqlParameter("@ID_Consulta", elArchivo.ID_ConsultaBD));

                SqlParameter  param = new SqlParameter("@ID_Fichero", SqlDbType.BigInt);
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

    }
}
