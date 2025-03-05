using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Utilidades;
using Executer.Entidades;
using Executer.Utilidades;
using Interfases.Entidades;
//using Interfases.Entidades;
using Interfases.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    public class DAOPertenencia
    {


        public static int getPertenencia(Regla regla, ref Dictionary<String, Parametro> losParametros, SqlConnection connOperacion, SqlTransaction transaccionSQL
                            , RespuestaProcesaMov res)
        {
            int codRespuesta = -1;
            String desRespuesta = "";

            int resp = -1;
            SqlParameter param;
            SqlDataReader losParametrosNuevos = null;
            //Si la regla no se ejecuto regresa True.
            string Paramentro = "";
            string MedioAcceso = "";
            try
            {


                SqlCommand comando = new SqlCommand(regla.StoredProcedure, connOperacion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandTimeout = 10;
                comando.Transaction = transaccionSQL;


                foreach (Parametro valorParametro in regla.Parametros)
                {

                    TipoDatoSQL dato = valorParametro.TipoDato;


                    String elValorParametro = "";

                    if (losParametros.ContainsKey(valorParametro.Nombre))
                    {
                        elValorParametro = losParametros[valorParametro.Nombre].Valor;
                    }
                    else
                    {
                        elValorParametro = valorParametro.Valor;
                    }
                    switch (dato)
                    {
                        case TipoDatoSQL.VARCHAR:
                            if (valorParametro.Nombre == "@MedioAcceso")
                            {
                                MedioAcceso = elValorParametro;
                                SqlParameter paramSSN = comando.CreateParameter();
                                paramSSN.ParameterName = valorParametro.Nombre;
                                paramSSN.DbType = DbType.AnsiStringFixedLength;
                                paramSSN.Direction = ParameterDirection.Input;
                                paramSSN.Value = elValorParametro;
                                paramSSN.Size = 50;
                                comando.Parameters.Add(paramSSN);
                                break;
                            }
                            if (valorParametro.Nombre == "@MedioAccesoDesencriptado")
                            {
                                param = new SqlParameter(valorParametro.Nombre, SqlDbType.VarChar);
                                param.Value = MedioAcceso;
                                comando.Parameters.Add(param);
                                break;
                            }
                            else
                            {
                                param = new SqlParameter(valorParametro.Nombre, SqlDbType.VarChar);
                                param.Value = elValorParametro;
                                comando.Parameters.Add(param);
                                break;
                            }
                            break;
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
                            String msjError = "No hay Mapearo de Tipo de Dato : " + valorParametro.Nombre + " en la Regla " + regla.Nombre + ".";
                            Loguear.Error(msjError);
                            break;

                    }
                }


                //param = new SqlParameter("@Respuesta", SqlDbType.BigInt);
                //param.Value = "";
                //param.Direction = ParameterDirection.Output;
                //comando.Parameters.Add(param);

                //param = new SqlParameter("@Descripcion", SqlDbType.VarChar, 500);
                //param.Value = "";
                //param.Direction = ParameterDirection.Output;
                //comando.Parameters.Add(param);

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

                        int idtopoCol = 0;
                        if (Int32.TryParse(losParametrosNuevos["ID_TipoColectiva"].ToString(), out idtopoCol))
                        {
                            nuevoParam.ID_TipoColectiva = idtopoCol;
                        }



                        if (losParametros.ContainsKey(nuevoParam.Nombre))
                        {
                            losParametros[nuevoParam.Nombre] = nuevoParam;

                        }
                        else
                        {
                            losParametros.Add(nuevoParam.Nombre, nuevoParam);
                        }

                        Loguear.Evento("La regla:" + regla.StoredProcedure + " genero el Parametro: " + nuevoParam.Nombre + "=" + nuevoParam.Valor);

                    }
                }


                losParametrosNuevos.Close();

                return int.Parse(losParametros["@ID_Pertenencia"].Valor);

            }
            catch (SqlException ex)
            {
                if (ex.Message.ToString().Contains("NO HAY RELACIONES DE PERTENENCIA"))
                {
                    res.laRespuesta = Constants.ConstantesCompensacion.CodigoRespuestaPertenenciaInexistente;
                }
                Loguear.Error("[EJECUTOR] getPertenencia():SQLException " + ex.Message);
               
                throw new Exception("[EJECUTOR] getPertenencia():SQLException " + ex.Message);
            }

            catch (Exception ex)
            {
                Loguear.Error("[EJECUTOR] getPertenencia():" + ex.Message);
              
                throw new Exception("[EJECUTOR] getPertenencia():SQLException " + ex.Message);
            }
            finally
            {
                try
                {
                    losParametrosNuevos.Close();
                }
                catch (Exception err)
                {

                }
            }

            return resp;
        }

        public static Regla getParamentrosStoredPertenencia(SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {


            //String stored = new String();
            Regla reglas = new Regla();
            List<Parametro> Parametros;
            //  DAOUtilerias obtenParametros = new DAOUtilerias(_connConsulta, _transaccionSQL);
            SqlDataReader SqlReader = null;



            Boolean accion = true;
            String nomRegla = "", storedProc = "";
            int ejecucion = 1, cont = 0;

            //CallableStatement spEjecutor = null;
            //ResultSet Resultado=null;

            try
            {


                Parametros = new List<Parametro>();
                SqlParameter param;
                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneParametrosPertenencia", connOperacion);
                comando.Transaction = transaccionSQL;
                comando.CommandType = CommandType.StoredProcedure;




                SqlReader = comando.ExecuteReader();

                if (null != SqlReader)
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
                            Regla spRegla = new Regla();

                            spRegla.esAccion = accion;
                            spRegla.Nombre = nomRegla;
                            spRegla.OrdenEjecucion = ejecucion;
                            spRegla.StoredProcedure = storedProc;
                            // Agregamos los datos a la Regla
                            spRegla.Parametros = Parametros;
                            reglas = (spRegla);
                            Parametros = new List<Parametro>();
                            storedProc = "";
                        }

                        //Si el SP no regresa ni una regla no debe agregar nada.
                        Parametro parametro = new Parametro();
                        parametro.Nombre = (string)SqlReader["NombreParametro"];
                        parametro.TipoDato = TipoDato.getTipoDatoSQL((string)SqlReader["TipoDato"]);
                        String valor = "";
                        String NombreParam = "";
                        try
                        {
                            NombreParam = (string)SqlReader["NombreParametro"];
                            valor = "";

                        }
                        catch (Exception err)
                        {
                            Loguear.Error("Error al Obtener el valor de un parametro, puede no ser importante pero se levanta la alerta: " + NombreParam);
                            Loguear.Evento("INFORMATIVO: El parametro" + NombreParam + " no tiene un valor definido en la Operacion");
                        }
                        parametro.Valor = valor;
                        // Parametros.Add(parametro);


                        //Parametro unParametro = new Parametro();
                        parametro.Nombre = (string)SqlReader["NombreParametro"];
                        parametro.TipoColectiva = (int)SqlReader["ID_TipoColectiva"];
                        parametro.TipoDato = TipoDato.getTipoDatoSQL((string)SqlReader["TipoDato"]);
                        parametro.ClaveTipoColectiva = (string)SqlReader["ClaveTipoColectiva"];
                        // unParametro.Valor = (string)SqlReader["Valor"];
                        Parametros.Add(parametro);
                    }
                }



                if (storedProc.Length > 0)
                {
                    //inserta la ultima regla de la lista
                    Regla spRegla = new Regla();
                    spRegla.esAccion = accion;
                    spRegla.Nombre = nomRegla;
                    spRegla.OrdenEjecucion = ejecucion;
                    spRegla.StoredProcedure = storedProc;
                    // Agregamos los datos a la Regla
                    spRegla.Parametros = Parametros;
                    reglas = (spRegla);
                }


            }
            catch (SqlException ex)
            {
                Loguear.Error("[EJECUTOR] getParamentrosStoredPertenencia(): " + ex.Message);
                throw ex;
            }

            catch (Exception ex)
            {
                Loguear.Error("[EJECUTOR] getReglasOperacion(): " + ex.Message);
                throw new Exception("al Obtener la  Pertenencia" + ex.Message, ex);
            }
            finally
            {
                try
                {
                    SqlReader.Close();
                }
                catch (Exception err) { }
            }

            return reglas;
        }


        public static List<String> getEventosAEjecutarPorPertenencia(Int32 ID_Pertenencia, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {


            //String stored = new String();
            List<String> losEventos = new List<String>();
            List<Parametro> Parametros;
            //  DAOUtilerias obtenParametros = new DAOUtilerias(_connConsulta, _transaccionSQL);
            SqlDataReader SqlReader = null;



            Boolean accion = true;
            String nomRegla = "", storedProc = "";
            int ejecucion = 1, cont = 0;

            //CallableStatement spEjecutor = null;
            //ResultSet Resultado=null;

            try
            {


                // Parametros = new List<Parametro>();
                SqlParameter param;
                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneEventosPertenencia", connOperacion);
                comando.Transaction = transaccionSQL;
                comando.CommandType = CommandType.StoredProcedure;

                param = new SqlParameter("@ID_Pertenencia", SqlDbType.Int);
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
                Loguear.Error("[EJECUTOR] getReglasOperacion(): " + ex.Message);
                throw ex;
            }

            catch (Exception ex)
            {
                Loguear.Error("[EJECUTOR] getReglasOperacion(): " + ex.Message);
                throw new Exception("al Obtener las REglas de Operación", ex);
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
