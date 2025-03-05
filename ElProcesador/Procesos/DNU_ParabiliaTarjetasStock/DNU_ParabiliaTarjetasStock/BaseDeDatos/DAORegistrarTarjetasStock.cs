
namespace DNU_ParabiliaTarjetasStock.BaseDeDatos
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using Entidades;
    using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
    using BaseDeDatos;
    using CommonProcesador;
    using System.Data.SqlClient;

    public  class DAORegistrarTarjetasStock
    {

        const string CLASE = "DAORegistrarTarjetasStock";

        public static List<ArchivoConfiguracion> ObtenerArchivosConfigurados()
        {
            const string METODO = "ObtenerArchivosConfigurados";

            

            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Obteniendo configuracion de archivos...", CLASE, METODO));

            List<ArchivoConfiguracion> losArchivos = new List<ArchivoConfiguracion>();
            try
            {
                SqlDatabase database = new SqlDatabase(Conexion.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneConfigArchivosSinFilas");

                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {
                        ArchivoConfiguracion unArchivo = new ArchivoConfiguracion();

                        unArchivo.ID_Archivo = (Int64)losDatos.Tables[0].Rows[k]["ID_Archivo"];
                        unArchivo.ID_ConsultaBD = (Int64)losDatos.Tables[0].Rows[k]["ID_ConsultaBD"];
                        unArchivo.ClaveArchivo = (String)losDatos.Tables[0].Rows[k]["ClaveArchivo"];
                        unArchivo.DescripcionArchivo = (String)losDatos.Tables[0].Rows[k]["DescripcionArchivo"];
                        unArchivo.ID_TipoProceso = (Int32)losDatos.Tables[0].Rows[k]["ID_TipoProceso"];
                        unArchivo.Nombre = (String)losDatos.Tables[0].Rows[k]["Nombre"];
                        unArchivo.Prefijo = (String)losDatos.Tables[0].Rows[k]["Prefijo"];
                        unArchivo.Sufijo = (String)losDatos.Tables[0].Rows[k]["Sufijo"];
                        unArchivo.FormatoFecha = (String)losDatos.Tables[0].Rows[k]["FormatoFecha"];
                        unArchivo.PosicionFecha = (String)losDatos.Tables[0].Rows[k]["PosicionFecha"].ToString();
                        unArchivo.TipoArchivo = (String)losDatos.Tables[0].Rows[k]["TipoArchivo"].ToString();




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

        /// <summary>
        /// Valida Existencia de producto 
        /// </summary>
        /// <param name="subproducto"></param>
        /// <returns></returns>
         public static DataTable validaExistenciaSubproducto(string subproducto)
        {
            DataTable retorno = new DataTable();
            string strConexion = Conexion.strBDLecturaAutoCacao;
         
            try
            {


                using (SqlConnection conLocal = new SqlConnection(strConexion))
                {
                    using (SqlCommand command = new SqlCommand("ws_parabilia_verificaSubproducto"))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;


                        //command.Parameters.Add(new SqlParameter("@ClaveColectiva", claveColectiva));
                        command.Parameters.Add(new SqlParameter("@subProducto", subproducto));


                        conLocal.Open();
                        if(conLocal.State == ConnectionState.Open) 
                        {
                            SqlDataAdapter sda = new SqlDataAdapter(command);
                            DataSet opcional = new DataSet();
                            sda.Fill(opcional);
                            retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
                        }
                        else 
                        {
                            return null;
                        }
                        

                    }
                }

            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                retorno = null;
            }
            return retorno;
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="claveColectiva"></param>
        /// <param name="conn"></param>
        /// <returns></returns>

        internal static bool ColectivaValida(string claveColectiva)
        {

            string strConexion = Conexion.strBDLecturaAutoCacao;

            try
            {
                var result = false;
                using (SqlConnection conLocal = new SqlConnection(strConexion))
                {
                    conLocal.Open();
                    using (SqlCommand command = new SqlCommand("proc_ValidaColectiva", conLocal))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        int resp = -1;

                        command.Parameters.Add(new SqlParameter("@ClaveColectiva", claveColectiva));
                        var param = new SqlParameter("@Resultado", result);
                        param.Direction = ParameterDirection.Output;

                        command.Parameters.Add(param);



                        resp = command.ExecuteNonQuery();

                        result = Convert.ToBoolean(command.Parameters["@Resultado"].Value);
                        return result;
                    }

                }
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }

        internal  static bool afectacionTablas(DataTable archivoLeido,string nombreArchivo) 
        {
            string archivoConsecutivo = string.Empty;
            const string METODO = "AfectarTablas";
            SqlConnection conexion;
            DataTable IDS = new DataTable();

            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() Inicia el proceso de afectacion en tablas.", CLASE, METODO));
            try
            {
                
                conexion = Conexion.ConsultaFechaClientesEscritura;

                if (conexion.State == ConnectionState.Open)
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() Conexion Abierta ..", CLASE, METODO));

                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Inicia el Proceso de afectacion", CLASE, METODO));
                    

                        archivoConsecutivo = DAORegistrarTarjetasStock.AfectarArchivoProcesado( conexion, nombreArchivo);
                        if (!String.IsNullOrEmpty(archivoConsecutivo)) 
                        {

                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conexion))
                            {
                                bulkCopy.DestinationTableName =
                                    "dbo.ArchivoDatosGeneral";

                                try
                                {
                                    // Write from the source to the destination.
                                    bulkCopy.WriteToServer(archivoLeido);
                                }
                                catch (Exception ex)
                                {
                                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() SqlBulkCopy  Error al afectar : rollback", CLASE, METODO));
                                    Logueo.Error(string.Format(" [PRPROCESARTARSTOCK].{0}.{1}() {2}", CLASE, METODO, ex.Message));

                                    return false;
                                }

                            }
                               IDS = obtenerIDArchivoDatoGeneral(Convert.ToInt32(archivoLeido.Rows.Count),Convert.ToInt32(archivoConsecutivo), conexion);
                            if (IDS.Rows.Count > 0) 
                            {

                                Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() Se Obtuvieron los IDs consecutivos de menra correcta", CLASE, METODO));

                            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  SQLBulk Archivo-Datos", CLASE, METODO));
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conexion))
                            {
                                bulkCopy.DestinationTableName =
                                    "dbo.ArchivoDatos";

                                try
                                {
                                    // Write from the source to the destination.
                                    bulkCopy.WriteToServer(IDS);
                                }
                                catch (Exception ex)
                                {
                                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() SqlBulkCopy  Error al afectar", CLASE, METODO));
                                    Logueo.Error(string.Format(" [PRPROCESARTARSTOCK].{0}.{1}() {2}", CLASE, METODO, ex.Message));

                                    return false;
                                }
                                return true;
                            }
                         }
                        
                    }
                }
                else
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() Error al abrir la conexion.", CLASE, METODO));

                }
            }
            catch (Exception ex)
            {
                Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() ."+ex.Message, CLASE, METODO));

            }

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        public static DataTable obtenerIDArchivoDatoGeneral(int conteoRegistros, int idArchivo,SqlConnection conexion) 
        {
            const string METODO = "obtenerIDArchivoDatoGeneral";

            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() Afectacion Archivo de registros", CLASE, METODO));
            DataTable retorno = new DataTable();
            try
            {
                SqlCommand command = new SqlCommand("Proc_ObtenerIDS", conexion);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@conteoRegistros", conteoRegistros));
                command.Parameters.Add(new SqlParameter("@idArchivo", idArchivo));

                SqlDataAdapter sda = new SqlDataAdapter(command);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;

                if (retorno.Rows.Count>0)
                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() ListaObtenidaIds", CLASE, METODO));


                else
                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() No se pudo obtener el valor consecutivo de los ids", CLASE, METODO));
                


            }
            catch (Exception ex)
            {
                Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() Error al registar el total de tarjetas : " + ex.Message, CLASE, METODO));
                Logueo.Error(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() Error {2}", CLASE, METODO,ex.Message));

            }
            retorno.DefaultView.Sort = "ID_DATOS ASC";
            return retorno;
        }
        /// <summary>
        /// Metodo que afecta en tabla archivos y en tabla de lectura
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="conexion"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public static string AfectarArchivoProcesado( SqlConnection conexion, string nombre)
        {
            const string METODO = "AfectarArchivoProcesado";

            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}() Afectacion Registro de Archivos", CLASE, METODO));
            DataTable retorno = new DataTable();
            try
            {
                SqlCommand command = new SqlCommand("Proc_InsertArchivo", conexion);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@nombreArchivo", nombre));

                SqlDataAdapter sda = new SqlDataAdapter(command);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;

                if (retorno.Rows[0]["tipo"].ToString() == "00")
                {
                    return retorno.Rows[0]["valor"].ToString();
                }
                else
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() No se pudo Afectar Archivo-Registro ni obtener el consecutivo ", CLASE, METODO));
                }


            }
            catch (Exception ex)
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Error al registar el total de tarjetas : " + ex.Message, CLASE, METODO));
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() Error {2}", CLASE, METODO));

            }
            return string.Empty;
        }



    }


}

