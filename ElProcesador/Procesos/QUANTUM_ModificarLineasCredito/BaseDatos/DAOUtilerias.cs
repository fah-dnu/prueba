using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
//using Executer.Utilidades;
using System.Data;
using Interfases.Enums;
using Interfases.Exceptions;
using Executer.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Interfases.Entidades;
using CommonProcesador;

namespace Executer.BaseDatos
{
    class DAOUtilerias
    {
        
    SqlConnection _connConsulta;
        SqlTransaction _transaccionSQL;


        public DAOUtilerias(SqlConnection connConsulta, SqlTransaction transaccionSQL)
    {
        _connConsulta=connConsulta;
        _transaccionSQL = transaccionSQL;
    }

    public int getIDTipoColectiva(String ClaveTipoColectiva)  {



        //CallableStatement spEjecutor = null;
        int IdTipoColectiva=0;

        try {


            SqlParameter param = null;

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


            if (IdTipoColectiva==0)
            {
                throw new GenericalException(CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA, "La Clave del Tipo de Colectiva no existe en la Base de Datos.");
            }

            return IdTipoColectiva;


        } catch (SqlException e) {
            Logueo.Error("EJECUTOR:DaoUtilerias.getIDTipoColectiva(): " + e.Message);
            throw e;
        } catch (GenericalException err) {
            Logueo.Error("EJECUTOR:DaoUtilerias.getIDTipoColectiva():" + err.Mensaje);
            throw err;
        } catch (Exception e) {
            Logueo.Error("EJECUTOR:DaoUtilerias. getIDTipoColectiva(): " + e.Message);
            throw e;
        } 

    }

    public int getIDColectiva(String ClaveColectiva, Int64 ID_CadenaComercial, Int32 ID_TipoColectiva)
    {



       // CallableStatement spEjecutor = null;
        int IdColectiva=0;

        try {


                        SqlParameter param = null;

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



            if (IdColectiva==0)
            {
                throw new GenericalException(CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA, "La Clave de la Colectiva no existe en la Base de Datos.");
            }

            return IdColectiva;


        } catch (SqlException e) {
            Logueo.Error("EJECUTOR:DaoUtilerias.getIDTipoColectiva(): " + e.Message);
            throw e;
        } catch (GenericalException err) {
            Logueo.Error("EJECUTOR:DaoUtilerias.getIDTipoColectiva():" + err.Mensaje);
            throw err;
        } catch (Exception e) {
            Logueo.Error("EJECUTOR:DaoUtilerias. getIDTipoColectiva(): " + e.Message);
            throw e;
        } finally {
            
        }

    }
    
