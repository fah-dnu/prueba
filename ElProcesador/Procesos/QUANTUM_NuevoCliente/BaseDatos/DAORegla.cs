using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Executer.Entidades;
using Interfases.Exceptions;
using Interfases.Enums;
//using Executer.Utilidades;
using System.Data;
using Interfases.Entidades;
using CommonProcesador;

namespace Executer.BaseDatos
{


    class DAORegla
    {

         //private Connection _conn= new Conexion().getConexion();
    SqlConnection _connConsulta;
    SqlTransaction _transaccionSQL;

    public DAORegla(SqlConnection connConsulta, SqlTransaction transaccionSQL) {
        _connConsulta = connConsulta;
        _transaccionSQL = transaccionSQL;
    }

    public int ejecutaRegla(Regla regla, SqlConnection connOperacion, SqlTransaction transaccionSQL)
    {
        int codRespuesta = -1;
        String desRespuesta="";

        int resp=-1;
        SqlParameter param;
        //Si la regla no se ejecuto regresa True.
        try {


            SqlCommand comando = new SqlCommand(regla.StoredProcedure, _connConsulta);
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandTimeout = 10;
            comando.Transaction = transaccionSQL;


            foreach (Parametro valorParametro in regla.Parametros)
                {

                //spEjecutor.s("hola", valorParametro.getNombre());
                TipoDatoSQL dato = valorParametro.TipoDato;

                switch (dato) {
                    case TipoDatoSQL.VARCHAR:
                    case TipoDatoSQL.DATETIME:
                    case TipoDatoSQL.CHAR:

                        param = new SqlParameter(valorParametro.Nombre, SqlDbType.VarChar);
                        param.Value = valorParametro.Valor;
                        comando.Parameters.Add(param);
                        break;
                    case TipoDatoSQL.INT:
                       
                        param = new SqlParameter(valorParametro.Nombre, SqlDbType.Int);
                        param.Value = valorParametro.Valor;
                        comando.Parameters.Add(param);
                        break;
                    case TipoDatoSQL.BIGINT:
                       
                        param = new SqlParameter(valorParametro.Nombre, SqlDbType.BigInt);
                        param.Value = valorParametro.Valor;
                        comando.Parameters.Add(param);
                        break;
                    case TipoDatoSQL.BIT:
                        
                        param = new SqlParameter(valorParametro.Nombre, SqlDbType.Bit);
                        param.Value = valorParametro.Valor;
                        comando.Parameters.Add(param);
                        break;
                    case TipoDatoSQL.DECIMAL:
                    case TipoDatoSQL.DOUBLE:
                        
                        param = new SqlParameter(valorParametro.Nombre, SqlDbType.Decimal);
                        param.Value = valorParametro.Valor;
                        comando.Parameters.Add(param);
                        break;
                    case TipoDatoSQL.FLOAT:
                       
                        param = new SqlParameter(valorParametro.Nombre, SqlDbType.Float);
                        param.Value = valorParametro.Valor;
                        comando.Parameters.Add(param);
                        break;
                    case TipoDatoSQL.TINYINT:
                        
                         param = new SqlParameter(valorParametro.Nombre, SqlDbType.TinyInt);
                        param.Value = valorParametro.Valor;
                        comando.Parameters.Add(param);
                        break;
                    case TipoDatoSQL.SMALLINT:
                       
                         param = new SqlParameter(valorParametro.Nombre, SqlDbType.SmallInt);
                        param.Value = valorParametro.Valor;
                        comando.Parameters.Add(param);
                        break;
                    default:
                        String msjError = "No hay Mapeo de Tipo de Dato : " + valorParametro.Nombre + " en la Regla " + regla.Nombre + ".";
                        Logueo.Error(msjError);
                        break;

                }
            }


                param = new SqlParameter("@Respuesta", SqlDbType.BigInt);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Descripcion", SqlDbType.VarChar);
                param.Value = "";
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                //Todas las reglas deben tener un parametro de Salida Llamado @Respuesta

                comando.ExecuteNonQuery();

                codRespuesta = Int32.Parse(comando.Parameters["@Respuesta"].Value.ToString());
                desRespuesta = comando.Parameters["@Descripcion"].Value.ToString();
            
            
            //Asigna el codigo de respuesta despues de ejecutar la regla
            if (codRespuesta!=0)
            {
                throw new GenericalException(codRespuesta,desRespuesta);
            }
            else
            {
                //quiere decir que las reglas aprobaron el mensaje
               resp= (int)CodRespuesta03.APPROVED;
            }

        } catch (SqlException ex) {
            Logueo.Error("[EJECUTOR] ejecutaRegla():SQLException "+ex.Message);
           resp= (int)CodRespuesta03.DATABASE_ERROR;
        }
        catch (GenericalException ex)
        {
            Logueo.Error("[[EJECUTOR]] ejecutaRegla():"+ex.Mensaje);
            resp=ex.CodigoError;
        }
        catch (Exception ex) {
            Logueo.Error("[EJECUTOR] ejecutaRegla():"+ex.Message);
            resp=(int)CodRespuesta03.OTHER_ERROR;
        }

        return resp;
    }

