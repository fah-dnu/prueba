using CommonProcesador;
using DNU_ParabiliaProcesoCobranza.CapaDatos;
using DNU_ParabiliaProcesoCobranza.dataService;
using DNU_ParabiliaProcesoCobranza.Entidades;
using DNU_ParabiliaProcesoCortes.Common.Funciones;
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
using System.Xml.Xsl;
//using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;

namespace DNU_ParabiliaProcesoCobranza.LogicaNegocio
{
    class LNCobranza
    {
        private string cadenaConexion;// = "Data Source=45.32.4.114;initial catalog=Auto_producto;user id=OG;password=Og12369;";
        static XslCompiledTransform _transformador = new XslCompiledTransform();
        RespuestaSolicitud respuestaSolicitud;
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "PROCESACOBRANZA";
                _NombreNewRelic = PNConfig.Get(ClaveProceso, "NombreNewRelic");
                if (String.IsNullOrEmpty(_NombreNewRelic))
                {
                    _NombreNewRelic = ClaveProceso + "-SINNOMBRE";
                    Logueo.Evento("Se coloco nombre generico para instrumentacion NewRelic al no encontrar el parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                    Logueo.Error("No se encontro parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                }
                else
                {
                    Logueo.Evento("Se encontro parametro NombreNewRelic: " + _NombreNewRelic + " [" + ClaveProceso + "]");
                }
                return _NombreNewRelic;
            }
        }

        public LNCobranza()
        {
            string ArchXSLT = "";
            try
            {
                ConfiguracionContexto.InicializarContexto();
                ArchXSLT = PNConfig.Get("PROCESACOBRANZA", "ArchivoXSLT");
                
                //_transformador = new XslCompiledTransform();
                cadenaConexion = PNConfig.Get("PROCESACOBRANZA", "BDReadAutorizador");
                //_transformador.Load(ArchXSLT);
            }
            catch (Exception es)
            {
                Logueo.Error("[GeneraGastosCobranza] [Error al generar la cobranza] [Mensaje: " + es.Message + " TRACE: " + es.StackTrace + "]" + ArchXSLT);
            }
        }

