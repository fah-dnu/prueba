using CommonProcesador;
using Executer.Entidades;
using Interfases.Enums;
using Interfases.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class DAOPoliza
    {
        public SqlConnection _connConsulta;
        public SqlTransaction _transaccionSQL;

        public DAOPoliza(SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {
            _connConsulta = connConsulta;
            _transaccionSQL = transaccionSQL;
        }

        public List<ScriptContable> getScriptsContables(int idEvento)
        {


            SqlDataReader SqlReader = null;
            List<ScriptContable> losScripts = new List<ScriptContable>();

            int cont = 0;

            try
            {
                SqlParameter param;


                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneScriptsEvento", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;


                param = new SqlParameter("@ID_Evento", SqlDbType.BigInt);
                param.Value = idEvento;
                comando.Parameters.Add(param);

                SqlReader = comando.ExecuteReader();

                if (null != SqlReader)
                {

                    while (SqlReader.Read())
                    {

                        ScriptContable unScript = new ScriptContable();

                        unScript.ID_TipoColectiva = (int)SqlReader["ID_TipoColectiva"];
                        unScript.ID_Divisa = (int)SqlReader["ID_Divisa"];
                        unScript.ID_TipoCuenta = (int)SqlReader["ID_TipoCuenta"];
                        unScript.Formula = (string)SqlReader["Formula"];
                        unScript.esAbono = (Boolean)SqlReader["EsAbono"];
                        unScript.GeneraDetalle = (Boolean)SqlReader["GeneraDetalle"];
                        unScript.ValidaSaldo = (Boolean)SqlReader["ValidaSaldo"];

                        losScripts.Add(unScript);

                    }
                }

                if (losScripts.Count == 0)
                {
                    throw new GenericalException(CodRespuesta03.NO_HAY_SCRIPTS_CONTABLES, "EJECUTOR: El Evento no tiene configurados Scripts Contables");
                }


            }
            catch (SqlException e)
            {
                Logueo.Error("[EJECUTOR]getScriptsContables(): " + e.Message);
                throw e;
            }
            catch (GenericalException e)
            {
                Logueo.Error("[EJECUTOR]getScriptsContables(): " + e.Mensaje);
                throw e;
            }
            catch (Exception e)
            {
                Logueo.Error("[EJECUTOR]getScriptsContables(): " + e.Message);
                throw e;
            }
            finally
            {
                try { SqlReader.Close(); }
                catch (Exception e) { Logueo.Error("DAOUtilerias.getScriptsContables().Try: " + e.Message); }
            }

            return losScripts;

        }

        private Poliza insertaEncabezadoPoliza(ref Poliza laPoliza, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {

            int idPoliza = -1;
            // CallableStatement spEjecutor = null;
            try
            {

                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("EJECUTOR_InsertaPolizaContable", connOperacion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;

                param = new SqlParameter("@ID_Operacion", SqlDbType.BigInt);
                param.Value = laPoliza.ID_Operacion;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Concepto", SqlDbType.VarChar);
                param.Value = laPoliza.Concepto;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Id_Evento", SqlDbType.BigInt);
                param.Value = laPoliza.ID_Evento;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Importe", SqlDbType.Money);
                param.Value = laPoliza.Importe;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Cancela", SqlDbType.Bit);
                param.Value = laPoliza.Cancelacion;
                comando.Parameters.Add(param);

                param = new SqlParameter("@DBUser", SqlDbType.VarChar);
                param.Value = "";
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Poliza", SqlDbType.BigInt);
                param.Value = 0;
                param.Direction = ParameterDirection.Output;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ReferenciaNumerica", SqlDbType.BigInt);
                param.Value = laPoliza.ReferenciaNumerica;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Observaciones", SqlDbType.VarChar);
                param.Value = laPoliza.Observaciones;
                comando.Parameters.Add(param);

                //resp = database.ExecuteNonQuery(command);
                //comando.CommandTimeout = 5;

                comando.ExecuteNonQuery();

                idPoliza = Convert.ToInt32(comando.Parameters["@ID_Poliza"].Value);

                if (idPoliza == 0)
                {
                    throw new GenericalException(CodRespuesta03.CLAVE_DE_EVENTO_INVALIDA, "La Clave del Evento no existe en la Base de Datos.");
                }

                laPoliza.ID_Poliza = idPoliza;

                return laPoliza;


            }
            catch (SqlException err)
            {
                throw err;
            }

        }

        private Poliza insertaDetallesPoliza(ref Poliza laPoliza, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {

            int resp = -1;
            String Desc = "";

            try
            {

                foreach (DetallePoliza elDetalle in laPoliza.Detalles)
                {

                    SqlParameter param = null;

                    SqlCommand comando = new SqlCommand("EJECUTOR_InsertaDetallePolizaContable", connOperacion);
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Transaction = transaccionSQL;

                    param = new SqlParameter("@ID_Colectiva", SqlDbType.BigInt);
                    param.Value = elDetalle.ID_Colectiva;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@ID_Poliza", SqlDbType.BigInt);
                    param.Value = laPoliza.ID_Poliza;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@ID_TipoColectiva", SqlDbType.BigInt);
                    param.Value = elDetalle.Script.ID_TipoColectiva;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@ID_CadenaComercial", SqlDbType.BigInt);
                    param.Value = laPoliza.ID_CadenaComercial;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@ID_TipoCuenta", SqlDbType.BigInt);
                    param.Value = elDetalle.ID_TipoCuenta;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@ID_Divisa", SqlDbType.BigInt);
                    param.Value = elDetalle.Script.ID_Divisa;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@ValidaSaldo", SqlDbType.Bit);
                    param.Value = elDetalle.Script.ValidaSaldo;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@GeneraDetalle", SqlDbType.Bit);
                    param.Value = elDetalle.Script.GeneraDetalle;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@EsAbono", SqlDbType.Bit);
                    param.Value = elDetalle.Script.esAbono;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@Cargo", SqlDbType.Money);
                    param.Value = elDetalle.Cargo;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@Abono", SqlDbType.Money);
                    param.Value = elDetalle.Abono;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@Respuesta", SqlDbType.Int);
                    param.Value = -1;
                    param.Direction = ParameterDirection.Output;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@Descripcion", SqlDbType.VarChar, 500);
                    param.Value = "-1";
                    param.Direction = ParameterDirection.Output;
                    comando.Parameters.Add(param);


                    //resp = database.ExecuteNonQuery(command);
                    // comando.CommandTimeout = 5;

                    comando.ExecuteNonQuery();

                    resp = Convert.ToInt32(comando.Parameters["@respuesta"].Value);
                    Desc = comando.Parameters["@Descripcion"].Value.ToString();


                    switch (resp)
                    {
                        case 0:
                            break;
                        default:
                            // gpd Logueo.Error("EJECUTOR: insertaDetallesPoliza(): No se pudo insertar Detalle:[" + resp + "]");
                            throw new GenericalException((CodRespuesta03)resp, Desc);
                    }
                }

                laPoliza.isProcesada = true;


            }
            catch (GenericalException er)
            {
                Logueo.Error("EJECUTOR: insertaDetallesPoliza():" + er.Mensaje);
                throw er;
            }
            catch (Exception er)
            {
                Logueo.Error("EJECUTOR: insertaDetallesPoliza():" + er.Message);
                throw er;
            }
            return laPoliza;
        }

        //Regresa el codigo de Error al intentar insertar la poliza en el
        public int guardarPoliza(ref Poliza laPoliza, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {
            int resp = 0;
            StringBuilder saldos = new StringBuilder();
            try
            {
                Poliza respPoliza = insertaEncabezadoPoliza(ref laPoliza, connOperacion, transaccionSQL);


                if ((respPoliza.ID_Poliza == 0) || (respPoliza.ID_Poliza == -1))
                {
                    throw new GenericalException(CodRespuesta03.NO_SE_GENERO_POLIZA, "EJECUTOR: DaoPoliza.guardarPoliza(). No se pudo generar la Poliza en la Base de Datos");
                }

                laPoliza = insertaDetallesPoliza(ref laPoliza, connOperacion, transaccionSQL);

                if (!laPoliza.isProcesada)
                {
                    throw new GenericalException(CodRespuesta03.NO_SE_GENERO_POLIZA, "EJECUTOR: DaoPoliza.guardarPoliza(). No se pudo guardar los detalles de la Poliza en la Base de Datos");
                }

                //   laPoliza.Saldos = ObtieneSaldos(laPoliza.ID_Poliza, connOperacion);


            }
            catch (SqlException ex)
            {
                Logueo.Error("EJECUTOR:" + ex.Message);
                return (int)CodRespuesta03.DATABASE_ERROR;
            }
            catch (GenericalException ex)
            {
                Logueo.Error("EJECUTOR:" + ex.Mensaje);
                return ex.CodigoError;
            }
            catch (Exception ex)
            {
                Logueo.Error("EJECUTOR:" + ex.Message);
                return (int)CodRespuesta03.OTHER_ERROR;
            }

            return 0;
        }

        private List<Saldo> ObtieneSaldos_(int idPoliza, SqlConnection connConsulta)
        {

            SqlDataReader rsSaldos = null;
            //CallableStatement spEjecutor = null;
            List<Saldo> saldos = new List<Saldo>();

            try
            {

                SqlParameter param;


                SqlCommand comando = new SqlCommand("EJECUTOR_ObtieneSaldosColectivas", _connConsulta);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = _transaccionSQL;


                param = new SqlParameter("@ID_Poliza", SqlDbType.BigInt);
                param.Value = idPoliza;
                comando.Parameters.Add(param);

                rsSaldos = comando.ExecuteReader();

                if (null != rsSaldos)
                {

                    while (rsSaldos.Read())
                    {

                        Saldo elSaldo = new Saldo();
                        elSaldo.Monto = rsSaldos["SaldoDisponible"].ToString();
                        elSaldo.CreditoDebito = rsSaldos["DebitoCredito"].ToString();
                        elSaldo.TipoCuenta = rsSaldos["TipoCuentaISO"].ToString();
                        elSaldo.TipoMonto = rsSaldos["TipoMonto"].ToString();
                        elSaldo.PosicionesDecimales = rsSaldos["Decimales"].ToString();
                        elSaldo.CodigoMoneda = (string)rsSaldos["CodigoMoneda"];
                        elSaldo.ID_Colectiva = (int)rsSaldos["ID_Colectiva"];
                        elSaldo.ID_Cuenta = int.Parse(rsSaldos["ID_Cuenta"].ToString());
                        saldos.Add(elSaldo);
                        elSaldo = null;
                    }

                }


            }
            catch (SqlException er)
            {
                Logueo.Error("EJECUTOR:[ID_Poliza:" + idPoliza + "] insertaSaldosEnPoliza():" + er.Message);
                throw er;
            }
            catch (Exception er)
            {
                Logueo.Error("EJECUTOR: [ID_Poliza:" + idPoliza + "] insertaSaldosEnPoliza():" + er.Message);
                throw er;
            }
            finally
            {
                try { rsSaldos.Close(); }
                catch (Exception erro) { Logueo.Error(erro.Message); }
            }
            return saldos;
        }



        public bool insertaLotePoliza(Int32 ID_Poliza, Int64 ID_lote, Int64 ID_EventoAgrupado, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {

            int idPoliza = -1;
            // CallableStatement spEjecutor = null;
            try
            {

                SqlParameter param = null;

                SqlCommand comando = new SqlCommand("EJECUTOR_InsertaLotePoliza", connOperacion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;

                param = new SqlParameter("@ID_Poliza", SqlDbType.BigInt);
                param.Value = ID_Poliza;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_Corte", SqlDbType.BigInt);
                param.Value = ID_lote;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ID_EventoAgrupado", SqlDbType.BigInt);
                param.Value = ID_EventoAgrupado;
                comando.Parameters.Add(param);


                comando.ExecuteNonQuery();


                return true;


            }
            catch (SqlException err)
            {
                throw err;
            }

        }


    }
}