   public int getIDColectivaFromMA(String MedioAcceso, String TipoCuenta,Int64 la) {



        //CallableStatement spEjecutor = null;
        int IdColectiva=0;

        try {


            SqlParameter param = null;

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

                  
            if (IdColectiva==0)
            {
                throw new GenericalException(CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA, "La Clave de la Colectiva no existe en la Base de Datos.");
            }

            return IdColectiva;


        } catch (SqlException e) {
            Logueo.Error("EJECUTOR:DaoUtilerias.getIDColectivaFromMA(): " + e.Message);
            throw e;
        } catch (GenericalException err) {
            Logueo.Error("EJECUTOR:DaoUtilerias.getIDColectivaFromMA():" + err.Mensaje);
            throw err;
        } catch (Exception e) {
            Logueo.Error("EJECUTOR:DaoUtilerias. getIDColectivaFromMA(): " + e.Message);
            throw e;
        } finally {
           
        }

    }
    public void getIDPertenencia(String claveTipoMensaje,String claveMedioAcceso,String claveTipoMedioAcceso,String claveBeneficiario, String TipoMoneda,
    String claveProcessingCode,
    String claveAfiliacion,
    ref Dictionary<String, Parametro> parametros)
    {
        //CallableStatement spEjecutor=null ;
      // Map<String, IParametro> respuesta= new HashMap<String, IParametro>();
        SqlParameter param = null;
     
         try {

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

            Parametro  parametro1 = new Parametro();
            parametro1.Nombre="@ID_Pertenencia";
            parametro1.ClaveTipoColectiva=null;
            parametro1.TipoDato=TipoDatoSQL.VARCHAR;
            parametro1.Valor = comando.Parameters["@ID_Pertenencia"].Value.ToString();
            
            Parametro  parametro2 = new Parametro();
            parametro2.Nombre="@ID_Emisor";
            parametro2.ClaveTipoColectiva="EMI";
            parametro2.TipoDato=TipoDatoSQL.VARCHAR;
            parametro2.Valor = comando.Parameters["@ID_Emisor"].Value.ToString();
            
            Parametro  parametro3 = new Parametro();
            parametro3.Nombre="@ID_CadenaComercial";
            parametro3.ClaveTipoColectiva="CCM";
            parametro3.TipoDato=TipoDatoSQL.VARCHAR;
            parametro3.Valor = comando.Parameters["@ID_CadenaComercial"].Value.ToString();
            
            Parametro  parametro4 = new Parametro();
            parametro4.Nombre="@ID_GrupoCuenta";
            parametro4.ClaveTipoColectiva=null;
            parametro4.TipoDato=TipoDatoSQL.VARCHAR;
            parametro4.Valor = comando.Parameters["@ID_GrupoCuenta"].Value.ToString();
            
            Parametro  parametro5 = new Parametro();
            parametro5.Nombre="@ID_GrupoMA";
            parametro5.ClaveTipoColectiva=null;
            parametro5.TipoDato=TipoDatoSQL.VARCHAR;
            parametro5.Valor = comando.Parameters["@ID_GrupoMA"].Value.ToString();
            
            Parametro  parametro6 = new Parametro();
            parametro6.Nombre="@ID_GrupoComercial";
            parametro6.ClaveTipoColectiva="GCM";
            parametro6.TipoDato=TipoDatoSQL.VARCHAR;
            parametro6.Valor = comando.Parameters["@ID_GrupoComercial"].Value.ToString();
            
            Parametro  parametro7 = new Parametro();
            parametro7.Nombre="@ID_TarjetaHabiente";
            parametro7.ClaveTipoColectiva="CCH";
            parametro7.TipoDato=TipoDatoSQL.VARCHAR;
            parametro7.Valor = comando.Parameters["@ID_TarjetaHabiente"].Value.ToString();
            
            parametros.Add(parametro1.Nombre, parametro1);
            parametros.Add(parametro2.Nombre, parametro2);
            parametros.Add(parametro3.Nombre, parametro3);
            parametros.Add(parametro4.Nombre, parametro4);
            parametros.Add(parametro5.Nombre, parametro5);
            parametros.Add(parametro6.Nombre, parametro6);
            parametros.Add(parametro7.Nombre, parametro7);


            if (parametro1.Valor ==null || parametro1.Valor=="0")
            {
                //Levanta exception por no tener pertenencia.
                throw new GenericalException (CodRespuesta03.NO_PERTENENCIA,"[EJECUTOR] No hay Pertenencia para los valores: Medio Acceso:"+ claveMedioAcceso+",TipoMedioAcceso:"+ claveTipoMedioAcceso+", CodigoMoneda: "+
                        " Beneficiario: "+ claveBeneficiario+", ProcessingCode:"+ claveProcessingCode + ", Afiliacion:"+ claveAfiliacion);
            }
                        
        } catch (SqlException e) {
            Logueo.Error("[EJECUTOR] getPertenencia(): " + e.Message);
            throw e;
        } 
        catch (GenericalException e) {
             Logueo.Error("[EJECUTOR] getPertenencia(): " + e.Mensaje);
             throw e;
        }
        catch (Exception e) {
             Logueo.Error("[EJECUTOR] getPertenencia(): " + e.Message);
             throw e;
        }
        finally
        {
        //try {_conn.close();}catch(Exception e){}
       
        }


    }


    public void getParametrosReferidosPertenencia(int ID_TipoContrato, int ID_CadenaComercial, ref Dictionary<String, Parametro> losParametros)
    {
   
    
    //CallableStatement spEjecutor=null ;
    //ResultSet Resultado=null;
         SqlDataReader SqlReader = null;

         try {
             SqlParameter param;


             SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneParamentroReferencia", _connConsulta);
             comando.CommandType = CommandType.StoredProcedure;
             comando.Transaction = _transaccionSQL;

             param = new SqlParameter("@ID_TipoContrato", SqlDbType.BigInt);
             param.Value = ID_TipoContrato;
             comando.Parameters.Add(param);

             param = new SqlParameter("@ID_CadenaComercial", SqlDbType.BigInt);
             param.Value = ID_CadenaComercial;
             comando.Parameters.Add(param);

             SqlReader=comando.ExecuteReader();

             if (null != SqlReader)
             {

                 while (SqlReader.Read())
                 {

                    Parametro unParametro = new Parametro();
                    unParametro.Nombre=(string)SqlReader["NombreParametro"];
                    unParametro.TipoColectiva=(int)SqlReader["ID_TipoColectiva"];
                    unParametro.TipoDato=TipoDato.getTipoDatoSQL((string)SqlReader["TipoDato"]);
                    unParametro.ClaveTipoColectiva=(string)SqlReader["ClaveTipoColectiva"];
                    unParametro.Valor=(string)SqlReader["Valor"];
                    losParametros.Add((string)SqlReader["NombreParametro"],unParametro);
                 }
             }


        } catch (SqlException e) {
            Logueo.Error("[EJECUTOR]getParametrosReferidosPertenencia(): " + e.Message);
            throw e;
        }catch (Exception e) {
            Logueo.Error("[EJECUTOR]getParametrosReferidosPertenencia(): " + e.Message);
            throw e;
        }
        finally
        {
            try { SqlReader.Close();}
            catch (Exception e) { Logueo.Error("DAOUtilerias.getParametrosReferidosPertenencia().Try: " + e.Message); }
        }

    }
  
    }
}
