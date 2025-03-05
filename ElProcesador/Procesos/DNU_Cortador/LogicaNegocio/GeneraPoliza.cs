using CommonProcesador;
using DNU_Cortador.BaseDatos;
using DNU_Cortador.Entidades;
using Executer.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

namespace DNU_Cortador.LogicaNegocio
{
    class GeneraPoliza
    {
        static Thread unTimer;
        static SqlConnection conn = new SqlConnection(BDCortes.strBDEscritura);


        public bool ProcesarCorte(Corte unCorte)
        {

            try
            {

                Dictionary<String, Parametro> TodosLosParametros = DAOEvento.ListaDeParamentrosContrato(unCorte.ID_CadenaComercial.ToString(), unCorte.Tag, unCorte.ClaveEvento, "Procesador");


                TodosLosParametros["@ID_CuentaHabiente"] = new Parametro() { Nombre = "@ID_CuentaHabiente", Valor = unCorte.ID_Colectiva.ToString(), EsClave = false, ID_TipoColectiva = (unCorte.ID_TipoColectivaCuentaTelevia), Descripcion = "ID_CuentaHabiente" };
                TodosLosParametros["@ID_CadenaComercial"] = new Parametro() { Nombre = "@ID_CadenaComercial", Valor = unCorte.ID_CadenaComercial.ToString(), EsClave = false, ID_TipoColectiva = (6), Descripcion = "ID_CadenaComercial" };
                TodosLosParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = unCorte.Importe.ToString(), Descripcion = "Importe" };




                Logueo.EntradaSalida("El Corte: " + unCorte.ToString(), "", true);

                Respuesta unaRespo = EjecutarEvento(unCorte.ID_Evento, unCorte.ID_Contrato, 0, unCorte.ID_Colectiva.ToString(), unCorte.Tag, unCorte.DescripcionEvento, unCorte.Observaciones, Int64.Parse(unCorte.Consecutivo), TodosLosParametros, unCorte);

                Logueo.EntradaSalida("Respuesta de El Corte:" + unaRespo.ToString(), "", false);

                return true;
            }
            catch (Exception ex)
            {

                Logueo.Error("Generacion de Poliza: ID_Corte:" + unCorte.ID_Corte + ", TAG: " + unCorte.Tag + "." + ex.ToString());

                return false;
            }

        }



        public Respuesta EjecutarEvento(int ID_Evento, int ID_Contrato, int DiasVigencia, String IDCliente, String Tarjeta, String Concepto, String Observaciones, Int64 Consecutivo, Dictionary<String, Parametro> TodosLosParametros, Corte unCorte)
        {

            Respuesta unaRespo2 = new Respuesta();

            DAOCortes UnBDCorte = new DAOCortes();


            unaRespo2.Tarjeta = Tarjeta;

            try
            {


                int Resp = -1;

                try
                {
                    conn.Open();

                }
                catch (Exception err)
                {
                    // Logueo.Error("AplicarMovimiento.LNEventos.EjecuarEvento.AbrirConexion()" + err.Message);
                }

                using (SqlTransaction transaccionSQL = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {

                    try
                    {
                        Poliza laPoliza = null;


                        //Genera y Aplica la Poliza
                        Executer.EventoManual aplicador = new Executer.EventoManual(ID_Evento, ID_Contrato, Concepto, false, TodosLosParametros, conn, transaccionSQL, Observaciones, Consecutivo);
                        laPoliza = aplicador.AplicaContablilidad();


                        if (laPoliza.ID_Poliza <= 0)
                        {
                            transaccionSQL.Rollback();
                            throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                        }

                        //Guardar el Lote
                        if (laPoliza.ID_Poliza <= 0)
                        {
                            throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                        }
                        else
                        {

                            //Se Actualiza la Poliza en el Corte
                            Logueo.EntradaSalida("Actualiando la Poliza [" + laPoliza.ID_Poliza + "] del Corte [" + unCorte.ID_Corte + "]", "", true);

                            UnBDCorte.Televia_ActualizaPolizaEnCorte(unCorte.ID_Corte, laPoliza.ID_Poliza, conn, transaccionSQL);

                            Logueo.EntradaSalida("Finaliza Actualizacion la Poliza [" + laPoliza.ID_Poliza + "] del Corte [" + unCorte.ID_Corte + "] finalizada con Exito", "", false);




                            Logueo.EntradaSalida("Insertar Periodo anterior [" + laPoliza.ID_Poliza + "] del Corte [" + unCorte.ID_Corte + "]", "", true);

                            UnBDCorte.Televia_ActualizaPeriodoAnteriorEnComplemento(unCorte.ID_Corte, unCorte.Tag, conn, transaccionSQL);

                            Logueo.EntradaSalida("Finalzia Insertar Encabezado [" + laPoliza.ID_Poliza + "] del Corte [" + unCorte.ID_Corte + "] finalizada con Exito", "", false);



                            unaRespo2.CodigoRespuesta = 0;
                            unaRespo2.Autorizacion = DateTime.Now.ToString("HHmmss");
                            //unaRespo2.Descripcion = "AUTORIZADA
                            unaRespo2.ID_Poliza = laPoliza.ID_Poliza;
                            unaRespo2.Descripcion = "AUTORIZADA " + (TodosLosParametros.ContainsKey("@DescEvento") ? TodosLosParametros["@DescEvento"].Valor : "").ToUpper();
                            transaccionSQL.Commit();

                        }

                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        Logueo.Error("AplicarMovimiento.LNEventos.EjecuarEvento()" + err.Message);
                        unaRespo2.CodigoRespuesta = 87;
                        unaRespo2.Autorizacion = "000000";
                        //unaRespo2.Descripcion = "DECLINADO";
                        unaRespo2.Descripcion = "DECLINADA " + (TodosLosParametros.ContainsKey("@DescEvento") ? TodosLosParametros["@DescEvento"].Valor : "").ToUpper();

                    }
                }

            }


            catch (Exception err)
            {
                Logueo.Error("Cortador.LNEventos.EjecuarEvento()" + err.Message);
                unaRespo2.CodigoRespuesta = 87;
                unaRespo2.Autorizacion = "000000";
                unaRespo2.Descripcion = "DECLINADO";

            }

            return unaRespo2;
        }


    }
}
