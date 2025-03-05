using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QUANTUM_ModificarLineasCredito.Entidades;
using Interfases.Entidades;
using CommonProcesador;
using System.Data;
using System.Data.SqlClient;
using QUANTUM_ModificarLineasCredito.BaseDatos;

namespace QUANTUM_ModificarLineasCredito.LogicaNegocio
{
    public class LNModificarLC
    {
        public static Respuesta ModificarLineaCredito(int  ID_Evento,  int ID_Contrato, int DiasVigencia, String IDCliente, String Tarjeta, String Concepto, String Observaciones, Int64 Consecutivo, Dictionary<String, Parametro> TodosLosParametros )
        {

            Respuesta unaRespo2 = new Respuesta();


            unaRespo2.Tarjeta = Tarjeta;
            
            try
            {


                int Resp = -1;
                //if (ID_Contrato == 0 | ID_Evento == 0)
                //{
                //    throw new Exception("No se pudo obtener un ID_Evento o Contraro valido");
                //}



                    using (SqlConnection conn = new SqlConnection(BDModificarLC.strBDEscritura))
                    {
                        conn.Open();

                        using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                        {
                            try
                            {
                                 

                                //Genera y Aplica la Poliza
                                Executer.EventoManual aplicador = new Executer.EventoManual(ID_Evento, ID_Contrato, Concepto, false, TodosLosParametros, conn, transaccionSQL, Observaciones, Consecutivo);
                                Resp = aplicador.AplicaContablilidad();

                                //Guardar el Lote
                                if (Resp != 0)
                                {
                                    throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                                }

                                
                                    //Asgina la vigencia

                                unaRespo2 = DAOEvento.AsignarVigencia(Tarjeta, DiasVigencia, "ProcesadorNocturno", conn, transaccionSQL);

                                if (unaRespo2.CodigoRespuesta == 0)
                                {
                                    unaRespo2.Autorizacion = unaRespo2.Autorizacion;
                                    unaRespo2.Saldo = unaRespo2.Saldo;
                                    unaRespo2.CodigoRespuesta = unaRespo2.CodigoRespuesta;

                                    //  lasRespuestas.Add(unaRespo);


                                    unaRespo2.CodigoRespuesta = Resp;
                                    unaRespo2.Autorizacion = DateTime.Now.ToString("HHmmss");
                                    unaRespo2.Descripcion = "AUTORIZADA " + (TodosLosParametros.ContainsKey("@DescEvento") ? TodosLosParametros["@DescEvento"].Valor : "").ToUpper(); 

                                    transaccionSQL.Commit();
                                }
                                else
                                {
                                    transaccionSQL.Rollback();
                                }
                                


                            }

                            catch (Exception err)
                            {
                                Logueo.Error("ModificarLinea.LNEventos.EjecuarEvento()" + err.Message);
                                unaRespo2.Descripcion = "DECLINADA " + (TodosLosParametros.ContainsKey("@DescEvento") ? TodosLosParametros["@DescEvento"].Valor : "").ToUpper(); 
                                unaRespo2.CodigoRespuesta = 87;
                                unaRespo2.Autorizacion = "000000";

                                transaccionSQL.Rollback();
                            }

                        }
                    }

                
            }
            catch (Exception err)
            {
                Logueo.Error("ModificarLinea.LNEventos.EjecuarEvento()" + err.Message);
                unaRespo2.CodigoRespuesta = 87;
                unaRespo2.Autorizacion = "000000";

            }

            return unaRespo2;
        }


