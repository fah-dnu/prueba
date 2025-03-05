using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Utilidades;
using Executer.Entidades;
using Interfases.Entidades;
using Interfases.Enums;
using Interfases.Exceptions;

namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    /// <summary>
    /// contiene las definiciones para la contencion y ejecucion de variantes
    /// </summary>
    /// <creation>Cruz Mejia Raul - 07/10/2022</creation>
    public class DAOVariante
    {
        const string CLASE = "DAOVariante";



        /// <summary>
        /// Sobrecarga de actualizar operacion para variante
        /// </summary>
        /// <param name="ID_OperacionOriginal"></param>
        /// <param name="ID_EstatusOperacion"></param>
        /// <param name="ID_EstatusPostOperacion"></param>
        /// <param name="codigoProceso"></param>
        /// <param name="cveEstatusPostOperacionDestino"></param>
        /// <param name="connConsulta"></param>
        /// <param name="transaccionSQL"></param>
        /// <returns></returns>
        public static int ActualizaEstatusOperacionOriginal(
           long ID_OperacionOriginal,
           long ID_EstatusOperacion,
           long ID_EstatusPostOperacion,
            string cveEstatusPostOperacionDestino,
           string codigoProceso,
           SqlConnection connConsulta,
           SqlTransaction transaccionSQL)
        {



            // CallableStatement spEjecutor = null;
            //int IdColectiva = -1;

            try
            {

                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("EJECUTOR_ActualizaEstatusOperacionOriginal", connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;

                param = new SqlParameter("@T112_ProcessingCode", SqlDbType.VarChar);
                param.Value = codigoProceso;
                comando.Parameters.Add(param);

                param = new SqlParameter("@cveEstatusPostOperacionDestino", SqlDbType.VarChar);
                param.Value = cveEstatusPostOperacionDestino;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_OperacionOriginal", SqlDbType.Int);
                param.Value = ID_OperacionOriginal;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_EstatusOperacion", SqlDbType.Int);
                param.Value = ID_EstatusOperacion;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_EstatusPostOperacion", SqlDbType.Int);
                param.Value = ID_EstatusPostOperacion;
                comando.Parameters.Add(param);



                comando.CommandTimeout = 10;

                comando.ExecuteNonQuery();


                return 0;


            }
            catch (SqlException e)
            {
                Log.Error("[COMP-PARABILIUM]:" + CLASE + ".ActualizaEstatusOperacionOriginal():" + e.Message);
                throw e;
            }
            catch (GenericalException err)
            {
                Log.Error("[COMP-PARABILIUM]:" + CLASE + ".ActualizaEstatusOperacionOriginal():" + err.Mensaje);
                throw err;
            }
            catch (Exception e)
            {
                Log.Error("[COMP-PARABILIUM]" + CLASE + " ActualizaEstatusOperacionOriginal(): " + e.Message);
                throw e;
            }
            finally
            {

            }

        }
        /// <summary>
        /// Metodo que  obtiene las varientes-reglas relacionadas a la pertenecia
        /// </summary>
        /// <param name="idPertenecia"></param>
        /// <param name="losParametros"></param>
        /// <param name="conexion"></param>
        /// <param name="transaccion"></param>
        /// <returns></returns>
        public List<ReglaVariante> getVariantesPerteneciaTipo(string cvePertenenciaTipo, ref Dictionary<String, Parametro> losParametros, SqlConnection conexion, SqlTransaction transaccion)
        {
            const string METODO = "getVariantesPerteneciaTipo";
            #region Locales
            List<ReglaVariante> listaVariantes = new List<ReglaVariante>();
            List<Parametro> Parametros;
            SqlDataReader SqlReader = null;

            Boolean accion = true;
            string nomRegla = string.Empty;
            string storedProc = string.Empty;
            int ejecucion = 1;
            #endregion
            try
            {
                Parametros = new List<Parametro>();
                SqlParameter param;
                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneVariantes", conexion);
                comando.Transaction = transaccion;
                comando.CommandType = CommandType.StoredProcedure;

                param = new SqlParameter("@cvePertenenciaTipo", SqlDbType.VarChar);
                param.Value = cvePertenenciaTipo;
                comando.Parameters.Add(param);

                SqlReader = comando.ExecuteReader();

                if (SqlReader != null || SqlReader.HasRows == true)
                {
                    while (SqlReader.Read())
                    {
                        if (storedProc == (string)SqlReader["Regla"] || storedProc.Length == 0)
                        {
                            accion = (Boolean)SqlReader["EsAccion"];
                            nomRegla = (string)SqlReader["Regla"];
                            ejecucion = (int)SqlReader["Ejecucion"];
                            storedProc = (string)SqlReader["Regla"];
                        }

                        if (!(storedProc == (string)SqlReader["Regla"]))
                        {
                            ReglaVariante reglaVariante = new ReglaVariante();

                            reglaVariante.EsAccion = accion;
                            reglaVariante.Nombre = nomRegla;
                            reglaVariante.OrdenEjecucion = ejecucion;
                            reglaVariante.StoredProcedure = storedProc;
                            // Agregamos los datos a la Regla
                            reglaVariante.Parametros = Parametros;
                            listaVariantes.Add(reglaVariante);
                            Parametros = new List<Parametro>();
                            storedProc = string.Empty;
                        }
                        Parametro parametro = new Parametro();
                        parametro.Nombre = (string)SqlReader["NombreParametro"];
                        parametro.TipoDato = TipoDato.getTipoDatoSQL((string)SqlReader["TipoDato"]);
                        String valor = null;
                        String NombreParam = null;
                        try
                        {
                            NombreParam = (string)SqlReader["NombreParametro"];
                            valor = losParametros[(string)SqlReader["NombreParametro"]].Valor;

                        }
                        catch (Exception)
                        {
                            Log.Error("[COMP-PARABILIUM]." + METODO + "Error al Obtener el valor de un parametro, puede no ser importante pero se levanta la alerta: " + NombreParam);
                            Log.Evento("[COMP-PARABILIUM]." + METODO + "INFORMATIVO: El parametro" + NombreParam + " no tiene un valor definido en la Operacion");
                        }

                        parametro.Nombre = (string)SqlReader["NombreParametro"];
                        parametro.TipoColectiva = (int)SqlReader["ID_TipoColectiva"];
                        parametro.TipoDato = TipoDato.getTipoDatoSQL((string)SqlReader["TipoDato"]);
                        parametro.ClaveTipoColectiva = (string)SqlReader["ClaveTipoColectiva"];

                        Parametros.Add(parametro);
                    }
                }
                if (storedProc.Length > 0)
                {
                    //inserta la ultima regla de la lista
                    ReglaVariante reglaVariante = new ReglaVariante();
                    reglaVariante.EsAccion = accion;
                    reglaVariante.Nombre = nomRegla;
                    reglaVariante.OrdenEjecucion = ejecucion;
                    reglaVariante.StoredProcedure = storedProc;
                    // Agregamos los datos a la Regla
                    reglaVariante.Parametros = Parametros;
                    listaVariantes.Add(reglaVariante);
                }
            }
            catch (Exception ex)
            {
                Log.Error("[COMP-PARABILIUM]." + METODO + " " + ex.Message);
                throw new Exception("al Obtener las REglas de Operación", ex);
            }
            finally
            {
                try
                {
                    SqlReader.Close();
                }
                catch (Exception) { }
            }
            return listaVariantes;
        }



        /// <summary>
        /// metodo que ejecuta una variante de manera dinamica asociada a una pertenecia
        /// </summary>
        /// <param name="reglaVariante"></param>
        /// <param name="losParametros"></param>
        /// <param name="connOperacion"></param>
        /// <param name="transaccionSQL"></param>
        /// <returns>RespuestaVariante</returns>
        public RespuestaVariante ejecutaVariante(ReglaVariante reglaVariante, ref Dictionary<string, Parametro> losParametros,
         SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {
            const string METODO = "ejecutaVariante";
            int codRespuesta = -1;
            string desRespuesta = string.Empty;



            Dictionary<string, string> paramsLog = new Dictionary<string, string>();
            int counter = 1;


            RespuestaVariante respReglasVariante = new RespuestaVariante()
            {
                CodigoRespuesta = -1
            };
            SqlParameter param;
            SqlDataReader losParametrosNuevos = null;
            //Si la regla no se ejecuto regresa True.
            string Paramentro = string.Empty;

            try
            {
                SqlCommand comando = new SqlCommand(reglaVariante.StoredProcedure, connOperacion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandTimeout = 10;
                comando.Transaction = transaccionSQL;

                foreach (Parametro valorParametro in reglaVariante.Parametros)
                {
                    TipoDatoSQL dato = valorParametro.TipoDato;
                    string elValorParametro = string.Empty;

                    if (losParametros.ContainsKey(valorParametro.Nombre))
                        elValorParametro = losParametros[valorParametro.Nombre].Valor;
                    else
                        elValorParametro = valorParametro.Valor;

                    switch (dato)
                    {
                        case TipoDatoSQL.VARCHAR:
                        case TipoDatoSQL.DATETIME:
                        case TipoDatoSQL.CHAR:
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.VarChar);
                            param.Value = elValorParametro;
                            comando.Parameters.Add(param);
                            break;

                        case TipoDatoSQL.INT:
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.Int);
                            param.Value = elValorParametro;
                            comando.Parameters.Add(param);
                            break;

                        case TipoDatoSQL.BIGINT:
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.BigInt);
                            param.Value = elValorParametro;
                            comando.Parameters.Add(param);
                            break;

                        case TipoDatoSQL.BIT:
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.Bit);
                            param.Value = elValorParametro;
                            comando.Parameters.Add(param);
                            break;

                        case TipoDatoSQL.DECIMAL:
                        case TipoDatoSQL.DOUBLE:
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.Decimal);
                            param.Value = elValorParametro;
                            comando.Parameters.Add(param);
                            break;

                        case TipoDatoSQL.FLOAT:
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.Float);
                            param.Value = elValorParametro;
                            comando.Parameters.Add(param);
                            break;

                        case TipoDatoSQL.TINYINT:
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.TinyInt);
                            param.Value = elValorParametro;
                            comando.Parameters.Add(param);
                            break;

                        case TipoDatoSQL.SMALLINT:
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.SmallInt);
                            param.Value = elValorParametro;
                            comando.Parameters.Add(param);
                            break;

                        default:
                            string msjError = "No hay Mapeo de Tipo de Dato : " + valorParametro.Nombre + " en la variante " + reglaVariante.Nombre + ".";
                            break;
                    }


                    paramsLog.Add("P" + counter.ToString(), valorParametro.Nombre + "=" + elValorParametro);
                    counter++;

                }

                param = new SqlParameter("@Respuesta", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Descripcion", SqlDbType.VarChar, 500);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                //Todas las reglas deben tener un parametro de Salida Llamado @Respuesta
                losParametrosNuevos = comando.ExecuteReader();

                //inserta los nuevos parametros
                if (null != losParametrosNuevos)
                {
                    while (losParametrosNuevos.Read())
                    {
                        Parametro nuevoParam = new Parametro();
                        Paramentro = (String)losParametrosNuevos["NombreParametro"];

                        nuevoParam.Nombre = (String)losParametrosNuevos["NombreParametro"];
                        nuevoParam.TipoDato = TipoDatoSQL.VARCHAR;
                        nuevoParam.Valor = losParametrosNuevos["Valor"].ToString();

                        if (losParametros.ContainsKey(nuevoParam.Nombre))
                            losParametros[nuevoParam.Nombre] = nuevoParam;
                        else
                            losParametros.Add(nuevoParam.Nombre, nuevoParam);

                    }
                }

                losParametrosNuevos.Close();

                codRespuesta = Int32.Parse(comando.Parameters["@Respuesta"].Value.ToString());
                desRespuesta = comando.Parameters["@Descripcion"].Value.ToString();

                //Asigna el codigo de respuesta despues de ejecutar la  variante-regla
                if (codRespuesta != 0)
                {
                    Log.Error("[COMP-PARABILIUM]." + METODO + " La Variante :" + reglaVariante.StoredProcedure + " genero el siguiente error:" + codRespuesta + ":" + desRespuesta);
                    throw new GenericalException(codRespuesta, desRespuesta);

                }
                else
                {
                    //quiere decir que las reglas aprobaron el mensaje
                    respReglasVariante.CodigoRespuesta = (int)CodRespuesta03.APPROVED;
                    respReglasVariante.DescripcionRespuesta = "Aprobada";
                }

            }
            catch (SqlException ex)
            {
                Log.Error("[COMP-PARABILIUM]." + METODO + ":SQLException" + ex.Message);

                respReglasVariante.DescripcionRespuesta = ex.Message;
                respReglasVariante.CodigoRespuesta = (int)CodRespuesta03.DATABASE_ERROR;
            }
            catch (GenericalException ex)
            {
                Log.Error("[COMP-PARABILIUM]." + METODO + ":GenericalException" + ex.Message);
                respReglasVariante.DescripcionRespuesta = desRespuesta;
                respReglasVariante.CodigoRespuesta = codRespuesta;
            }
            catch (Exception ex)
            {
                Log.Error("[COMP-PARABILIUM]." + METODO + ":Exception" + ex.Message);
                respReglasVariante.DescripcionRespuesta = ex.Message;
                respReglasVariante.CodigoRespuesta = (int)CodRespuesta03.OTHER_ERROR;
            }
            finally
            {
                try
                {
                    losParametrosNuevos.Close();
                }
                catch (Exception err)
                {
                    Log.Error("[COMP-PARABILIUM]." + METODO + ":Exception" + err.Message);
                }
            }

            return respReglasVariante;
        }


        /// <summary>
        /// metodo que obtiene los eventos a  ejecutar  atravez de la pertencia variante
        /// </summary>
        /// <param name="ID_Pertenencia"></param>
        /// <param name="connOperacion"></param>
        /// <param name="transaccionSQL"></param>
        /// <returns></returns>
        public List<String> getEventosAEjecutarPorPertenenciaVariante(Int32 ID_Pertenencia, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {
            const string METODO = "getEventosAEjecutarPorPertenencia";
            List<String> losEventos = new List<String>();
            SqlDataReader SqlReader = null;
            try
            {

                SqlParameter param;
                SqlCommand comando = new SqlCommand("[EJECUTOR_ObtieneEventosPertenenciaVariante]", connOperacion);
                comando.Transaction = transaccionSQL;
                comando.CommandType = CommandType.StoredProcedure;

                param = new SqlParameter("@idPertenenciaVariante", SqlDbType.Int);
                param.Value = ID_Pertenencia;
                comando.Parameters.Add(param);
                SqlReader = comando.ExecuteReader();

                if (null != SqlReader)
                {
                    while (SqlReader.Read())
                    {
                        losEventos.Add((string)SqlReader["ClaveEvento"]);
                    }
                }
            }
            catch (SqlException ex)
            {
                Log.Error("[COMP-PARABILIUM]." + METODO + " " + ex.Message);
                throw ex;
            }

            catch (Exception ex)
            {
                Log.Error("[COMP-PARABILIUM]." + METODO + " " + ex.Message);
                throw new Exception("Error al obtener los eventos de la pertencia-variante ha ejecutar", ex);
            }
            finally
            {
                try
                {
                    SqlReader.Close();
                }
                catch (Exception err) { }
            }

            return losEventos;
        }


    }
}

