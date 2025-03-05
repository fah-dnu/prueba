using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Executer.Entidades;
using Interfases.Exceptions;
using Executer.BaseDatos;
//using Executer.Utilidades;
using Executer.LogicaNegocio;
using Interfases.Enums;
using Interfases;
using Interfases.Entidades;
using QUANTUM_NuevoCliente.BaseDatos;
using CommonProcesador;

namespace Executer
{
    public class Ejecutor
    {
        Dictionary<String, Parametro> _parametros;
         String _claveEvento;
         Boolean _Cancela;
         SqlConnection _conn;
         SqlTransaction _transaccionSQL;

         int _IDOperacion;
         String _Descripcion;
         float _Importe;
         String _claveCodigoMoneda;
         int _ID_CadenaComercial;
         int _ID_TipoContrato;
         public Poliza PolizaFinal  { get; set; }
    



    public Ejecutor(int IDOperacion, int ID_CadenaComercial,int ID_CuentaHabiente,int IdTipocontrato, String Descripcion, String claveEvento, Boolean Cancela,
        Dictionary<String, Parametro> parametros, SqlConnection conn, SqlTransaction transaccionSQL) 
    {
        //Validar los valores recibidos.
        try {
            if (conn == null) {
                throw new GenericalException(CodRespuesta03.DATABASE_ERROR, "La Conexión a la Base de Datos es nula");
            }

            if (conn.State== System.Data.ConnectionState.Closed) {
                throw new GenericalException(CodRespuesta03.DATABASE_ERROR, "La Conexión a la Base de Datos esta Cerrada");
            }

          

            //if (Importe == 0) {
            //    throw new GenericalException(CodRespuesta03.INVALID_AMOUNT, "El Importe para generar la Póliza debe ser Mayor a cero.");
            //}

            if (Descripcion.Length==0 || Descripcion.Trim().Length==0) {
                throw new GenericalException(CodRespuesta03.OTHER_ERROR, "Se requiere una descripcion para la generación de la Póliza");
            }

            if (claveEvento.Length==0 || claveEvento.Trim().Length==0) {
                throw new GenericalException(CodRespuesta03.OTHER_ERROR, "Se requiere una descripcion para la generación de la Póliza");
            }
            
            DAOUtilerias util = new DAOUtilerias(conn,transaccionSQL);
            
            
            this._IDOperacion = IDOperacion;
            this._Descripcion = Descripcion;
            this._Importe = 0;//Importe;
            this._parametros = parametros;
            this._Cancela = Cancela;
            this._claveEvento = claveEvento;
            this._conn = conn;
            this._ID_CadenaComercial = ID_CadenaComercial;
            this._ID_TipoContrato = IdTipocontrato;
            this._transaccionSQL = transaccionSQL;
            

            //Se agregan los valores al map de los paramentros. (en este Caso solo el importe se inserta en el Map
            Parametro Parametro1 = new Parametro();
            Parametro1.Nombre=("@Importe");
            Parametro1.TipoDato=(TipoDatoSQL.FLOAT);
            Parametro1.Valor=(this._Importe.ToString());
            parametros.Add("@Importe", Parametro1);

            Parametro Parametro4 = new Parametro();
            Parametro4.Nombre = ("@ID_CuentaHabiente");
            Parametro4.TipoDato = (TipoDatoSQL.VARCHAR);
            Parametro4.ClaveTipoColectiva = "CCH";
            Parametro4.Valor = ID_CuentaHabiente.ToString();
            parametros.Add("@ID_CuentaHabiente", Parametro4);

            Parametro Parametro3 = new Parametro();
            Parametro3.Nombre = ("@ID_CadenaComercial");
            Parametro3.TipoDato = (TipoDatoSQL.VARCHAR);
            Parametro3.ClaveTipoColectiva = "CCM";
            Parametro3.Valor = ID_CadenaComercial.ToString();
            parametros.Add("@ID_CadenaComercial", Parametro3);


            
            //obtenemos los parametros de la pertenencia.
            //Logueo.Evento("[EJECUTOR] Se Obtiene la pertenencia");
            //util.getIDPertenencia(claveTipoMensaje, claveMedioAcceso,   claveTipoMedioAcceso, claveBeneficiario,claveProcessingCode, claveAfiliacion, parametros);
            
            Logueo.Evento("[EJECUTOR] Se Obtiene Los valores del Contrato de la Cadena Comercial para la aplicacion de reglas y poliza");
            //util.getParametrosReferidosPertenencia(Integer.parseInt(parametros.get("@ID_Pertenencia").getValor()), parametros);
            
           Logueo.Evento("[EJECUTOR] Parametros de entrada: " +parametros.ToString());

        } catch (GenericalException err) {
            Logueo.Error("EJECUTOR:Contable(): " + err.Mensaje);
            throw err;
        } catch (SqlException err) {
            Logueo.Error("EJECUTOR:Contable(): " + err.Message);
            throw err;
        } catch (Exception err) {
            Logueo.Error("EJECUTOR:Contable(): " + err.Message);
            throw err;
        }
    }