      //  [Transaction]
        public bool inicio(string fecha = null)
        {

           // ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
          //  ApmNoticeWrapper.SetTransactionName("inicio");

            string cultura = CultureInfo.CurrentCulture.Name;
            Logueo.Evento("[GeneraGastosCobranza] culture " + cultura);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            Logueo.Evento("[GeneraGastosCobranza] cultureMX " + cultura);
            Logueo.Evento("Iniciando proceso de corte");
            DAOCobranza _daoCobranza = new DAOCobranza();


            //string eventoComPagoTardio = PNConfig.Get("PROCESACOBRANZA", "EventoComPagoTardio");
            //string eventoIvaComPagoTardio = PNConfig.Get("PROCESACOBRANZA", "EventoIvaComPagoTardio");
            //string eventoComPorNoPago = PNConfig.Get("PROCESACOBRANZA", "EventoComPorNoPago");
            //string eventoIvaComPorNoPago = PNConfig.Get("PROCESACOBRANZA", "EventoIvaComPorNoPago");

            string eventoGastosCobranza = PNConfig.Get("PROCESACOBRANZA", "EventoGastosCobranza");
            string eventoIvaGastosCobranza = PNConfig.Get("PROCESACOBRANZA", "IvaGastosCobranza");


            try
            {//aqui no deberia de entrar pero estaba cuando se tomo el codigo
                respuestaSolicitud = new RespuestaSolicitud();

                using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                {
                    conn2.Open();
                    try
                    {
                        List<DetalleFactura> listaDetallesFactura = new List<DetalleFactura>();

                        //OBTIENE EL SET DE CUENTAS A CORTAR HOY
                        servicioDatos servicioDatos_ = new servicioDatos(conn2, cadenaConexion);
                        Logueo.Evento("[GeneraGastosCobranza] Procesando gastos cobranza");
                        foreach (Cuentas cuenta in servicioDatos_.Obtiene_Set_deCuentas(fecha))
                        {
                            Logueo.Evento("[GeneraGastosCobranza] Iniciando proceso credito");
                            Logueo.Evento("[GeneraGastosCobranza] Cuenta: " + cuenta.ID_Cuenta);

                            if (string.IsNullOrEmpty(cuenta.RFCCliente))
                            {
                                Logueo.Error("[GeneraGastosCobranza] el cliente no cuenta con RFC, cuenta:" + cuenta.ID_Cuenta);
                                continue;
                            }

                            Dictionary<String, Parametro> Todos_losParametros = new Dictionary<string, Parametro>();
                            Dictionary<String, Parametro> Set_de_Parametros_Cuenta_MultiAsignacion;
                            Dictionary<String, Parametro> Set_de_Parametros_Cuenta_Corte_Anterior;

                            Logueo.Evento("[GeneraGastosCobranza] Obteniendo prametros, cuenta " + cuenta.ID_Cuenta.ToString());
                            Set_de_Parametros_Cuenta_MultiAsignacion = servicioDatos_.ObtieneParametros_Cuenta(cuenta.ID_Cuenta, cuenta.ID_Corte, cuenta.Tarjeta);
                            if (Set_de_Parametros_Cuenta_MultiAsignacion.Count == 1)
                            {
                                Logueo.Error("[GeneraGastosCobranza] Error al obtener los parametros:" + Set_de_Parametros_Cuenta_MultiAsignacion["@error"].Valor);
                                continue;
                            }

                            foreach (string parametro in Set_de_Parametros_Cuenta_MultiAsignacion.Keys)
                            {
                                Todos_losParametros.Add(parametro, Set_de_Parametros_Cuenta_MultiAsignacion[parametro]);
                            }


                            //// POR CADA CUENTA OBTIENE SUS PARAMETROS DEL CORTE ANTERIOR
                            //Logueo.Evento("[GeneraEstadoCuentaCredito] Obtener parametros corte anterior");
                            //Set_de_Parametros_Cuenta_Corte_Anterior = servicioDatos_.ObtieneParametros_Cuenta_CorteAnterior(cuenta.ID_Cuenta, Todos_losParametros["@DiaFechaCorte"].Valor, cuenta.Fecha_Corte, cuenta.ID_Corte);

                            //foreach (string parametro in Set_de_Parametros_Cuenta_Corte_Anterior.Keys)
                            //{
                            //    Todos_losParametros.Add(parametro, Set_de_Parametros_Cuenta_Corte_Anterior[parametro]);
                            //}


                            //recorriendo el dictionary                                             
                            bool saldoInsolutoPagado = Todos_losParametros["@pagoMinimoSinIntereses"].Valor == "1" ? true : false;


                            //decimal ComPagoTardio = Convert.ToDecimal(Set_de_Parametros_Cuenta_MultiAsignacion["@ComPagoTarjetaTardio"].Valor);
                            //decimal ComNoPago = Convert.ToDecimal(Set_de_Parametros_Cuenta_MultiAsignacion["@ComSinPagoTarjeta"].Valor);

                            //if (ComPagoTardio > 0 || ComNoPago > 0)
                            //{
                            Logueo.Evento("[GeneraGastosCobranza] Obteniendo eventos agrupadores");
                            List<Evento> eventAgrupadores_ = servicioDatos_.Obtiene_EventosAgrupadores(cuenta.ID_Cuenta, cuenta.ClaveCorteTipo, cuenta.ID_Corte, Todos_losParametros["@numeroMes"].Valor);

                            if (eventAgrupadores_.Count == 0)
                            {
                                Logueo.Error("[GeneraGastosCobranza] No hay eventos agrupados");

                                //  Logueo.Error("[GeneraGastosCobranza] No hay eventos agrupados:" + Set_de_Parametros_Cuenta_MultiAsignacion["@error"].Valor);
                            }

                            decimal ivaOrd = 0;
                            decimal ivaMor = 0;
                            decimal intOrd = 0;
                            decimal intMor = 0;

                            //if (ComNoPago > 0)
                            //{
                            using (SqlTransaction transaccionSQL = conn2.BeginTransaction())
                            {
                                try
                                {
                                    string[] sEventos = new string[] { eventoGastosCobranza, eventoIvaGastosCobranza };
                                    bool foreachCorrecto = EjecutaEventosAgrupadores(sEventos, eventAgrupadores_, Todos_losParametros, servicioDatos_, cuenta, transaccionSQL, conn2, cuenta.ID_Corte);

                                    if (foreachCorrecto)
                                    {
                                        var tabla = _daoCobranza.ActualizaBanderaCobranza(cuenta.ID_Corte, conn2, transaccionSQL);
                                        transaccionSQL.Commit();
                                    }
                                    else
                                        transaccionSQL.Rollback();
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        transaccionSQL.Rollback();
                                    }
                                    catch (Exception exTransaction)
                                    {
                                        Logueo.Error("[GeneraGastosCobranza] error al realizar rollback:" + exTransaction.Message + " " + exTransaction.StackTrace);
                                 //       ApmNoticeWrapper.NoticeException(exTransaction);
                                    }
                                }
                            }
                            //}
                            //else if (ComPagoTardio > 0)
                            //{

                            //    using (SqlTransaction transaccionSQL = conn2.BeginTransaction())
                            //    {
                            //        try
                            //        {
                            //            string[] sEventos = new string[] { eventoComPagoTardio, eventoIvaComPagoTardio };
                            //            bool foreachCorrecto = EjecutaEventosAgrupadores(sEventos, eventAgrupadores_, Todos_losParametros, servicioDatos_, cuenta, transaccionSQL, conn2, cuenta.ID_Corte);

                            //            if (foreachCorrecto)
                            //            {
                            //                var tabla = _daoCobranza.ActualizaBanderaCobranza(cuenta.ID_Corte, conn2, transaccionSQL);
                            //                transaccionSQL.Commit();
                            //            }
                            //            else
                            //                transaccionSQL.Rollback();
                            //        }
                            //        catch (Exception ex)
                            //        {
                            //            try
                            //            {
                            //                transaccionSQL.Rollback();
                            //            }
                            //            catch (Exception exTransaction)
                            //            {
                            //                Logueo.Error("[GeneraGastosCobranza] error al realizar rollback:" + exTransaction.Message + " " + exTransaction.StackTrace);
                            //            }
                            //        }
                            //    }
                            //}


                            //}
                            //else
                            //{
                            //    Logueo.Error("[GeneraGastosCobranza] No se aplican gastos de cobranza");
                            //    continue;
                            //}
                        }
                        Logueo.Evento("[GeneraGastosCobranza] fin Procesando gastos cobranza");
                    }
                    catch (Exception ex)
                    {
                        Logueo.Evento("[GeneraGastosCobranza] Error proceso gastos cobranza:"+ex.Message);
                        //  ApmNoticeWrapper.NoticeException(ex);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logueo.Evento("[GeneraGastosCobranza] Error proceso gastos cobranza ex"+ ex.Message);

                //    ApmNoticeWrapper.NoticeException(ex);
                throw ex;
            }



        }

        private bool EjecutaEventosAgrupadores(string[] sEventos, List<Evento> eventAgrupadores_, Dictionary<String, Parametro> Todos_losParametros, servicioDatos servicioDatos_,
                Cuentas cuenta, SqlTransaction transaccionSQL, SqlConnection conn2, long ID_Corte)
        {
            try
            {
                bool foreachCorrecto = true;    //validara las ejecuciones correctas de todos los eventos
                                                //decimal saldoVencido = 0;
                                                //bool interesMoratorio = false;
                bool sinDeuda = false;
                Logueo.Evento("[GeneraGastosCobranza] Procesando eventos agrupadores");
                foreach (string sEvento in sEventos)
                {
                    foreach (Evento unEvento in eventAgrupadores_)
                    {
                        if (sEvento == unEvento.ClaveEventoAgrupador)
                        {

                            Logueo.Evento("[GeneraEstadoCuentaCredito] Evento:" + unEvento.ClaveEventoAgrupador);
                            Dictionary<String, Parametro> Set_de_Parametros_Evento_Agrupador = new Dictionary<string, Parametro>();
                            Todos_losParametros["@" + unEvento.ClaveEventoAgrupador] = new Parametro() { Nombre = "@" + unEvento.ClaveEventoAgrupador, Valor = "0.00" };

                            Set_de_Parametros_Evento_Agrupador["@DescEvento"] = new Parametro() { Nombre = "@DescEvento", Valor = unEvento.Descripcion.ToString(), Descripcion = "Fecha corte" };
                            Set_de_Parametros_Evento_Agrupador["@ID_CadenaComercial_"] = new Parametro() { Nombre = "@ID_CadenaComercial_", Valor = cuenta.id_CadenaComercial.ToString(), Descripcion = " " };
                            Set_de_Parametros_Evento_Agrupador["@ID_CuentaHabiente_"] = new Parametro() { Nombre = "@ID_CuentaHabiente_", Valor = cuenta.ID_CuentaHabiente.ToString(), Descripcion = " " };
                            Set_de_Parametros_Evento_Agrupador["@ID_Evento"] = new Parametro() { Nombre = "@ID_Evento", Valor = unEvento.ID_Evento.ToString(), Descripcion = " " };

                            //AGREGA AL SET DE PARAMETROS EL ID_EVENTO AGRUPADOR
                            Set_de_Parametros_Evento_Agrupador["@ID_EventoAgrupador"] = new Parametro() { Nombre = "@ID_EventoAgrupador", Valor = unEvento.ID_AgrupadorEvento.ToString(), Descripcion = "Agrupador" };

                            // AGREGA O ACTUALIZA EL EVENTO AGRUPADOR a TODOS LOS PARAMETROS
                            Todos_losParametros["@ID_EventoAgrupador"] = new Parametro() { Nombre = "@ID_EventoAgrupador", Valor = unEvento.ID_AgrupadorEvento.ToString(), Descripcion = "id Agrupador" };//este id es el de evento no el del agrupador
                            Todos_losParametros["@ClaveEventoAgrupador"] = new Parametro() { Nombre = "@ClaveEventoAgrupador", Valor = unEvento.ClaveEventoAgrupador.ToString(), Descripcion = "Clave Agrupador" };

                            ////RESPECTO AL STORED PROCEDURE DEL EVENTO AGRUPADOR ACTUAL, OBTIENE SUS PARAMETROS Y TIPOS 
                            //if (unEvento.Stored_Procedure.Length > 0)
                            //{
                            //    try
                            //    {
                            //        DataTable laTabla = null;
                            //        Logueo.Evento("[GeneraGastosCobranza] ejecutando procedimiento evento");

                            //        laTabla = servicioDatos_.ejectutar_SP_EventoAgrupador(unEvento.Stored_Procedure, Todos_losParametros, transaccionSQL);

                            //        for (int k = 0; k < laTabla.Rows.Count; k++)
                            //        {
                            //            Parametro unParamentro = new Parametro();
                            //            unParamentro.Nombre = (laTabla.Rows[k]["NombreParametro"]).ToString();
                            //            unParamentro.Valor = (laTabla.Rows[k]["Valor"]).ToString();

                            //            //AGREGA LOS PARAMETROS DEVUELTOS...
                            //            Set_de_Parametros_Evento_Agrupador.Add((laTabla.Rows[k]["NombreParametro"]).ToString(), unParamentro);
                            //            Todos_losParametros.Add((laTabla.Rows[k]["NombreParametro"]).ToString(), unParamentro);
                            //        }
                            //    }
                            //    catch (Exception err)
                            //    {
                            //        Logueo.Error("[GeneraGastosCobranza] Error al ejecutar el SP del Evento Agrupador:" + err.Message);
                            //        foreachCorrecto = false;
                            //        break;
                            //    }
                            //}

                            Todos_losParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = "0.00", Descripcion = "Importe" };

                            if (!string.IsNullOrEmpty(unEvento.parametroMontoCobranza))
                            {
                                Todos_losParametros["@Importe"].Valor = Todos_losParametros[unEvento.parametroMontoCobranza].Valor;
                            }
                            else
                            {//implica que no se va hacer ninguna poliza y solo se va guardar el valor para el xml(es decir los eventos ya se han aplicado)
                                Logueo.Evento("[GeneraGastosCobranza] Obteniendo polizas ya creadas");
                                if ((!string.IsNullOrEmpty(unEvento.eventoPrincipal)))
                                {
                                    foreach (Evento evento in eventAgrupadores_)
                                    {
                                        if (evento.id_corteEventoAgrupador == unEvento.eventoPrincipal)
                                        {

                                        }
                                    }
                                }
                                continue;
                            }


                            try
                            {
                                if (Todos_losParametros["@pagoMinimoSinIntereses"].Valor == "1")
                                {
                                    sinDeuda = true;
                                }
                                if (Convert.ToDecimal(Todos_losParametros["@Importe"].Valor) == 0)
                                {
                                    if (unEvento.Stored_Procedure.Length > 0 && sinDeuda == false)
                                    {

                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                Logueo.Evento("[GeneraGastosCobranza] Generando poliza");

                                // Respuesta unaRespo =   GeneraPoliza.EjecutarEvento(unEvento.ID_Evento, 0, unEvento.ID_CadenaComercial.ToString(), unEvento.ClaveEvento, unEvento.Descripcion, "", (unEvento.Consecutivo), TodosLosParametros, unEvento, ID_Corte, ref conn2, ref transaccionSQL);
                                //LLAMA AL SCRIPT CONTABLE

                                DAOCobranza _daoCobranza = new DAOCobranza();
                                LNValidacionesCampos _validaciones = new LNValidacionesCampos();

                                DatosOperacionesExecute solicitudDatos = new DatosOperacionesExecute { claveEvento = unEvento.ClaveEventoAgrupador, importe = Todos_losParametros["@Importe"].Valor, tarjeta = cuenta.Tarjeta, tipoMedioAcceso = "TAR" };
                                DataTable tablaDatoCortes = _daoCobranza.obtenerDatosParaExecute(solicitudDatos, conn2, transaccionSQL);
                                Response errores = new Response();
                                if (!_validaciones.BusquedaSinErrores(errores, tablaDatoCortes))
                                {
                                    Logueo.Error("[GeneraGastosCobranza] Error al obtener los datos para execute: [" + unEvento.ClaveEvento + "], Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                    foreachCorrecto = false;
                                    break;
                                }
                                tablaDatoCortes = FuncionesTablas.onbtenerValoresTablaEncriptadosAE(tablaDatoCortes);

                                Bonificacion bonificacion = LNBonificacion.obtenerDatosParaDiccionario(tablaDatoCortes);
                                bonificacion.Importe = Todos_losParametros["@Importe"].Valor; // solicitudDatos.importe.ToString();
                                                                                              //bonificacion.Observaciones = retirarTarjetaEmpresa.Observaciones;
                                                                                              //bonificacion.RefNumerica = Convert.ToInt32("0");
                                Logueo.Evento("[GeneraGastosCobranza] obteniendo parametros");

                                Dictionary<String, Parametro> parametrosRetirar = _daoCobranza.ObtenerDatosParametros(bonificacion, tablaDatoCortes, conn2, transaccionSQL);
                                
                                foreach (KeyValuePair<string, Parametro> Par in Todos_losParametros)
                                {
                                    if (!parametrosRetirar.ContainsKey(Par.Key))
                                    {
                                        if (Par.Key.Contains("@"))
                                        {
                                            parametrosRetirar[Par.Key] = new Parametro()
                                            {
                                                Nombre = Todos_losParametros[Par.Key].Nombre,
                                                Valor = Todos_losParametros[Par.Key].Valor,
                                                Descripcion = Todos_losParametros[Par.Key].Descripcion

                                            };
                                        }
                                    }
                                }
                                Logueo.Evento("[GeneraEstadoCuentaCredito] Ejecutando evento");

                                // Reemplazamos el ID_Corte por 0
                                Respuesta unaRespo = GeneraPoliza.EjecutarEvento(unEvento.ID_AgrupadorEvento, 0, unEvento.ID_CadenaComercial.ToString(), unEvento.ClaveEventoAgrupador, unEvento.Descripcion, "", unEvento.Consecutivo, Todos_losParametros, unEvento, 0, conn2, transaccionSQL, bonificacion, parametrosRetirar);
                                if (unaRespo.CodigoRespuesta != 00)
                                {
                                    Logueo.Error("[GeneraGastosCobranza] La poliza no pudo crearse: [" + unEvento.ClaveEvento + "], Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                    foreachCorrecto = false;
                                    break;
                                }
                                else
                                {
                                    Logueo.Evento("[GeneraGastosCobranza] La poliza Creada: [" + unEvento.ClaveEventoAgrupador + "], Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".");
                                    //  GeneracionExitosaDePolizas = true;
                                    //Ligar las polizas a los eventos de corte
                                    // laUtil.LigarPolizasAEventoDeCorte(unEvento.ID_AgrupadorEvento, ID_Corte, transaccionSQL);
                                    // transaccionSQL.Commit();
                                    Logueo.Evento("[GeneraEstadoCuentaCredito]Relacionando fecha corte");
                                    Dictionary<String, Parametro> ParametroMonto_delEventoAgrupador = servicioDatos_.relacionarCorteEventoPolizas(ID_Corte, unEvento.ClaveEventoAgrupador, cuenta.ID_Cuenta,
                                        unaRespo.ID_Poliza.ToString(), transaccionSQL);
                                    
                                    //AGREGA A  Todos_losParametros  la CLAVE DEL EVENTO AGRUPADOR COMO PARAMETRO CON EL VALOR (TOTAL DE CARGOS DE SUS POLIZAS)
                                    // las cuales fueron generadas por el Script
                                    //foreach (string parametro in ParametroMonto_delEventoAgrupador.Keys)
                                    //{
                                    //    if (!Todos_losParametros.ContainsKey(parametro))
                                    //    {
                                    //        Todos_losParametros.Add(parametro, ParametroMonto_delEventoAgrupador[parametro]);
                                    //    }
                                    //    else
                                    //    {
                                    //        Todos_losParametros[parametro] = new Parametro() { Nombre = parametro, Valor = ParametroMonto_delEventoAgrupador[parametro].Valor };

                                    //    }
                                    //}
                                    //Logueo.Evento("[GeneraEstadoCuentaCredito]Obteniendo ivas y comisiones");
                                    ////llenando lista de factura
                                    //if ((!string.IsNullOrEmpty(unEvento.eventoPrincipal)))
                                    //{
                                    //    foreach (Evento evento in eventAgrupadores_)
                                    //    {
                                    //        if (evento.id_corteEventoAgrupador == unEvento.eventoPrincipal)
                                    //        {
                                    //            if (evento.ClaveEventoAgrupador == "INTMOR")
                                    //            {
                                    //                intMor = Convert.ToDecimal(Todos_losParametros["@INTMOR"].Valor);// Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                    //                ivaMor = Convert.ToDecimal(Todos_losParametros["@IVAINTMOR"].Valor);
                                    //            }
                                    //            string eventoIva = Todos_losParametros["@" + unEvento.ClaveEventoAgrupador].Valor;
                                    //            string eventoPrimario = Todos_losParametros["@" + evento.ClaveEventoAgrupador].Valor;
                                    //            if (evento.parametroMonto != "@INTORD" && evento.parametroMonto != "@INTMOR")
                                    //            {
                                    //                //es comision
                                    //                sumaComisiones = sumaComisiones + Convert.ToDecimal(eventoPrimario);
                                    //                ivaComision = ivaComision + Convert.ToDecimal(eventoIva);
                                    //            }
                                    //            DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = unEvento.unidadSAT, ClaveProdServ = unEvento.ClaveProdServSAT, ClaveUnidad = unEvento.ClaveUnidadSAT, NombreProducto = unEvento.descripcionEventoEdoCuenta, impImporte = eventoIva, Total = Convert.ToDecimal(eventoPrimario), PrecioUnitario = Convert.ToDecimal(eventoPrimario), impBase = eventoPrimario, impImpuesto = unEvento.impImpuestoSAT, impTipoFactor = unEvento.impTipoFactorSAT, impTasaOCuota = Todos_losParametros["@IVA"].Valor };
                                    //            listaDetallesFactura.Add(detalle);
                                    //        }
                                    //    }
                                    //}
                                    continue;
                                }
                            }
                            catch (Exception err)
                            {
                                //   transaccionSQL.Rollback();
                                // GeneracionExitosaDePolizas = false;
                                Logueo.Error("[GeneraGastosCobranza] GenerarLasPolizasDeCortes() Error se deshacen la polizas para la cadena comercial " + unEvento.ID_CadenaComercial + " :" + err.Message + "," + err.StackTrace);
                                foreachCorrecto = false;
                                break;
                            }
                        }
                    }
                }

                return foreachCorrecto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
