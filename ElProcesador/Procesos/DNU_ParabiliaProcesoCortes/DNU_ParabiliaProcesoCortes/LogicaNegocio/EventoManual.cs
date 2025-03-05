using CommonProcesador;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.Utilidades;
using Executer.Entidades;
using Interfases.Entidades;
using Interfases.Enums;
using Interfases.Exceptions;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.CapaNegocio
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




        public EventoManual(int ID_Evento, String Descripcion, Boolean Cancela,
            Dictionary<String, Parametro> parametros, SqlConnection conn, SqlTransaction transaccionSQL, String Observaciones, Int64 ReferenciaNumerica)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            //Validar los valores recibidos.
            try
            {
                if (conn == null)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + CodRespuesta03.DATABASE_ERROR + "La Conexión a la Base de Datos es nula]");
                    throw new GenericalException(CodRespuesta03.DATABASE_ERROR, "La Conexión a la Base de Datos es nula");
                }

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + CodRespuesta03.DATABASE_ERROR + "La Conexión a la Base de Datos esta Cerrada]");
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

                // for GPD Logueo.Evento("[EJECUTOR] Parametros de entrada: " + parametros.ToString());

            }
            catch (GenericalException err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:EventoManual(): " + err.Mensaje + "]");
                throw err;
            }
            catch (SqlException err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:EventoManual(): " + err.Message + "]");
                throw err;
            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:EventoManual(): " + err.Message + "]");
               throw err;
            }
        }



        public Poliza AplicaContablilidad()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            int resp = -1;
            Poliza laPoliza = new Poliza();

            try
            {

                //Valida las reglas que se generar
                //Ejecusion de Reglas en SPs
                DAORegla DBRegla = new DAORegla(_conn, _transaccionSQL);
                DAOEvento BDEvento = new DAOEvento(_conn, _transaccionSQL);
                int respRegla = -1;

                // GPD Logueo.Evento("[EventoManual] Se ha iniciado la construccion de una Poliza por medio de Ejecutor Contable ");
                //Logueo.Evento("[EventoManual] Inicia la Obtencion de Reglas");
                List<Executer.Entidades.Regla> lasReglas = DBRegla.getReglasEvento(_ID_Evento, 0, 0, _parametros);
              
                foreach (Executer.Entidades.Regla execRegla in DBRegla.getReglasEvento(_ID_Evento, this._ID_CadenaComercial, this._ID_TipoContrato, _parametros))
                {
                    respRegla = DBRegla.ejecutaRegla(execRegla, _conn, _transaccionSQL, ref _parametros);
                    if (respRegla != 0)
                    {
                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EventoManual, La regla " + execRegla.Nombre + " no fue autorizada: Respuesta: " + respRegla + "]");
                        throw new Exception("LA REGLA " + execRegla.Nombre + ", NO FUE AUTORIZADA:" + respRegla);
                    }
                }
                //Generamos la Poliza a partur de los datos recibidos.
                laPoliza = new LNPoliza().generaPoliza(0, _Importe, _Descripcion, _ID_Evento.ToString(), _Cancela, _parametros, _conn, _transaccionSQL);
                // GPD Logueo.Evento("[EventoManual] Iniciando la construccion de la Poliza : " + _Descripcion + ", con el Evento: " + _ID_Evento + ", de un Monto:" + _Importe);

                //GPD  if (laPoliza.ImportePremio != 0)
                // {
                //   laPoliza.Importe = laPoliza.ImportePremio;
                //GPD  }
                //Se almacena la Poliza en la Base de Datos.
                resp = new DAOPoliza(_conn, _transaccionSQL).guardarPoliza(ref laPoliza, _conn, _transaccionSQL);


                PolizaFinal = laPoliza;


                if (resp != 0)
                {
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + resp + ": NO SE GENERO NINGUNA POLIZA" + "]");
                    throw new Exception(resp + ": NO SE GENERO NINGUNA POLIZA");
                }

                // GPD  Logueo.Evento("[EventoManual] Se ha generado una Poliza correctamente con el Identificador: " + laPoliza.ID_Poliza);



            }
            catch (GenericalException err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:EventoManual(): " + err.Mensaje + "]");
                throw err;
                //return err.CodigoError;
            }
            catch (SqlException err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:EventoManual(): " + err.Message + "]");
                throw err;
                //return (int)CodRespuesta03.DATABASE_ERROR;
            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR:EventoManual(): " + err.Message + "]");
                throw err;
                //return (int)CodRespuesta03.OTHER_ERROR;
            }
            try
            {
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, Poliza Generada con Exito ID_Poliza:" + laPoliza.ID_Poliza + "; Descripcion:" + laPoliza.Concepto + "]");
             }
            catch (Exception err)
            {

            }
            //Se genero y almaceno correctamente la poliza
            return laPoliza;
        }
    }
}