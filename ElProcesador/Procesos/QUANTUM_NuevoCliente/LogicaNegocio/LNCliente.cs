using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using QUANTUM_AplicarMovimientos.Entidades;
using Interfases.Entidades;
using QUANTUM_NuevoCliente.Entidades;
using QUANTUM_NuevoCliente.BaseDatos;
using System.Data.SqlClient;
using CommonProcesador;
using System.Data;
using Interfases.Exceptiones;
using Executer;

namespace QUANTUM_NuevoCliente.LogicaNegocio
{
    public class LNCliente
    {

       static wsQuantum.Quantum unaOperacion = new wsQuantum.Quantum();

        public static bool ProcesaNuevosClientes()
        {
            List<Respuesta> lasRespuestas = new List<Respuesta>();
            try
            {

                String ClaveCadenaComercial = PNConfig.Get("NUEVOCLNT", "ClaveCadenaComercial");
                String ClaveEvento = PNConfig.Get("NUEVOCLNT", "ClaveEvento");
                String TipoColectiva = PNConfig.Get("NUEVOCLNT", "IDTipoColectivaCCH");

                wsQuantum.Quantum unaOperacion = new wsQuantum.Quantum();


                Dictionary<String, Parametro> TodosLosParametros = DAOEvento.ListaDeParamentrosContrato(ClaveCadenaComercial,"", ClaveEvento, "ProcesadorNocturno");

                int ID_Evento = TodosLosParametros.ContainsKey("@ID_Evento") ? Int32.Parse(TodosLosParametros["@ID_Evento"].Valor) : 0;
                int ID_Contrato = TodosLosParametros.ContainsKey("@ID_Contrato") ? Int32.Parse(TodosLosParametros["@ID_Contrato"].Valor) : 0;


              

                foreach (Cliente unCliente in obtieneNuevosClientes())
                {

                   // TodosLosParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = unCliente.LimiteCredito.ToString(), Descripcion = "Importe" };
                  //  Respuesta unaRespo = ModificarLineaCredito(ID_Evento, ID_Contrato, laLinea.DiasVigencia, laLinea.ReferenciaPago, laLinea.Tarjeta, "Modificacion de Linea de Credito ", "", Int64.Parse(laLinea.Consecutivo), TodosLosParametros);

                    Respuesta unaRespo = AgregarCliente(ClaveCadenaComercial, ID_Evento, ID_Contrato, int.Parse(TipoColectiva),  unCliente, "ProcesadorNocturno", TodosLosParametros);

                    

                    if (unaRespo.CodigoRespuesta == 0)
                    {
                        DAOCliente.IndicarProcesado(true, unCliente.Tarjeta);
                       // DataSet laresp = unaOperacion.NuevoCliente_respuesta(int.Parse(unCliente.Consecutivo), unCliente.ID_Cliente, unaRespo.CodigoRespuesta.ToString(), "AUTORIZADA");
                    }
                    else
                    {
                        DAOCliente.IndicarProcesado(true, unCliente.Tarjeta);
                       // DataSet laresp = unaOperacion.NuevoCliente_respuesta(int.Parse(unCliente.Consecutivo), unCliente.ID_Cliente, unaRespo.CodigoRespuesta.ToString(), "DECLINADA");
                    }
  
                }


                return true;
            }
            catch (Exception err)
            {
                Logueo.Error("ModificarLineas():" + err.Message);
                return false;
            }
        }