    public int AplicaContablilidad()
    {
        int resp = -1;
        Poliza laPoliza= new Poliza();

        try
        {

            //Valida las reglas que se generar
            //Ejecusion de Reglas en SPs
            DAORegla DBRegla = new DAORegla(_conn, _transaccionSQL);
            DAOEvento BDEvento = new DAOEvento(_conn, _transaccionSQL);
            int respRegla = -1;
            Logueo.Evento("[EJECUTOR] Se ha iniciado la construccion de una Poliza por medio de Ejecutor Contable ");
            Logueo.Evento("[EJECUTOR] Inicia la Obtencion de Reglas");
           // Iterator lasReglas = DBRegla.getReglasEvento(BDEvento.getIDEvento(_claveEvento), Integer.parseInt(_parametros.get("@ID_Pertenencia").getValor()), _parametros).iterator();
            Logueo.Evento("[EJECUTOR] Inicia la ejecucion de Reglas");
            
            foreach (Regla execRegla in DBRegla.getReglasEvento(BDEvento.getIDEvento(_claveEvento),this._ID_CadenaComercial,this._ID_TipoContrato, _parametros)) 
            {
                respRegla = DBRegla.ejecutaRegla(execRegla, _conn,_transaccionSQL);
                if (respRegla != 0)
                {
                    Logueo.Evento("[EJECUTOR] La regla [" + execRegla.Nombre + "] no fue autorizada: Respuesta: " + respRegla);
                    return respRegla;
                }
            }
            Logueo.Evento("[EJECUTOR] Ejecucion de Reglas terminada");
            //Generamos la Poliza a partur de los datos recibidos.
            laPoliza = new LNPoliza().generaPoliza(_IDOperacion, _Importe, _Descripcion, _claveEvento, _Cancela, _parametros, _conn,_transaccionSQL);
            Logueo.Evento("[EJECUTOR] Iniciando la construccion de la Poliza : " + _Descripcion + ", con el Evento: " + _claveEvento + ", de un Monto:" + _Importe);

            //Se almacena la Poliza en la Base de Datos.
            resp = new DAOPoliza(_conn,_transaccionSQL).guardarPoliza(ref laPoliza, _conn,_transaccionSQL);


            PolizaFinal = laPoliza;
            
            Logueo.Evento("[EJECUTOR] Se ha generado una Poliza correctamente con el Identificador: " + laPoliza.ID_Poliza);



        }
        catch (GenericalException err)
        {
            Logueo.Error("EJECUTOR:Contable(): " + err.Mensaje);
            return err.CodigoError;
        }
        catch (SqlException err)
        {
            Logueo.Error("EJECUTOR:Contable(): " + err.Message);
            return (int)CodRespuesta03.DATABASE_ERROR;
        }
        catch (Exception err)
        {
            Logueo.Error("EJECUTOR:Contable(): " + err.Message);
            return (int)CodRespuesta03.OTHER_ERROR;
        }
       
        Logueo.Evento("[EJECUTOR] Poliza Generada con Exito [ID_Poliza]:" + laPoliza.ID_Poliza + "; [Descripcion]:" + laPoliza.Concepto );

        //Se genero y almaceno correctamente la poliza
        return resp;
    }
    }
}
