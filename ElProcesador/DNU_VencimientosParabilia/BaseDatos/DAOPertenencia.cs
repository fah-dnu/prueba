using DNU_VencimientosParabilia.Utilidades;
using Executer.Entidades;
using Executer.Utilidades;
using Interfases.Entidades;
using Interfases.Enums;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_VencimientosParabilia.BaseDatos
{
    public class DAOPertenencia
    {


        public static int getPertenencia(Regla regla, ref Dictionary<String, Parametro> losParametros, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
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

                    //spEjecutor.s("hola", valorParametro.getNombre());
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
                            String msjError = "No hay Mapeo de Tipo de Dato : " + valorParametro.Nombre + " en la Regla " + regla.Nombre + ".";
                            LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [" + msjError + " ]");
                            break;

                    }
                }

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
                        {
                            losParametros[nuevoParam.Nombre] = nuevoParam;

                        }
                        else
                        {
                            losParametros.Add(nuevoParam.Nombre, nuevoParam);
                        }

                        LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [La regla:" + regla.StoredProcedure + " genero el Parametro: " + nuevoParam.Nombre + "=" + nuevoParam.Valor + " ]");
                     
                    }
                }


                losParametrosNuevos.Close();

                return int.Parse(losParametros["@ID_Pertenencia"].Valor);

            }
            catch (SqlException ex)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, getPertenencia():SQLException " + ex.Message + " ]");
                // resp = (int)CodRespuesta03.DATABASE_ERROR;
                throw new Exception("[EJECUTOR] getPertenencia():SQLException " + ex.Message);
            }

            catch (Exception ex)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR] getPertenencia():" + ex.Message + " ]");
        
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
                    LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + " ]");

                }
            }

            return resp;
        }

        public static Regla getParamentrosStoredPertenencia(SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {
            string log = ""; //ThreadContext.Properties["log"].ToString();
            string ip = ""; //ThreadContext.Properties["ip"].ToString();

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

                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtieneParametrosPertenencia]");

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
                            LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Error al Obtener el valor de un parametro, puede no ser importante pero se levanta la alerta: " + NombreParam + "]");
                            LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [INFORMATIVO: El parametro" + NombreParam + " no tiene un valor definido en la Operacion]");
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
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, getParamentrosStoredPertenencia(): " + ex.Message + "]");
                throw ex;
            }

            catch (Exception ex)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [[EJECUTO,] getReglasOperacion(): " + ex.Message + "]");
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
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();

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

                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtieneEventosPertenencia]");
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
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, getReglasOperacion(): " + ex.Message + "]");
                 throw ex;
            }

            catch (Exception ex)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, getReglasOperacion(): " + ex.Message + "]");
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