        public static Respuesta AgregarCliente(String CadenaComercial, int ID_Evento, int ID_Contrato, int ID_tipoColectiva, Cliente elCliente, String elUsuario, Dictionary<String, Parametro> TodosLosParametros)
        {
            Respuesta laRespuesta = new Respuesta();
            int Resp = -1;
            //  Parametro[] losParamentros = null;
            try
            {



                Parametro unParametro = new Parametro() { Nombre = "@Importe", Valor = elCliente.LimiteCredito.ToString(), Descripcion = "Importe" };
                TodosLosParametros[unParametro.Nombre] = unParametro;
      
                //if (ID_Contrato == 0 | ID_Evento == 0)
                //{
                //    throw new Exception("No se pudo obtener un ID_Evento o Contraro valido");
                //}

                using (SqlConnection conn = new SqlConnection(BaseDatos.BDNuevoCliente.strBDEscritura))
                {
                    conn.Open();

                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {


                            //CREAR LA COLECTIVA 
                            elCliente = DAOCliente.AgregarCliente(elCliente, ID_tipoColectiva, elCliente.ClaveSucursal, conn, transaccionSQL);

                            //  //MEETER EN LOS PARAMETROS EL TIPO COLECTIVA TARJETAHABIENTE O CUENTAHABIENTE


                            Parametro unParametroCH = new Parametro() { Nombre = "@ID_CuentaHabiente", Valor = elCliente.ID_Colectiva.ToString(), Descripcion = "Colectiva Cliente CuentaHabiente", ID_TipoColectiva = int.Parse(PNConfig.Get("NUEVOCLNT", "IDTipoColectivaCCH")) };

                            TodosLosParametros[unParametroCH.Nombre] = unParametroCH;


                            bool laRespCuenta = false;
                            if (elCliente.ID_Colectiva != 0)
                            {
                                //CREAR LAS CUENTAS
                                String[] losDatos = PNConfig.Get("NUEVOCLNT", "cuentasPorCrear").Split(',');

                                foreach (String unTipoCuenta in losDatos)
                                {
                                    laRespCuenta = DAOCliente.GenerarCuentas(elCliente, CadenaComercial, elCliente.Tarjeta, elCliente.Tarjeta, unTipoCuenta, conn, transaccionSQL);
                                }
                            }

                            //GENERA Y APLICA LA POLIZA DEL LIMITE DE CREDITO
                            if (laRespCuenta)
                            {
                                EventoManual aplicador = new Executer.EventoManual(ID_Evento, ID_Contrato, "Establecimiento de Linea de Credito", false, TodosLosParametros, conn, transaccionSQL, "", Int32.Parse(DateTime.Now.ToString("MMddHHmmss")));
                                Resp = aplicador.AplicaContablilidad();
                            }

                            if (Resp != 0)
                            {
                                throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                            }
                            else
                            {

                                Respuesta unaRespo2 = DAOEvento.AsignarVigencia(elCliente.Tarjeta, elCliente.DiasVigencia, "ProcesadorNocturno", conn, transaccionSQL);

                                if (unaRespo2.CodigoRespuesta == 0)
                                {
                                    
                                    unaRespo2.CodigoRespuesta = Resp;
                                    unaRespo2.Autorizacion = DateTime.Now.ToString("HHmmss");

                                   // transaccionSQL.Commit();
                                }
                                else
                                {
                                    transaccionSQL.Rollback();
                                }


                                Logueo.Evento("Usuario Creado: " + elCliente.NombreORazonSocial + " " + elCliente.APaterno + " " + elCliente.AMaterno + ", Tarjeta: " + elCliente.Tarjeta);
                                transaccionSQL.Commit();
                                laRespuesta.CodigoRespuesta = 0;
                            }


                        }
                        catch (CAppException err)
                        {
                            transaccionSQL.Rollback();
                            String elerror = err.MensajeOriginal() + " , " + err.Mensaje();
                            Logueo.Error(elerror + " NuevoCliente. LNEventos.EjecuarEvento()");
                            laRespuesta.CodigoRespuesta = -1;
                        }
                        catch (Exception err)
                        {
                            transaccionSQL.Rollback();
                            Logueo.Error(err.Message + "NuevoCliente. LNEventos.EjecuarEvento()");
                            laRespuesta.CodigoRespuesta = -1;
                        }

                    }
                }
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message + " NuevoCliente.LNEventos.EjecuarEvento()");
                laRespuesta.CodigoRespuesta = -1;
            }

            return laRespuesta;

        }


        private static List<Cliente> obtieneNuevosClientes()
        {
            List<Cliente> losNuevosClientes = new List<Cliente>();

            //Cliente unClienteNuevo = new Cliente();

            //unClienteNuevo.Consecutivo = "141625";
            //unClienteNuevo.ID_Cliente = "90000000070308";
            //unClienteNuevo.NombreORazonSocial = "JOSE DE JESUS";
            //unClienteNuevo.APaterno = "PEREZ";
            //unClienteNuevo.AMaterno = "CASTAGNE";
            //unClienteNuevo.FechaNacimiento = DateTime.Now;
            //unClienteNuevo.Tarjeta = "2001010000002103";
            //unClienteNuevo.ClaveSucursal = "SPBR001";
            //unClienteNuevo.Telefono = "2222812653";
            //unClienteNuevo.LimiteCredito = 925.23M;
            //unClienteNuevo.DiasVigencia = 23;

            //losNuevosClientes.Add(unClienteNuevo);
            //return losNuevosClientes;


            try
            {
               losNuevosClientes = DAOCliente.ListaUsuariosParaLC();

                //obtiene el dataset del ws
                
               
                ///***********ESTO ES PARA OBTENER LOS NUEVOS CLIENTES DESDE EL WEBSERVICE
                  
                        //DataSet losUsuarios = unaOperacion.NuevoCliente(DateTime.Now.ToString("yyyy-MM-dd"));


                        //foreach (DataRow renglon in losUsuarios.Tables[0].Rows)
                        //{
                        //    Cliente unClienteNuevo = new Cliente();

                        //    unClienteNuevo.Consecutivo = renglon["CONSECUTIVO"].ToString();
                        //    unClienteNuevo.ID_Cliente = renglon["REFERENCIAPAGO"].ToString();
                        //    unClienteNuevo.NombreORazonSocial = renglon["NOMBRE"].ToString();
                        //    unClienteNuevo.APaterno = renglon["APATERNO"].ToString();
                        //    unClienteNuevo.AMaterno = renglon["AMATERNO"].ToString();
                        //    unClienteNuevo.FechaNacimiento = (DateTime)renglon["FECHA_NACIMIENTO"];
                        //    unClienteNuevo.Tarjeta = renglon["TARJETA"].ToString();
                        //    unClienteNuevo.ClaveSucursal = "na";
                        //    unClienteNuevo.Telefono = renglon["TELEFONO"].ToString();
                        //    unClienteNuevo.LimiteCredito = (Decimal)renglon["IMPORTE"];
                        //    unClienteNuevo.DiasVigencia = Int32.Parse(renglon["DIASVIGENCIA"].ToString());

                        //    losNuevosClientes.Add(unClienteNuevo);
                        //}
                //**********************************HASTA AQUI NUEVOS CLIENTES DESDE EL WS


            }
            catch (Exception err)
            {
                Logueo.Error("obtieneNuevosClientes()" + err.Message);
            }

            return losNuevosClientes;
        }
    }
}