    public List<Regla> getReglasEvento(int idEvento, int ID_CadenaComercial, int ID_TipoContrato, Dictionary<String, Parametro> losParametros)
    {


        //String stored = new String();
        List<Regla> reglas = new List<Regla>();
        List<Parametro> Parametros;
        DAOUtilerias obtenParametros = new DAOUtilerias(_connConsulta,_transaccionSQL);
        SqlDataReader SqlReader = null;
        

        //Obtiene los valores de los parametros tanto de referencia como Operativos a partir del Mensaje ISO
        obtenParametros.getParametrosReferidosPertenencia(ID_TipoContrato,ID_CadenaComercial, ref losParametros);

        Boolean accion = true;
        String nomRegla = "", storedProc = "";
        int ejecucion = 1, cont = 0;

        //CallableStatement spEjecutor = null;
        //ResultSet Resultado=null;

        try
        {


            Parametros = new List<Parametro>();
            SqlParameter param;
            SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneReglasDeEvento", _connConsulta);
            comando.Transaction = _transaccionSQL;
            comando.CommandType = CommandType.StoredProcedure;

            //param = new SqlParameter("@ID_Pertenencia", SqlDbType.BigInt);
            //param.Value = ID_Pertenencia;
            //comando.Parameters.Add(param);

            param = new SqlParameter("@ID_Evento", SqlDbType.BigInt);
            param.Value = idEvento;
            comando.Parameters.Add(param);



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
                        reglas.Add(spRegla);
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
                        valor = losParametros[(string)SqlReader["NombreParametro"]].Valor;

                    }
                    catch (Exception err)
                    {
                        throw new GenericalException(CodRespuesta03.FALTA_CONFIGURAR_UN_PARAMETRO, "El parametro" + NombreParam + " no tiene un valor definido en la Operacion");
                    }
                    parametro.Valor = valor;
                    Parametros.Add(parametro);


                    Parametro unParametro = new Parametro();
                    unParametro.Nombre = (string)SqlReader["NombreParametro"];
                    unParametro.TipoColectiva = (int)SqlReader["ID_TipoColectiva"];
                    unParametro.TipoDato = TipoDato.getTipoDatoSQL((string)SqlReader["TipoDato"]);
                    unParametro.ClaveTipoColectiva = (string)SqlReader["ClaveTipoColectiva"];
                    unParametro.Valor = (string)SqlReader["Valor"];
                    losParametros.Add((string)SqlReader["NombreParametro"], unParametro);
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
                reglas.Add(spRegla);
            }


        }
        catch (SqlException ex)
        {
            Logueo.Error("[EJECUTOR] getReglasOperacion(): " + ex.Message);
            throw ex;
        }
        catch (GenericalException err)
        {
            Logueo.Error("DaoEvento.getIDEvento():" + err.Mensaje);
            throw err;
        }
        catch (Exception ex)
        {
            Logueo.Error("[EJECUTOR] getReglasOperacion(): " + ex.Message);
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

        return reglas;
    }


    }
}
