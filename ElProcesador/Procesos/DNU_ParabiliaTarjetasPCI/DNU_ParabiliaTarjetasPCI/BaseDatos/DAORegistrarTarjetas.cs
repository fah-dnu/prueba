// -----------------------------------------------------------------------
// <copyright file="DAORegistrarTarjetas.cs" company="DNU">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DNU_ParabiliaTarjetasPCI.BaseDatos
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using CommonProcesador;
    using DNU_ParabiliaTarjetasPCI.Entidades;
    using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

    public class DAORegistrarTarjetas
    {

        const string CLASE = "DAORegistrarTarjetas";

        /// <summary>
        /// Metodo que afecta tablas con la informacion de los archivos leeidos
        /// </summary>
        /// <param name="dataTarjetas"></param>
        /// <param name="arrayTotalTarjetas"></param>
        /// <param name="idConsecutivo"></param>
        /// <param name="idClientes"></param>
        /// <returns></returns>
        public bool   AfectarTablas(DataTable dataTarjetas,string []arrayTotalTarjetas, string idConsecutivo,DataTable idClientes,DataTable NIPS, bool banderaAerchivo3) 
        {
            const string METODO = "AfectarTablas";
            SqlConnection conexion;
            
            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Inicia el proceso de afectacion en tablas.", CLASE, METODO));
            try
            {
                conexion = Conexiones.ConsultaFechaClientesEscritura;
                if (conexion.State == ConnectionState.Open)
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Se abre contexto de transaccion", CLASE, METODO));
                    using (SqlTransaction tran = conexion.BeginTransaction())
                    {
                        ///Insercion masiva de  tarjetas
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() SqlBulkCopy  TarjetasRegistradas", CLASE, METODO));
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conexion, SqlBulkCopyOptions.Default, tran))
                        {
                            bulkCopy.DestinationTableName =
                                "dbo.ArchivoConversionPrincipal";

                            try
                            {
                                // Write from the source to the destination.
                                bulkCopy.WriteToServer(dataTarjetas);
                            }
                            catch (Exception ex)
                            {
                                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() SqlBulkCopy  Error al afectar : rollback", CLASE, METODO));
                                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() {2}", CLASE, METODO,ex.Message));
                                tran.Rollback();
                                return false;
                            }
                        }

                        if (!registrarTotalTarjetas(tran, conexion, arrayTotalTarjetas, idConsecutivo))
                        {
                            tran.Rollback();
                            return false;
                        }

                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() SqlBulkCopy  NumerosIdClienteTarjeta", CLASE, METODO));

                        if (banderaAerchivo3)
                        {
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conexion, SqlBulkCopyOptions.Default, tran))
                            {
                                bulkCopy.DestinationTableName =
                                    "dbo.ArchivoConversionIdExterno";

                                try
                                {
                                    // Write from the source to the destination.
                                    bulkCopy.WriteToServer(idClientes);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    tran.Rollback();
                                    return false;
                                }
                            }
                        }
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() SqlBulkCopy  NIPRegistrados", CLASE, METODO));

                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conexion, SqlBulkCopyOptions.Default, tran))
                        {
                            bulkCopy.DestinationTableName =
                                "dbo.ArchivoConversionValorDos";

                            try
                            {
                                // Write from the source to the destination.
                                bulkCopy.WriteToServer(NIPS);
                            }
                            catch (Exception ex)
                            {
                                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() SqlBulkCopy  Error al afectar : rollback", CLASE, METODO));
                                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() {2}", CLASE, METODO, ex.Message));
                                tran.Rollback();
                                return false;
                            }
                        }


                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Afectacion tablas  : Commit", CLASE, METODO));

                        tran.Commit();
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Termina contexto de transaccion  : Commit", CLASE, METODO));

                        return true;
                    }

                }
                else
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Error  la conexion esta cerrada", CLASE, METODO));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() {2}", CLASE, METODO, ex.Message));
                return false;
            }


          
         }



        /// <summary>
        /// Metodo que registra la informacion de  el archivo con las tajetas leeidass
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="conexion"></param>
        /// <param name="arrayTotalTarjetas"></param>
        /// <param name="idregistro"></param>
        /// <returns></returns>
         public  bool  registrarTotalTarjetas(SqlTransaction tran , SqlConnection conexion ,string []arrayTotalTarjetas , string idregistro) 
        {
            const string METODO = "registrarTotalTarjetas";

            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Afectacion de tarjetas Totales", CLASE, METODO));
            DataTable retorno = new DataTable();
            try 
            {
                SqlCommand command = new SqlCommand("PCI_InsertarTotalTarjetas", conexion, tran);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ClaveEmisor", arrayTotalTarjetas[0]));
                command.Parameters.Add(new SqlParameter("@TotalTarjetas", arrayTotalTarjetas[1]));
                command.Parameters.Add(new SqlParameter("@IDRegistro", idregistro));

                SqlDataAdapter sda = new SqlDataAdapter(command);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;

                if (retorno.Rows[0]["tipo"].ToString() == "00")
                    return true;
                else
                    return false;

            }
            catch(Exception ex) 
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Error al registar el total de tarjetas : "+ex.Message, CLASE, METODO));
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() Error {2}", CLASE, METODO));
                return false;
            }
            
        }




        /// <summary>
        /// /Metodo que obtiene el  id del regiostro consecutivo
        /// </summary>
        /// <returns></returns>
        public DataTable ObtenerRegistroArchivoConsecutivo() 
        {
            const string METODO = "ObtenerRegistroArchivoConsecutivo";


            SqlConnection conexion;
            conexion = Conexiones.ConsultaFechaClientesLectura;
            DataTable retorno = new DataTable();

            try 
            {
                if(conexion.State == ConnectionState.Open) 
                {

                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() EXEC ObtenerConsecutivoRegistro.", CLASE, METODO));

                    SqlCommand command = new SqlCommand("PCI_ObtenerConsecutivoRegistro", conexion);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sda = new SqlDataAdapter(command);
                    DataSet opcional = new DataSet();
                    sda.Fill(opcional);
                    retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;

                }
               
            }
            catch(Exception Ex) 
            {
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR : {2}.", CLASE, METODO,Ex.Message));
                retorno = null;

            }
            finally  
            {
                conexion.Close();

            }
            return retorno;

        }


        /// <summary>
        /// Metodo que consulta los parametros por clave emisor
        /// </summary>
        /// <returns></returns>
        public DataTable ConsultarParametrosPorClaveEmpresa(string claveEmisor)
        {
            const string METODO = "ConsultarParametrosPorClaveEmpresa";
            SqlConnection conexion;
            conexion = Conexiones.BDLecturaAutoCacao;
            DataTable retorno = new DataTable();
            SqlDataReader reader;

            try
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() EXEC ObtenerDatosContratoPorColectiva.", CLASE, METODO));

                if (conexion.State == ConnectionState.Open)
                {
                    SqlCommand command = new SqlCommand("ObtenerDatosContratoPorColectiva", conexion);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@claveEmisor", claveEmisor);
                    reader = command.ExecuteReader();
                    retorno.Load(reader);

                }
            }
            catch (Exception ex)
            {
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() Error"+ex.Message, CLASE, METODO));

                retorno = null;
             
            }
            finally
            {
                conexion.Close();

            }
            return retorno;
        }

        /// <summary>
        /// Metodo Obtener el medio Acceso determinado
        /// </summary>
        /// <param name="tarjeta"></param>
        /// <param name="tipoMA"></param>
        /// <returns></returns>
        public static string[] ObtenerClabeTarjeta(String tarjeta, String tipoMA)
        {
            const string METODO = "ObtenerClabeTarjeta";
            try
            {
                

                using (SqlConnection conLocal = new SqlConnection(Conexiones.strBDLecturaAutoCacao))
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()   EXEC Procnoc_ObtieneClaveMAFromTarjetaAndTipoClaveMA  : ", CLASE, METODO));

                    using (SqlCommand command = new SqlCommand("Procnoc_ObtieneClaveMAFromTarjetaAndTipoClaveMA"))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;


                        //command.Parameters.Add(new SqlParameter("@ClaveColectiva", claveColectiva));
                        command.Parameters.Add(new SqlParameter("@claveMedioAcceso", tarjeta.Trim()));
                        command.Parameters.Add(new SqlParameter("@ClaveTipoMA", tipoMA));


                        conLocal.Open();
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            return new string[] { "00", reader["ClaveMA"].ToString() };
                        }

                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() No se pudo Obtener el MA  : ", CLASE, METODO));
                        return new string[] { "99", "------------------" };
                    }
                }

            }
            catch (Exception Ex)
            {
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}()  ERROR :  {2} ", CLASE,Ex.Message ));
                return new string[] { "99", Ex.Message };
            }
        }




        /// <summary>
        /// Metodo que obtiene la configuracion del archivo
        /// </summary>
        /// <returns></returns>
        public static List<ArchivoConfiguracion> ObtenerArchivosConfigurados() 
        {
            const string METODO = "ObtenerArchivosConfigurados";
            List<ArchivoConfiguracion> losArchivos = new List<ArchivoConfiguracion>();

            try 
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Ejecutando Proc_ObtieneConfigArchivosSinFilas ...", CLASE, METODO));
                SqlDatabase database = new SqlDatabase(Conexiones.strBDLecturaArchivo);
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

            }catch(Exception ex) 
            {
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}()- Error :Exception {2}", CLASE, METODO,ex.Message));
                losArchivos = null;
            }
            return losArchivos;
        }


        /// <summary>
        /// Metodo Obtener el medio Acceso determinado
        /// </summary>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public static string[] ObtenerTokenByTarjeta(String tarjeta)
        {
            try
            {
                

                using (SqlConnection conLocal = new SqlConnection(Conexiones.strBDLecturaAutoCacao))
                {
                    using (SqlCommand command = new SqlCommand("ObtenerTokenByTarjeta"))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;


                        
                        command.Parameters.Add(new SqlParameter("@numTarjeta", tarjeta.Trim()));
                       
                              

                        conLocal.Open();
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            return new string[] { "00", reader["Token"].ToString() };
                        }

                        Logueo.Error("[ObtenerClabeTarjeta] [No se encontró " + tarjeta+ " para tarjeta: " + tarjeta.Substring(12, 4) + "]");
                        return new string[] { "99", "------------------" };
                    }
                }

            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return new string[] { "99", err.Message };
            }
        }


        /// <summary>
        /// Constructor de clase
        /// </summary>
        public DAORegistrarTarjetas() 
        {


        
        }


    }


   
}
