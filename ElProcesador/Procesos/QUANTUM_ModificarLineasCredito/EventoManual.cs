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
using QUANTUM_ModificarLineasCredito.BaseDatos;
using CommonProcesador;

namespace Executer
{
    public class EventoManual
    {
        Dictionary<String, Parametro> _parametros;
        Int32 _ID_Evento;
        Boolean _Cancela;
        SqlConnection _conn;
        SqlTransaction _transaccionSQL;

        int _IDOperacion;
        String _Descripcion;
        float _Importe;
        String _claveCodigoMoneda;
        String _Observaciones;
        Int64 _RefNumerica;
        int _ID_CadenaComercial;
        int _ID_TipoContrato;
        public Poliza PolizaFinal { get; set; }




        public EventoManual(int ID_Evento, int ID_contrato, String Descripcion,  Boolean Cancela,
            Dictionary<String, Parametro> parametros, SqlConnection conn, SqlTransaction transaccionSQL, String Observaciones, Int64 ReferenciaNumerica)
        {
            //Validar los valores recibidos.
            try
            {
                if (conn == null)
                {
                    throw new GenericalException(CodRespuesta03.DATABASE_ERROR, "La Conexión a la Base de Datos es nula");
                }

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    throw new GenericalException(CodRespuesta03.DATABASE_ERROR, "La Conexión a la Base de Datos esta Cerrada");
                }


                DAOUtilerias util = new DAOUtilerias(conn, transaccionSQL);

                this._Descripcion = Descripcion;
                this._parametros = parametros;
                this._Importe = float.Parse(_parametros["@Importe"].Valor);
                this._Cancela = Cancela;
                this._ID_Evento = ID_Evento;
                this._conn = conn;
                this._transaccionSQL = transaccionSQL;
                this._RefNumerica = ReferenciaNumerica;
                this._Observaciones = Observaciones;

                Logueo.Evento("[EJECUTOR] Parametros de entrada: " + parametros.ToString());

            }
            catch (GenericalException err)
            {
                Logueo.Error("EJECUTOR:EventoManual(): " + err.Mensaje);
                throw err;
            }
            catch (SqlException err)
            {
                Logueo.Error("EJECUTOR:EventoManual(): " + err.Message);
                throw err;
            }
            catch (Exception err)
            {
                Logueo.Error("EJECUTOR:EventoManual(): " + err.Message);
                throw err;
            }
        }



        public int AplicaContablilidad()
        {
            int resp = -1;
            Poliza laPoliza = new Poliza();

            try
            {

                //Valida las reglas que se generar
                //Ejecusion de Reglas en SPs
                DAORegla DBRegla = new DAORegla(_conn, _transaccionSQL);
                DAOEvento BDEvento = new DAOEvento(_conn, _transaccionSQL);
                int respRegla = -1;

                Logueo.Evento("[EventoManual] Se ha iniciado la construccion de una Poliza por medio de Ejecutor Contable ");
                //Logueo.Evento("[EventoManual] Inicia la Obtencion de Reglas");
                //// Iterator lasReglas = DBRegla.getReglasEvento(BDEvento.getIDEvento(_claveEvento), Integer.parseInt(_parametros.get("@ID_Pertenencia").getValor()), _parametros).iterator();
                //Logueo.Evento("[EventoManual] Inicia la ejecucion de Reglas");

                //foreach (Regla execRegla in DBRegla.getReglasEvento(_ID_Evento, this._ID_CadenaComercial, this._ID_TipoContrato, _parametros))
                //{
                //    respRegla = DBRegla.ejecutaRegla(execRegla, _conn, _transaccionSQL);
                //    if (respRegla != 0)
                //    {
                //        Logueo.Evento("[EventoManual] La regla [" + execRegla.Nombre + "] no fue autorizada: Respuesta: " + respRegla);
                //        return respRegla;
                //    }
                //}
                //Logueo.Evento("[EventoManual] Ejecucion de Reglas terminada");
                //Generamos la Poliza a partur de los datos recibidos.
                laPoliza = new LNPoliza().generaPolizaManual(_Importe, _Descripcion, _ID_Evento, _Cancela, _parametros, _conn, _transaccionSQL,_Observaciones, _RefNumerica);
                Logueo.Evento("[EventoManual] Iniciando la construccion de la Poliza : " + _Descripcion + ", con el Evento: " + _ID_Evento + ", de un Monto:" + _Importe);

                //Se almacena la Poliza en la Base de Datos.
                resp = new DAOPoliza(_conn, _transaccionSQL).guardarPoliza(ref laPoliza, _conn, _transaccionSQL);


                PolizaFinal = laPoliza;

                Logueo.Evento("[EventoManual] Se ha generado una Poliza correctamente con el Identificador: " + laPoliza.ID_Poliza);



            }
            catch (GenericalException err)
            {
                Logueo.Error("EJECUTOR:EventoManual(): " + err.Mensaje);
                return err.CodigoError;
            }
            catch (SqlException err)
            {
                Logueo.Error("EJECUTOR:EventoManual(): " + err.Message);
                return (int)CodRespuesta03.DATABASE_ERROR;
            }
            catch (Exception err)
            {
                Logueo.Error("EJECUTOR:EventoManual(): " + err.Message);
                return (int)CodRespuesta03.OTHER_ERROR;
            }

            Logueo.Evento("[EJECUTOR] Poliza Generada con Exito [ID_Poliza]:" + laPoliza.ID_Poliza + "; [Descripcion]:" + laPoliza.Concepto);

            //Se genero y almaceno correctamente la poliza
            return resp;
        }
    }
}