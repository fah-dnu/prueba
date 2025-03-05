using CommonProcesador;
using DNU_ParabiliumProcesoMSI.CapaDeDatos;
using DNU_ParabiliumProcesoMSI.CapaDeNegocio.Extensiones;
using DNU_ParabiliumProcesoMSI.CapaDeNegocio.Funciones;
using DNU_ParabiliumProcesoMSI.CapaDeNegocio.Validaciones;
using DNU_ParabiliumProcesoMSI.Modelos.Abtracciones;
using DNU_ParabiliumProcesoMSI.Modelos.Clases;
using DNU_ParabiliumProcesoMSI.Modelos.Entidades;
using DNU_ParabiliumProcesoMSI.Servicios.EjecutorPolizas;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI.CapaDeNegocio
{
    internal class LNMSI : AMSI
    {
        public LNMSI()
        {
            ConfiguracionContexto.InicializarContexto();
        }

        public override bool inicioProcesoMSI()
        {


            //   ApmNoticeWrapper.SetTransactionName("inicio");
            const string METODO = "Proceso MSI";
            string cultura = CultureInfo.CurrentCulture.Name;
            Logueo.Evento("[ProcesoMSI] culture " + cultura);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            Logueo.Evento("[ProcesoMSI] cultureMX " + cultura);
            Logueo.Evento("Iniciando proceso de MSI");
            string cadenaConexion = PNConfig.Get("PROCESADIFERIMIENTOS", "BDWriteAutorizador");
            DAOEjecutor daoEjecutor = new DAOEjecutor();


            bool isTemplateGeneral = false;
            //
            string ruta = "";
          //  ruta = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalida");

            try
            {
                //      pdfGeneradoCorrectamente = LNOperacionesCorte.GeneracionPDFTimbreSinCorteV2(id, "", fecha, ruta, rutaImagen, false, new RespuestaSolicitud(), _lnEnvioCorreo, _daoCortes, nombrePdfCredito, nombrePdfPrepago, false, _daoFacturas, _transformador);
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraReporte] [Error al generar el PDF o envio de correo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");

            }

            //Procesando cuentas
            Cuentas cuentaPac = new Cuentas();
            try
            {

                //facturas internas
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    conn.Open();
                    try
                    {
                        //OBTIENE EL SET DE CUENTAS A CORTAR HOY
                        DAOOperaciones daoOperaciones = new DAOOperaciones(null);
                        DataTable tablaOperacionesAProcesar = daoOperaciones.ObtieneOperacionesADiferir(conn);
                        List<Operaciones> listaOperaciones = new List<Operaciones>();
                        if (tablaOperacionesAProcesar != null && tablaOperacionesAProcesar.Rows.Count > 0)
                        {
                            listaOperaciones = FTablas.ConvertDataTable<Operaciones>(tablaOperacionesAProcesar);
                        }
                        else
                        {
                            Logueo.Evento("[ProcesoMSI] No se encontraron operaciones a diferir");

                        }
                        foreach (Operaciones operacion in listaOperaciones)
                        {
                            //credito
                            Logueo.Evento("[ProcesoMSI] Iniciando Proceso Credito");
                            // string formattedDate = fecha.ToString("yyyy-MM-dd hh:mm:ss");


                            Dictionary<String, Parametro> Todos_losParametros = new Dictionary<string, Parametro>();
                            Todos_losParametros["@ID_Cuenta"] = new Parametro { Nombre = "@ID_Cuenta", Valor = operacion.IdCuenta.ToString(), Descripcion = "id de la cuenta" };
                            // Todos_losParametros["@ID_CadenaComercial"] = new Parametro() { Nombre = "@ID_CadenaComercial", Valor = cuenta.id_CadenaComercial.ToString(), Descripcion = "id cadena comercial" };
                            Todos_losParametros["@FechaCorte"] = new Parametro() { Nombre = "@FechaCorte", Valor = operacion.FechaProximoCorte.ToString("yyyy-MM-dd HH:mm:ss"), Descripcion = "Fecha corte" };
                            Todos_losParametros["@FechaCompra"] = new Parametro() { Nombre = "@FechaCompra", Valor = operacion.FechaCompra.ToString("yyyy-MM-dd HH:mm:ss"), Descripcion = "Fecha compra" };
                            Todos_losParametros["@ID_CuentaHabiente"] = new Parametro() { Nombre = "@ID_CuentaHabiente", Valor = operacion.IdColectivaCuentahabiente.ToString(), ID_TipoColectiva = Convert.ToInt32(operacion.IDTipoColectivaCuentahabiente), Descripcion = " " };
                            Todos_losParametros["@ID_CadenaComercial"] = new Parametro() { Nombre = "@ID_CadenaComercial", Valor = operacion.IdCadenaComercial.ToString(), Descripcion = " " };
                            // Todos_losParametros["@ClaveCorteTipo"] = new Parametro() { Nombre = "@ClaveCorteTipo", Valor = cuenta.ClaveCorteTipo.ToString(), Descripcion = " " };
                            Todos_losParametros["@Tarjeta"] = new Parametro() { Nombre = "@Tarjeta", Valor = operacion.Tarjeta.ToString(), Descripcion = " " };
                            Todos_losParametros["@ClaveCadenaComercial"] = new Parametro() { Nombre = "@ClaveCadenaComercial", Valor = operacion.ClaveCliente.ToString(), Descripcion = " " };
                            Todos_losParametros["@Evento"] = new Parametro() { Nombre = "@Evento", Valor = operacion.Evento.ToString(), Descripcion = " " };
                            Todos_losParametros["@IdPoliza"] = new Parametro() { Nombre = "@IdPoliza", Valor = operacion.IdPoliza.ToString(), Descripcion = " " };
                            Todos_losParametros["@Meses"] = new Parametro() { Nombre = "@Meses", Valor = operacion.Meses.ToString(), Descripcion = " " };
                            Todos_losParametros["@ImporteOperacion"] = new Parametro() { Nombre = "@ImporteOperacion", Valor = operacion.Cargo.ToString(), Descripcion = " " };
                            Todos_losParametros["@IdEvento"] = new Parametro() { Nombre = "@IdEvento", Valor = operacion.IdEvento.ToString(), Descripcion = " " };
                            Todos_losParametros["@IdOperacion"] = new Parametro() { Nombre = "@IdOperacion", Valor = operacion.IdOperacion.ToString(), Descripcion = " " };
                            Todos_losParametros["@Concepto"] = new Parametro() { Nombre = "@Concepto", Valor = operacion.Concepto.ToString(), Descripcion = " " };
                            Todos_losParametros["@ClavePromocion"] = new Parametro() { Nombre = "@ClavePromocion", Valor = operacion.ClavePromocion.ToString(), Descripcion = " " };
                            Todos_losParametros["@Diferimiento"] = new Parametro() { Nombre = "@Diferimiento", Valor = operacion.Diferimiento.ToString(), Descripcion = " " };
                            Todos_losParametros["@TasaInteresMSI"] = new Parametro() { Nombre = "@TasaInteresMSI", Valor = operacion.TasaInteresMSI.ToString(), Descripcion = " " };
                            Todos_losParametros["@IdEventoIntereses"] = new Parametro() { Nombre = "@IdEventoIntereses", Valor = operacion.IdEventoIntereses.ToString(), Descripcion = " " };
                            Todos_losParametros["@EventoIntereses"] = new Parametro() { Nombre = "@EventoIntereses", Valor = operacion.EventoIntereses.ToString(), Descripcion = " " };
                            Todos_losParametros["@IdProducto"] = new Parametro() { Nombre = "@IdProducto", Valor = operacion.IdProducto.ToString(), Descripcion = " " };
                            Todos_losParametros["@ConceptoMCI"] = new Parametro() { Nombre = "@ConceptoMCI", Valor = operacion.ConceptoMCI.ToString(), Descripcion = " " };
                            Todos_losParametros["@IdEventoBonificacion"] = new Parametro() { Nombre = "@IdEventoBonificacion", Valor = operacion.IdEventoBonificacion.ToString(), Descripcion = " " };
                            Todos_losParametros["@DescripcionEventoBonificacion"] = new Parametro() { Nombre = "@DescripcionEventoBonificacion", Valor = operacion.DescripcionEventoBonificacion.ToString(), Descripcion = " " };


                            //POR CADA CUENTA OBTIENE SUS PARAMETROS MULTIASIGNACION Y SALDO DEL CORTE ANTERIOR
                            Logueo.Evento("[ProcesoMSI] Obteniendo Parametros, cuenta " + operacion.IdCuenta.ToString());


                            //Obteniendo parametros
                            Dictionary<String, Parametro> parametrosExecute = daoEjecutor.ObtenerDatosParametros(Todos_losParametros, conn, null, cadenaConexion);
                            foreach (string parametro in parametrosExecute.Keys)
                            {

                                Todos_losParametros[parametro] = new Parametro() { Nombre = parametro, Valor = parametrosExecute[parametro].Valor, Descripcion = parametrosExecute[parametro].Descripcion, ID_TipoColectiva = parametrosExecute[parametro].ID_TipoColectiva };

                                //unParamentro.Nombre = (laTabla.Rows[k]["Nombre"]).ToString();
                                //unParamentro.ID_TipoColectiva = Int16.Parse((laTabla.Rows[k]["ID_TipoColectiva"]).ToString());
                                //unParamentro.Valor = (laTabla.Rows[k]["valor"]).ToString();
                                //  Todos_losParametros.Add(parametro, parametrosExecute[parametro]);

                            }

                            int mesesTotales = Convert.ToInt32(Todos_losParametros["@Meses"].Valor);
                            bool commitMSI = true;
                            DataTable tablaMesesAProcesarPorOperacion = daoOperaciones.ObtieneMesesPorOperacion(conn, Todos_losParametros);
                            if (tablaMesesAProcesarPorOperacion != null && tablaMesesAProcesarPorOperacion.Rows.Count >= 1 && tablaMesesAProcesarPorOperacion.Rows[0][0].ToString() != "error")
                            {
                                using (SqlTransaction transaccionSQL = conn.BeginTransaction())//System.Data.IsolationLevel.ReadCommitted
                                {
                                    bool esMSI = true;
                                    //validamos si trae tasa de ineteres para que se use entonces MCI
                                    if (Convert.ToDecimal(tablaMesesAProcesarPorOperacion.Rows[0]["intereses"]) > 0) {
                                        esMSI = false;
                                    }
                                    decimal tasaTransaccion = Convert.ToDecimal(tablaMesesAProcesarPorOperacion.Rows[0]["tasaInteresMSI"]);
                                    if (Todos_losParametros["@Evento"].Valor.ToLower().Contains("reverso"))
                                    {
                                        try
                                        {
                                            string idPoliza = Todos_losParametros["@IdPoliza"].Valor;
                                            RespuestaProceso errores = new RespuestaProceso();
                                            //reservando poliza
                                            //  decimal intereses = Convert.ToDecimal(fila["intereses"].ToString());
                                            Logueo.Evento("[ProcesoMSI] Generando bonificacion por: " +Todos_losParametros["@ImporteOperacion"].Valor);
                                            Todos_losParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = Todos_losParametros["@ImporteOperacion"].Valor, Descripcion = " " };

                                            Bonificacion bonificacion = new Bonificacion
                                            {
                                                IdEvento = Convert.ToInt32(Todos_losParametros["@IdEventoBonificacion"].Valor),
                                                Observaciones = "BONIFICACION Diferimiento [Poliza "+ idPoliza.ToString() + "]",
                                                Concepto = Todos_losParametros["@DescripcionEventoBonificacion"].Valor,
                                                RefNumerica = 0
                                            };
                                            RespuestaExecuter respuestaExecute = ServicioExecuter.EjecutarEvento(Todos_losParametros, bonificacion, conn, transaccionSQL);
                                            if (respuestaExecute.CodigoRespuesta != 00)
                                            {
                                                Logueo.Error("[ProcesoMSI] La poliza no pudo crearse , Descripcion: " + respuestaExecute.Descripcion + ", Cadena comercial:" + Todos_losParametros["@ID_CadenaComercial"].Valor + ".");
                                                // GeneracionExitosaDePolizas = false;
                                                //transaccionSQL.Rollback();
                                                commitMSI = false;


                                            }
                                            else{
                                                DataTable tablaActualizarPolizasOperacion = daoOperaciones.ActualizaPolizaOperacion(respuestaExecute.ID_Poliza.ToString(),  Todos_losParametros["@IdOperacion"].Valor, idPoliza.ToString(), conn, transaccionSQL);
                                                if (tablaActualizarPolizasOperacion != null && tablaActualizarPolizasOperacion.Rows.Count > 0 && tablaActualizarPolizasOperacion.Rows[0][0].ToString() == "0000")
                                                {
                                                    Logueo.Evento("[ProcesoMSI]Relacion fecha realizada");
                                                }
                                                else
                                                {
                                                    commitMSI = false;
                                                    Logueo.Evento("[ProcesoMSI]Error al relacionar fecha");
                                                    break;
                                                }
                                            }

                                            //var tablaobtenerDatosReverso = daoOperaciones.ReversaPoliza(idPoliza, "", "", conn, transaccionSQL);
                                            //if (VRespuestaProcedimientos.BusquedaSinErrores(errores, tablaobtenerDatosReverso))
                                            //{
                                            //    string nuevaPoliza = tablaobtenerDatosReverso.Rows[0][0].ToString(); ;//datosTarjeta.nuevaPoliza;
                                            //    if (Convert.ToInt64(nuevaPoliza) > 0)
                                            //    {

                                            //        // rollBack = false;
                                            //    }
                                            //    else
                                            //    {
                                            //        commitMSI = false;
                                            //        Logueo.Error("[api/confirmacionSpei/] error al realizar reverso en poliza,num Poliza: " + nuevaPoliza);

                                            //    }

                                            //}
                                            //else
                                            //{
                                            //    commitMSI = false;
                                            //    Logueo.Error("[ProcesoMSI]  error al reversar poliza");
                                            //}
                                        }
                                        catch (Exception ex)
                                        {
                                            commitMSI = false;
                                            Logueo.Error("[ProcesoMSI]  Error en la reversa poliza " + ex.Message + "," + ex.StackTrace);

                                        }
                                    }
                                    if (commitMSI)
                                    {
                                        foreach (DataRow fila in tablaMesesAProcesarPorOperacion.Rows)
                                        {
                                            try
                                            {
                                                string observaciones = fila["observaciones"].ToString();
                                                int numeroMes = Convert.ToInt32(fila["numeroMes"].ToString());
                                                DateTime fechaPoliza = DateTime.Parse(fila["fechaPoliza"].ToString());
                                                string monto = fila["monto"].ToString();
                                                Todos_losParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = monto, Descripcion = " " };
                                                int idEvento = Convert.ToInt32(Todos_losParametros["@IdEvento"].Valor);
                                                string concepto = Todos_losParametros["@Concepto"].Valor;
                                                if (!esMSI) {
                                                    string interes = fila["intereses"].ToString();
                                                    string ivaInteres = fila["ivaIntereses"].ToString();
                                                    Todos_losParametros["@InteresesParcialidad"] = new Parametro() { Nombre = "@InteresesParcialidad", Valor = interes, Descripcion = " " };
                                                    Todos_losParametros["@IvaInteresesParcialidad"] = new Parametro() { Nombre = "@IvaInteresesParcialidad", Valor = ivaInteres, Descripcion = " " };
                                                    idEvento = Convert.ToInt32(Todos_losParametros["@IdEventoIntereses"].Valor);
                                                    concepto = Todos_losParametros["@ConceptoMCI"].Valor;
                                                }
                                              //  decimal intereses = Convert.ToDecimal(fila["intereses"].ToString());
                                                Logueo.Evento("[ProcesoMSI] Generando poliza mes: " + numeroMes + " operacion:" + Todos_losParametros["@IdOperacion"].Valor);
                                                //INSERTA EL CORTE.
                                                Bonificacion bonificacion = new Bonificacion
                                                {
                                                    IdEvento = idEvento,// Convert.ToInt32(Todos_losParametros["@IdEvento"].Valor),
                                                    Observaciones = observaciones,
                                                    Concepto = concepto,//Todos_losParametros["@Concepto"].Valor,
                                                    RefNumerica = 0
                                                };
                                                RespuestaExecuter respuestaExecute = ServicioExecuter.EjecutarEvento(Todos_losParametros, bonificacion, conn, transaccionSQL);
                                                if (respuestaExecute.CodigoRespuesta != 00)
                                                {
                                                    Logueo.Error("[ProcesoMSI] La poliza no pudo crearse , Descripcion: " + respuestaExecute.Descripcion + ", Cadena comercial:" + Todos_losParametros["@ID_CadenaComercial"].Valor + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                                    // GeneracionExitosaDePolizas = false;
                                                    //transaccionSQL.Rollback();
                                                    commitMSI = false;
                                                    break;
                                                    //continue;
                                                }
                                                else
                                                {
                                                    Logueo.Evento("[ProcesoMSI] La poliza Creada: [" + respuestaExecute.ID_Poliza + "]" + ".");
                                                    bool mesFinal = false;
                                                    if (mesesTotales == numeroMes)
                                                    {
                                                        mesFinal = true;
                                                    }
                                                    DataTable tablaActualizarPolizas = daoOperaciones.ActualizaFechaPoliza(respuestaExecute.ID_Poliza.ToString(), fechaPoliza, Todos_losParametros["@IdOperacion"].Valor, mesFinal, conn, transaccionSQL, tasaTransaccion);
                                                    if (tablaActualizarPolizas != null && tablaActualizarPolizas.Rows.Count > 0 && tablaActualizarPolizas.Rows[0][0].ToString() == "0000")
                                                    {
                                                        Logueo.Evento("[ProcesoMSI]Relacion fecha realizada");
                                                    }
                                                    else
                                                    {
                                                        commitMSI = false;
                                                        Logueo.Evento("[ProcesoMSI]Error al relacionar fecha");
                                                        break;
                                                    }

                                                    if (false)//(intereses > 0)
                                                    {
                                                        Bonificacion bonificacionIntereses = new Bonificacion
                                                        {
                                                            IdEvento = Convert.ToInt32(Todos_losParametros["@IdEventoIntereses"].Valor),
                                                            Observaciones = observaciones,
                                                            Concepto = Todos_losParametros["@Concepto"].Valor,
                                                            RefNumerica = 0
                                                        };
                                                        RespuestaExecuter respuestaExecuteIntereses = ServicioExecuter.EjecutarEvento(Todos_losParametros, bonificacionIntereses, conn, transaccionSQL);
                                                        if (respuestaExecuteIntereses.CodigoRespuesta != 00)
                                                        {
                                                            Logueo.Error("[ProcesoMSI] La poliza intereses no pudo crearse , Descripcion: " + respuestaExecuteIntereses.Descripcion + ", Cadena comercial:" + Todos_losParametros["@ID_CadenaComercial"].Valor + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                                            // GeneracionExitosaDePolizas = false;
                                                            //transaccionSQL.Rollback();
                                                            commitMSI = false;
                                                            break;
                                                            //continue;
                                                        }
                                                        else
                                                        {
                                                            Logueo.Evento("[ProcesoMSI] La poliza intereses Creada: [" + respuestaExecute.ID_Poliza + "]" + ".");
                                                            //bool mesFinal = false;
                                                            DataTable tablaActualizarPolizasIntereses = daoOperaciones.ActualizaFechaPoliza(respuestaExecuteIntereses.ID_Poliza.ToString(), fechaPoliza, Todos_losParametros["@IdOperacion"].Valor, mesFinal, conn, transaccionSQL,tasaTransaccion);
                                                            if (tablaActualizarPolizasIntereses != null && tablaActualizarPolizasIntereses.Rows.Count > 0)
                                                            {
                                                                Logueo.Evento("[ProcesoMSI]Relacion fecha poliza realizada");
                                                            }
                                                            else
                                                            {
                                                                commitMSI = false;
                                                                Logueo.Evento("[ProcesoMSI]Error al relacionar fecha con poliza");
                                                                break;
                                                            }
                                                        }

                                                    }

                                                    //llenando lista de factura

                                                    continue;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                commitMSI = false;
                                                Logueo.Error("[ProcesoMSI]  Error en la generaciondne poliza " + ex.Message + "," + ex.StackTrace);
                                                break;
                                            }
                                        }
                                    }
                                    if (commitMSI)
                                    {
                                        Logueo.Error("[ProcesoMSI]  realizando commit");
                                        transaccionSQL.Commit();
                                        Logueo.Error("[ProcesoMSI]  commit realizado");
                                    }
                                    else
                                    {
                                        Logueo.Error("[ProcesoMSI]  realizando rollback");
                                        transaccionSQL.Rollback();
                                        Logueo.Error("[ProcesoMSI]  rollback realizado");
                                    }
                                }

                            }
                            else
                            {
                                String descripcionError = "";
                                if (tablaMesesAProcesarPorOperacion != null && tablaMesesAProcesarPorOperacion.Rows.Count == 1)
                                {
                                    descripcionError = tablaMesesAProcesarPorOperacion.Rows[0][1].ToString();
                                }
                                Logueo.Error("[ProcesoMSI] error al obtener tabla de pagos o solo hay una mensualidad " + descripcionError);


                            }
                        }
                        //Logueo.Evento("[ProcesoMSI] fin proceso MSI");

                        //DE LAS CUENTAS A CORTAR HOY fin foreach
                        //pasando archivos a sftp

                    }
                    catch (Exception exConection)
                    {
                        Logueo.Error("[ProcesoMSI] error en el proceso:" + exConection.Message + " " + exConection.StackTrace);
                        // ApmNoticeWrapper.NoticeException(exConection);
                    }
                    finally
                    {
                        conn.Close();
                    }
                } //using conection


            }
            catch (Exception err)
            {
                Logueo.Error("[ProcesoMSI] error proceso MSI:" + err.Message + " " + err.StackTrace);
                // ApmNoticeWrapper.NoticeException(err);
            }
            Logueo.Evento("[ProcesoMSI] Fin proceso corte");
            return true;
        }
    }
}
