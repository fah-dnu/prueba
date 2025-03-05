using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Utilidades;
using Executer.Entidades;
using Interfases.Entidades;
using Interfases.Enums;
using Interfases.Exceptions;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.CapaDatos
{
    public class DAOUtilerias
    {

        public SqlConnection _connConsulta;
        public SqlTransaction _transaccionSQL;



        public DAOUtilerias(SqlConnection connConsulta)
        {
            _connConsulta = connConsulta;

        }

        public DAOUtilerias(SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            _connConsulta = connConsulta;
            _transaccionSQL = transaccionSQL;
        }

        public int getIDTipoColectiva(String ClaveTipoColectiva)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            //CallableStatement spEjecutor = null;
            int IdTipoColectiva = 0;

            try
            {


                SqlParameter param = null;

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtieneIDTipoColectiva]");
                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneIDTipoColectiva", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;

                param = new SqlParameter("@ClaveTipoColectivaEvento", SqlDbType.VarChar);
                param.Value = ClaveTipoColectiva;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_TipoColectiva", SqlDbType.Int);
                param.Value = 0;
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                //resp = database.ExecuteNonQuery(command);
                comando.CommandTimeout = 5;

                comando.ExecuteNonQuery();

                IdTipoColectiva = Convert.ToInt32(comando.Parameters["@ID_TipoColectiva"].Value.ToString());


                if (IdTipoColectiva == 0)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA + "La Clave del Tipo de Colectiva no existe en la Base de Datos.]");
                    throw new GenericalException(CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA, "La Clave del Tipo de Colectiva no existe en la Base de Datos.");
                }

                return IdTipoColectiva;


            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:DaoUtilerias.getIDTipoColectiva(): " + e.Message + "]");
                throw e;
            }
            catch (GenericalException err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:DaoUtilerias.getIDTipoColectiva():" + err.Mensaje + "]");
                throw err;
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:DaoUtilerias.getIDTipoColectiva():" + e.Message + "]");
               throw e;
            }

        }



        public int getIDColectiva(String ClaveColectiva, Int64 ID_CadenaComercial, Int32 ID_TipoColectiva)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            // CallableStatement spEjecutor = null;
            int IdColectiva = 0;

            try
            {


                SqlParameter param = null;

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtieneIDColectiva]");
                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneIDColectiva", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;

                param = new SqlParameter("@ClaveColectiva", SqlDbType.VarChar);
                param.Value = ClaveColectiva;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_CadenaComercial", SqlDbType.BigInt);
                param.Value = ID_CadenaComercial;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_TipoColectiva", SqlDbType.BigInt);
                param.Value = ID_TipoColectiva;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Colectiva", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                //resp = database.ExecuteNonQuery(command);
                comando.CommandTimeout = 5;

                comando.ExecuteNonQuery();

                IdColectiva = Convert.ToInt32(comando.Parameters["@ID_Colectiva"].Value.ToString());



                if (IdColectiva == 0)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA + ", La Clave de la Colectiva no existe en la Base de Datos.]");
                    throw new GenericalException(CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA, "La Clave de la Colectiva no existe en la Base de Datos.");
                }

                return IdColectiva;


            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:DaoUtilerias.getIDTipoColectiva(): " + e.Message + "]");
                throw e;
            }
            catch (GenericalException err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:DaoUtilerias.getIDTipoColectiva():" + err.Mensaje + "]");
                throw err;
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:DaoUtilerias. getIDTipoColectiva(): " + e.Message + "]");
               throw e;
            }
            finally
            {

            }

        }

        public int getIDColectivaFromMA(String MedioAcceso, String TipoCuenta, Int64 la)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            //CallableStatement spEjecutor = null;
            int IdColectiva = 0;

            try
            {


                SqlParameter param = null;

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtieneIDColectivaFromMA]");
                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneIDColectivaFromMA", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;

                param = new SqlParameter("@MedioAcceso", SqlDbType.VarChar);
                param.Value = MedioAcceso;
                comando.Parameters.Add(param);

                param = new SqlParameter("@TipoCuenta", SqlDbType.VarChar);
                param.Value = TipoCuenta;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Colectiva", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                //resp = database.ExecuteNonQuery(command);
                comando.CommandTimeout = 5;

                comando.ExecuteNonQuery();

                IdColectiva = Convert.ToInt32(comando.Parameters["@ID_Colectiva"].Value.ToString());


                if (IdColectiva == 0)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA + ", La Clave de la Colectiva no existe en la Base de Datos.]");
                    throw new GenericalException(CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA, "La Clave de la Colectiva no existe en la Base de Datos.");
                }

                return IdColectiva;


            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:DaoUtilerias.getIDColectivaFromMA(): " + e.Message + "]");
                throw e;
            }
            catch (GenericalException err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:DaoUtilerias.getIDColectivaFromMA():" + err.Mensaje + "]");
                throw err;
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:DaoUtilerias. getIDColectivaFromMA(): " + e.Message + "]");
                throw e;
            }
            finally
            {

            }

        }
        public void getIDPertenencia(String claveTipoMensaje, String claveMedioAcceso, String claveTipoMedioAcceso, String claveBeneficiario, String TipoMoneda,
        String claveProcessingCode,
        String claveAfiliacion,
        ref Dictionary<String, Parametro> parametros)
        {

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            //CallableStatement spEjecutor=null ;
            // Map<String, IParametro> respuesta= new HashMap<String, IParametro>();
            SqlParameter param = null;

            try
            {

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtienePertenencia]");
                SqlCommand comando = new SqlCommand("EJECUTOR_ObtienePertenencia", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;

                param = new SqlParameter("@MedioAcceso", SqlDbType.VarChar);
                param.Value = claveMedioAcceso;
                comando.Parameters.Add(param);

                param = new SqlParameter("@TipoMedioAcceso", SqlDbType.VarChar);
                param.Value = claveTipoMedioAcceso;
                comando.Parameters.Add(param);

                param = new SqlParameter("@CodigoMoneda", SqlDbType.VarChar);
                param.Value = TipoMoneda;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Beneficiario", SqlDbType.VarChar);
                param.Value = claveBeneficiario;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ProcessingCode", SqlDbType.VarChar);
                param.Value = claveProcessingCode;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Afiliacion", SqlDbType.VarChar);
                param.Value = claveAfiliacion;
                comando.Parameters.Add(param);

                param = new SqlParameter("@tipoMensaje", SqlDbType.VarChar);
                param.Value = claveTipoMensaje;
                comando.Parameters.Add(param);



                param = new SqlParameter("@ID_Pertenencia", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_CadenaComercial", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_GrupoComercial", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Emisor", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_GrupoCuenta", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_GrupoMA", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_CuentaHabiente", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_TarjetaHabiente", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);


                //resp = database.ExecuteNonQuery(command);
                comando.CommandTimeout = 5;

                comando.ExecuteNonQuery();

                Parametro parametro1 = new Parametro();
                parametro1.Nombre = "@ID_Pertenencia";
                parametro1.ClaveTipoColectiva = null;
                parametro1.TipoDato = TipoDatoSQL.VARCHAR;
                parametro1.Valor = comando.Parameters["@ID_Pertenencia"].Value.ToString();

                Parametro parametro2 = new Parametro();
                parametro2.Nombre = "@ID_Emisor";
                parametro2.ClaveTipoColectiva = "EMI";
                parametro2.TipoDato = TipoDatoSQL.VARCHAR;
                parametro2.Valor = comando.Parameters["@ID_Emisor"].Value.ToString();

                Parametro parametro3 = new Parametro();
                parametro3.Nombre = "@ID_CadenaComercial";
                parametro3.ClaveTipoColectiva = "CCM";
                parametro3.TipoDato = TipoDatoSQL.VARCHAR;
                parametro3.Valor = comando.Parameters["@ID_CadenaComercial"].Value.ToString();

                Parametro parametro4 = new Parametro();
                parametro4.Nombre = "@ID_GrupoCuenta";
                parametro4.ClaveTipoColectiva = null;
                parametro4.TipoDato = TipoDatoSQL.VARCHAR;
                parametro4.Valor = comando.Parameters["@ID_GrupoCuenta"].Value.ToString();

                Parametro parametro5 = new Parametro();
                parametro5.Nombre = "@ID_GrupoMA";
                parametro5.ClaveTipoColectiva = null;
                parametro5.TipoDato = TipoDatoSQL.VARCHAR;
                parametro5.Valor = comando.Parameters["@ID_GrupoMA"].Value.ToString();

                Parametro parametro6 = new Parametro();
                parametro6.Nombre = "@ID_GrupoComercial";
                parametro6.ClaveTipoColectiva = "GCM";
                parametro6.TipoDato = TipoDatoSQL.VARCHAR;
                parametro6.Valor = comando.Parameters["@ID_GrupoComercial"].Value.ToString();

                Parametro parametro7 = new Parametro();
                parametro7.Nombre = "@ID_TarjetaHabiente";
                parametro7.ClaveTipoColectiva = "CCH";
                parametro7.TipoDato = TipoDatoSQL.VARCHAR;
                parametro7.Valor = comando.Parameters["@ID_TarjetaHabiente"].Value.ToString();

                parametros.Add(parametro1.Nombre, parametro1);
                parametros.Add(parametro2.Nombre, parametro2);
                parametros.Add(parametro3.Nombre, parametro3);
                parametros.Add(parametro4.Nombre, parametro4);
                parametros.Add(parametro5.Nombre, parametro5);
                parametros.Add(parametro6.Nombre, parametro6);
                parametros.Add(parametro7.Nombre, parametro7);


                if (parametro1.Valor == null || parametro1.Valor == "0")
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + CodRespuesta03.NO_PERTENENCIA, "[EJECUTOR] No hay Pertenencia para los valores: Medio Acceso:" + claveMedioAcceso + ",TipoMedioAcceso:" + claveTipoMedioAcceso + ", CodigoMoneda: " +
                            " Beneficiario: " + claveBeneficiario + ", ProcessingCode:" + claveProcessingCode + ", Afiliacion:" + claveAfiliacion + "]");

                    //Levanta exception por no tener pertenencia.
                    throw new GenericalException(CodRespuesta03.NO_PERTENENCIA, "[EJECUTOR] No hay Pertenencia para los valores: Medio Acceso:" + claveMedioAcceso + ",TipoMedioAcceso:" + claveTipoMedioAcceso + ", CodigoMoneda: " +
                            " Beneficiario: " + claveBeneficiario + ", ProcessingCode:" + claveProcessingCode + ", Afiliacion:" + claveAfiliacion);
                }

            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, getPertenencia(): " + e.Message + "]");
                throw e;
            }
            catch (GenericalException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR] getPertenencia(): " + e.Mensaje + "]");
                throw e;
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, getPertenencia(): " + e.Message + "]");
                throw e;
            }
            finally
            {
                //try {_conn.close();}catch(Exception e){}

            }


        }


        public void getParametrosReferidosPertenencia(int ID_TipoContrato, int ID_CadenaComercial, ref Dictionary<String, Parametro> losParametros)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            //CallableStatement spEjecutor=null ;
            //ResultSet Resultado=null;
            SqlDataReader SqlReader = null;

            try
            {
                SqlParameter param;

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtieneParamentroReferencia]");
                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneParamentroReferencia", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;

                param = new SqlParameter("@ID_TipoContrato", SqlDbType.BigInt);
                param.Value = ID_TipoContrato;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_CadenaComercial", SqlDbType.BigInt);
                param.Value = ID_CadenaComercial;
                comando.Parameters.Add(param);

                SqlReader = comando.ExecuteReader();

                if (null != SqlReader)
                {

                    while (SqlReader.Read())
                    {

                        Parametro unParametro = new Parametro();
                        unParametro.Nombre = (string)SqlReader["NombreParametro"];
                        unParametro.TipoColectiva = (int)SqlReader["ID_TipoColectiva"];
                        unParametro.TipoDato = TipoDato.getTipoDatoSQL((string)SqlReader["TipoDato"]);
                        unParametro.ClaveTipoColectiva = (string)SqlReader["ClaveTipoColectiva"];
                        unParametro.Valor = (string)SqlReader["Valor"];
                        losParametros.Add((string)SqlReader["NombreParametro"], unParametro);
                    }
                }


            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR]getParametrosReferidosPertenencia(): " + e.Message + "]");
               throw e;
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR]getParametrosReferidosPertenencia(): " + e.Message + "]");
                throw e;
            }
            finally
            {
                try
                {
                    if(SqlReader != null)
                        SqlReader.Close();
                }
                catch (Exception e)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [DAOUtilerias.getParametrosReferidosPertenencia().Try: " + e.Message + "]");
                }
            }

        }



        public Dictionary<String, Parametro> getParametrosEventosAgrupados(Int64 ID_CadenaComercial, Int64 ID_EventoAgrupado, Int64 ID_corte)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            //CallableStatement spEjecutor=null ;
            //ResultSet Resultado=null;
            Dictionary<String, Parametro> losParametros = new Dictionary<String, Parametro>();
            SqlDataReader SqlReader = null;

            try
            {
                SqlParameter param;

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtieneParametrosEventosAgrupadosOperacion]");            
                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneParametrosEventosAgrupadosOperacion", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;


                param = new SqlParameter("@ID_CadenaComercial", SqlDbType.BigInt);
                param.Value = ID_CadenaComercial;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_EventoAgrupado", SqlDbType.BigInt);
                param.Value = ID_EventoAgrupado;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Corte", SqlDbType.BigInt);
                param.Value = ID_corte;
                comando.Parameters.Add(param);

                SqlReader = comando.ExecuteReader();

                if (null != SqlReader)
                {

                    while (SqlReader.Read())
                    {

                        Parametro unParametro = new Parametro();
                        unParametro.Nombre = (string)SqlReader["NombreParametro"];
                        unParametro.TipoColectiva = 0;
                        unParametro.TipoDato = TipoDato.getTipoDatoSQL("Money");
                        unParametro.ClaveTipoColectiva = "";
                        unParametro.Valor = (string)SqlReader["Valor"];
                        losParametros.Add((string)SqlReader["NombreParametro"], unParametro);
                    }
                }

                return losParametros;
            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR]getParametrosEventosAgrupados(): " + e.Message + "]");
                throw e;
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR]getParametrosEventosAgrupados(): " + e.Message + "]");
                throw e;
            }
            finally
            {
                try
                {
                    if(SqlReader != null)
                        SqlReader.Close();
                }
                catch (Exception e)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [DAOUtilerias.getParametrosEventosAgrupados().Try: " + e.Message + "]");
                }
            }

        }



        public Int64 InsertaLoteCorte(Int64 ID_CadenaComercial, Int64 ID_EventoAgrupado)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            //CallableStatement spEjecutor=null ;
            //ResultSet Resultado=null;
            Dictionary<String, Parametro> losParametros = new Dictionary<String, Parametro>();
            SqlDataReader SqlReader = null;
            Int64 ID_Lote = 0;
            try
            {
                SqlParameter param;

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_InsertaLoteCorte]");
                SqlCommand comando = new SqlCommand("EJECUTOR_InsertaLoteCorte", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                //  comando.Transaction = _transaccionSQL;


                param = new SqlParameter("@ID_CadenaComercial", SqlDbType.BigInt);
                param.Value = ID_CadenaComercial;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_ConfiguracionCorte", SqlDbType.BigInt);
                param.Value = ID_EventoAgrupado;
                comando.Parameters.Add(param);

                SqlReader = comando.ExecuteReader();

                if (null != SqlReader)
                {

                    while (SqlReader.Read())
                    {


                        ID_Lote = ((Int64)SqlReader["ID_Lote"]);

                    }
                }

                return ID_Lote;
            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR]InsertaLoteCorte(): " + e.Message + "]");
                return -1;
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR]InsertaLoteCorte(): " + e.Message + "]");
                return -1;
            }
            finally
            {
                try {
                    if (SqlReader != null)
                        SqlReader.Close();
                }
                catch (Exception e)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [DAOUtilerias.InsertaLoteCorte().Try: " + e.Message + "]");
                 }
            }

        }



        //
        public static Int64 InsertaCorte(String ClaveCorteTipo, DateTime Fecha_corte, int ID_cuenta, SqlConnection conn, SqlTransaction transaccionSQL)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            string Fecha_Corte_s = Convert.ToString(Fecha_corte).Substring(0, 10);

            DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            string formattedDate = date.ToString("yyyy-MM-dd");

            SqlDataReader SqlReader = null;
            Int64 ID_Lote = 0;

            try
            {


                SqlParameter param;
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_InsertaCorteCuenta]");
                SqlCommand comando2 = new SqlCommand("EJECUTOR_InsertaCorteCuenta", conn);
                comando2.Transaction = transaccionSQL;
                comando2.CommandType = CommandType.StoredProcedure;

                param = new SqlParameter("@ClaveCorteTipo", SqlDbType.VarChar);
                param.Value = ClaveCorteTipo;
                comando2.Parameters.Add(param);

                param = new SqlParameter("@Fecha_Corte", SqlDbType.DateTime);
                param.Value = formattedDate;
                comando2.Parameters.Add(param);


                param = new SqlParameter("@ID_Cuenta", SqlDbType.Int);
                param.Value = ID_cuenta;
                comando2.Parameters.Add(param);

                //  comando2.ExecuteNonQuery();
                SqlReader = comando2.ExecuteReader();


                if (null != SqlReader)
                {

                    while (SqlReader.Read())
                    {


                        ID_Lote = ((Int64)SqlReader["ID_Corte"]);

                    }
                }




            }

            catch (Exception err)
            {
                //   transaccionSQL.Rollback();
                // GeneracionExitosaDePolizas = false;
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GenerarLasPolizasDeCortes() Error se deshacen la polizas para la cadena comercial]");
              
            }
            return ID_Lote;

        }



        public Int64 InsertaCorteCuenta(String ClaveCorteTipo, DateTime Fecha_corte, Int64 ID_cuenta, String Fecha_Final, String DiaFechaCorte,
          String Fecha_Inicial, string DiaLimitePago, Int64 idCorte, SqlTransaction transaccionSQL, string numeroCobranza)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            //string Fecha_Corte_s = Convert.ToString(Fecha_corte).Substring(0, 10);

            //DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            string formattedDate = Fecha_corte.ToString("yyyy-MM-dd");

            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            DateTime dateFechaLimiteDePago = Convert.ToDateTime(DiaLimitePago);
            string conversionFormatoFecha = dateFechaLimiteDePago.ToString("yyyy-MM-dd");


            //CallableStatement spEjecutor=null ;
            //ResultSet Resultado=null;
            Dictionary<String, Parametro> losParametros = new Dictionary<String, Parametro>();
            SqlDataReader SqlReader = null;
            Int64 ID_Lote = 0;
            try
            {
                SqlParameter param;

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_InsertaCorteCuenta]");
                SqlCommand comando = new SqlCommand("EJECUTOR_InsertaCorteCuenta", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;//_transaccionSQL;


                //param = new SqlParameter("@ID_CadenaComercial", SqlDbType.BigInt);
                //param.Value = ID_CadenaComercial;
                //comando.Parameters.Add(param);

                //param = new SqlParameter("@ID_ConfiguracionCorte", SqlDbType.BigInt);
                //param.Value = ID_EventoAgrupado;
                //comando.Parameters.Add(param);


                param = new SqlParameter("@ClaveCorteTipo", SqlDbType.VarChar);
                param.Value = ClaveCorteTipo;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Fecha_Corte", SqlDbType.DateTime);
                param.Value = formattedDate;
                comando.Parameters.Add(param);


                param = new SqlParameter("@ID_Cuenta", SqlDbType.Int);
                param.Value = ID_cuenta;
                comando.Parameters.Add(param);


                param = new SqlParameter("@Fecha_Final", SqlDbType.DateTime);
                param.Value = Fecha_Final;
                comando.Parameters.Add(param);

                param = new SqlParameter("@DiaFechaCorte", SqlDbType.Int);
                param.Value = DiaFechaCorte;
                comando.Parameters.Add(param);



                param = new SqlParameter("@Fecha_Inicial", SqlDbType.DateTime);
                param.Value = Fecha_Inicial;
                comando.Parameters.Add(param);


                param = new SqlParameter("@DiaFechaLimitePago", SqlDbType.DateTime);
                param.Value = conversionFormatoFecha;
                comando.Parameters.Add(param);

                param = new SqlParameter("@numeroMes", SqlDbType.Int);
                param.Value = numeroCobranza;
                comando.Parameters.Add(param);



                SqlReader = comando.ExecuteReader();

                if (null != SqlReader)
                {

                    while (SqlReader.Read())
                    {


                        ID_Lote = ((Int64)SqlReader["ID_Corte"]);

                    }
                }

                return ID_Lote;
            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, InsertaLoteCorte(): " + e.Message + "]");
                return -1;
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, InsertaLoteCorte(): " + e.Message + "]");
                return -1;
            }
            finally
            {
                try {
                    if (SqlReader != null)
                        SqlReader.Close();
                }
                catch (Exception e)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [DAOUtilerias.InsertaLoteCorte().Try: " + e.Message + "]");
               }
            }

        }

        public Int64 InsertaSaldoEnCorte(Int64 ID_Corte, Int64 ID_CadenaComercial, Int64 ID_ConfiguracionCorte)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            //CallableStatement spEjecutor=null ;
            //ResultSet Resultado=null;
            Dictionary<String, Parametro> losParametros = new Dictionary<String, Parametro>();
            SqlDataReader SqlReader = null;
            Int64 ID_Lote = 0;
            try
            {
                SqlParameter param;

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_AjustaPolizasInsertaSaldoYCalculaNuevaFecha]");
                SqlCommand comando = new SqlCommand("EJECUTOR_AjustaPolizasInsertaSaldoYCalculaNuevaFecha", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                // comando.Transaction = _transaccionSQL;


                param = new SqlParameter("@ID_Corte", SqlDbType.BigInt);
                param.Value = ID_Corte;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_CadenaComercial", SqlDbType.BigInt);
                param.Value = ID_CadenaComercial;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_ConfiguracionCorte", SqlDbType.BigInt);
                param.Value = ID_ConfiguracionCorte;
                comando.Parameters.Add(param);


                comando.ExecuteNonQuery();

                return 0;
            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, InsertaSaldoEnCorte(): " + e.Message + "]");
                throw new Exception("[EJECUTOR]InsertaSaldoEnCorte(): " + e.Message);
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, InsertaSaldoEnCorte(): " + e.Message + "]");
                throw new Exception("[EJECUTOR]InsertaSaldoEnCorte(): " + e.Message);
            }
            finally
            {
                try {
                    if (SqlReader != null)
                        SqlReader.Close();
                }
                catch (Exception e)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [DAOUtilerias.InsertaSaldoEnCorte().Try: " + e.Message + "]");
                 }
            }

        }



        public Int64 LigarPolizasAEventoDeCorte(Int64 ID_Evento, Int64 ID_Corte, SqlTransaction transaccionSQL)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            SqlDataReader SqlReader = null;
            Int64 ID_Lote = 0;
            try
            {
                SqlParameter param;

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_LigarPolizasAEventoDeCorte]");
                SqlCommand comando = new SqlCommand("EJECUTOR_LigarPolizasAEventoDeCorte", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;


                param = new SqlParameter("@ID_Evento", SqlDbType.BigInt);
                param.Value = ID_Evento;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Corte", SqlDbType.BigInt);
                param.Value = ID_Corte;
                comando.Parameters.Add(param);



                comando.ExecuteNonQuery();

                return 0;
            }
            catch (SqlException e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, LigarPolizasAEventoDeCorte(): " + e.Message + "]");
                 return -1;
            }
            catch (Exception e)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, LigarPolizasAEventoDeCorte(): " + e.Message + "]");
                return -1;
            }
            finally
            {
                try {
                    if (SqlReader != null)
                        SqlReader.Close();
                }
                catch (Exception e)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [DAOUtilerias.LigarPolizasAEventoDeCorte().Try: " + e.Message + "]");
                }
            }

        }

    }
}