        public static bool ModificarLineas()
        {
            List<Respuesta> lasRespuestas = new List<Respuesta>();
            try
            {

                
                String ClaveEvento = PNConfig.Get("MODIFICAR", "ClaveEvento");
                String IDTipoColectiva = PNConfig.Get("MODIFICAR", "IDTipoColectivaCCH");

                



                /*
                 *   Parametro unParametroCH = new Parametro() { Nombre = "@ID_CuentaHabiente", Valor = elCliente.ID_Colectiva.ToString(), Descripcion = "Colectiva Cliente CuentaHabiente", ID_TipoColectiva = int.Parse(PNConfig.Get("NUEVOCLNT", "IDTipoColectivaCCH")) };

                              TodosLosParametros.Add(unParametroCH.Nombre, unParametroCH);
                 */
                wsQuantumLimCred.Quantum unaOperacion = new wsQuantumLimCred.Quantum();
                Int32 Conse = 1;

                //foreach (LineaCredito laLinea in DAOEvento.ListaUsuariosParaLC())
                //{

                //    Dictionary<String, Parametro> TodosLosParametros = DAOEvento.ListaDeParamentrosContrato(ClaveCadenaComercial, laLinea.Tarjeta, ClaveEvento, "ProcesadorNocturno");

                //    int ID_Evento = TodosLosParametros.ContainsKey("@ID_Evento") ? Int32.Parse(TodosLosParametros["@ID_Evento"].Valor) : 0;
                //    int ID_Contrato = TodosLosParametros.ContainsKey("@ID_Contrato") ? Int32.Parse(TodosLosParametros["@ID_Contrato"].Valor) : 0;



                //    TodosLosParametros["@ID_CuentaHabiente"] = new Parametro() { Nombre = "@ID_CuentaHabiente", Valor = laLinea.ID_Cliente, EsClave = true, ID_TipoColectiva = Int32.Parse(IDTipoColectiva), Descripcion = "ID_CuentaHabiente" };
                //    TodosLosParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = laLinea.LimiteCredito.ToString(), Descripcion = "Importe" };

                //    Respuesta unaRespo = ModificarLineaCredito(ID_Evento, ID_Contrato, laLinea.DiasVigencia, laLinea.ID_Cliente, laLinea.Tarjeta, "Modificacion de Linea de Credito ", "", Int64.Parse(laLinea.Consecutivo), TodosLosParametros);

                //    DAOEvento.IndicarProcesado(unaRespo.CodigoRespuesta == 0 ? true : false, laLinea.Tarjeta);

                //    DataSet laresp = unaOperacion.ModificacionLinea_respuesta(int.Parse(laLinea.Consecutivo), laLinea.ID_Cliente, laLinea.Tarjeta, unaRespo.CodigoRespuesta.ToString(), "");

                //}


                foreach (LineaCredito laLinea in obtieneNuevasLineas())
                {

                    String ClaveCadenaComercial = PNConfig.Get("MODIFICAR", laLinea.Tarjeta.Substring(0,8));

                    Dictionary<String, Parametro> TodosLosParametros = DAOEvento.ListaDeParamentrosContrato(ClaveCadenaComercial, laLinea.Tarjeta, ClaveEvento, "ProcesadorNocturno");

                    int ID_Evento = TodosLosParametros.ContainsKey("@ID_Evento") ? Int32.Parse(TodosLosParametros["@ID_Evento"].Valor) : 0;
                    int ID_Contrato = TodosLosParametros.ContainsKey("@ID_Contrato") ? Int32.Parse(TodosLosParametros["@ID_Contrato"].Valor) : 0;

                    TodosLosParametros["@ID_CuentaHabiente"] = new Parametro() { Nombre = "@ID_CuentaHabiente", Valor = laLinea.ID_Cliente, EsClave = true, ID_TipoColectiva = Int32.Parse(IDTipoColectiva), Descripcion = "ID_CuentaHabiente" };
                    TodosLosParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = laLinea.LimiteCredito.ToString(), Descripcion = "Importe" };

                    Logueo.EntradaSalida("La Modificacion de Limite de Credito: Consecutivo:" + laLinea.Consecutivo + ", ID_Cliente: " + laLinea.ID_Cliente + ", Tarjeta: " + laLinea.Tarjeta + ", Importe:" + laLinea.LimiteCredito , "", false);
                    Respuesta unaRespo = ModificarLineaCredito(ID_Evento, ID_Contrato, laLinea.DiasVigencia, laLinea.ID_Cliente, laLinea.Tarjeta, "Modificacion de Linea de Credito ", "", Int64.Parse(laLinea.Consecutivo), TodosLosParametros);
                    Logueo.EntradaSalida("Respuesta de la Modificacion de Limite de Credito: Consecutivo:" + laLinea.Consecutivo + ", CodigoRespuesta:" + unaRespo.CodigoRespuesta + ", Autorizacion: " + unaRespo.Autorizacion + ", Tarjeta: " + unaRespo.Tarjeta + ", XML:" + unaRespo.XmlExtras, "", false);
                    DataSet laresp = unaOperacion.ModificacionLinea_respuesta(int.Parse(laLinea.Consecutivo), laLinea.ID_Cliente, laLinea.Tarjeta, unaRespo.CodigoRespuesta.ToString(), unaRespo.Descripcion);

                }


                return true;
            }
            catch (Exception err)
            {
                Logueo.Error("ModificarLineas():" + err.Message);
                return false;
            }
        }



        private static List<LineaCredito> obtieneNuevasLineas()
        {
            List<LineaCredito> lasNuevasLineas = new List<LineaCredito>();

            //LineaCredito unaLineaCredito = new LineaCredito();

            //unaLineaCredito.Consecutivo = "141625";
            //unaLineaCredito.ID_Cliente = "90000000070308";
            //unaLineaCredito.LimiteCredito = 1523.23M;
            //unaLineaCredito.Tarjeta = "2001010000002103";
            //unaLineaCredito.DiasVigencia = 30;


  
            //lasNuevasLineas.Add(unaLineaCredito);
            try
            {
                //obtiene el dataset del ws
                wsQuantumLimCred.Quantum unaOperacion = new wsQuantumLimCred.Quantum();

                DataSet losUsuarios = unaOperacion.ModificacionLinea(DateTime.Now.ToString("yyyy-MM-dd"));


                foreach (DataRow renglon in losUsuarios.Tables[0].Rows)
                {
                    LineaCredito unaLineaCredito = new LineaCredito();

                    unaLineaCredito.Consecutivo = renglon["CONSECUTIVO"].ToString();
                    unaLineaCredito.ID_Cliente = renglon["REFERENCIAPAGO"].ToString();
                    unaLineaCredito.LimiteCredito = (Decimal)renglon["IMPORTE"];
                    unaLineaCredito.Tarjeta = renglon["TARJETA"].ToString();
                    unaLineaCredito.DiasVigencia = Int32.Parse(renglon["DIASVIGENCIA"].ToString());



                    lasNuevasLineas.Add(unaLineaCredito);
                }

            }
            catch (Exception err)
            {
                Logueo.Error("obtieneMovimientos()" + err.Message);
            }

            return lasNuevasLineas;
        }

    }
}
