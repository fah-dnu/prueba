using CommonProcesador;
using DNU_ParabiliaProcesoAnualidad.CapaDatos;
using DNU_ParabiliaProcesoAnualidad.dataService;
using DNU_ParabiliaProcesoAnualidad.Entidades;
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
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;
using DNU_ParabiliaProcesoCortes.Common.Funciones;

namespace DNU_ParabiliaProcesoAnualidad.LogicaNegocio
{
    public class LNAnualidad
    {
        private string cadenaConexion;
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
                string ClaveProceso = "PROCESAANUALIDAD";
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

        public LNAnualidad()
        {
            //string ArchXSLT = "";
            try
            {
                ConfiguracionContexto.InicializarContexto();
                //ArchXSLT = PNConfig.Get("PROCESAEDOCUENTA", "ArchivoXSLT");

                cadenaConexion = PNConfig.Get("PROCESAANUALIDAD", "BDReadAutorizador");
            }
            catch (Exception es)
            {
                Logueo.Error("[GeneraGastosAnualidad] [Error al generar la anualidad] [Mensaje: " + es.Message + " TRACE: " + es.StackTrace + "]");
            }
        }

        [Transaction]
        public bool inicio(string fecha = null)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("inicio");

            string cultura = CultureInfo.CurrentCulture.Name;
            Logueo.Evento("[GeneraGastosAnualidad] culture " + cultura);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            Logueo.Evento("[GeneraGastosAnualidad] cultureMX " + cultura);
            Logueo.Evento("[GeneraGastosAnualidad] Iniciando proceso de anualidad");
            DAOAnualidad _daoCobranza = new DAOAnualidad();



            string eventoComAnual = PNConfig.Get("PROCESAANUALIDAD", "EventoComAnual");
            string eventoIvaComAnual = PNConfig.Get("PROCESAANUALIDAD", "EventoIvaComAnual");

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
                        servicioDatosAnualidad servicioDatos_ = new servicioDatosAnualidad(conn2, cadenaConexion);
                        Logueo.Evento("[GeneraGastosAnualidad] Procesando cuentas de anualidad");
                        foreach (Cuentas cuenta in servicioDatos_.Obtiene_Set_deCuentas(fecha))
                        {
                            Logueo.Evento("[GeneraGastosAnualidad] Iniciando proceso credito");
                            Logueo.Evento("[GeneraGastosAnualidad] Cuenta: " + cuenta.ID_Cuenta);

                            if (string.IsNullOrEmpty(cuenta.RFCCliente))
                            {
                                Logueo.Error("[GeneraGastosAnualidad] el cliente no cuenta con RFC, cuenta:" + cuenta.ID_Cuenta);
                                continue;
                            }


                            Dictionary<String, Parametro> Todos_losParametros = new Dictionary<string, Parametro>();
                            Dictionary<String, Parametro> Set_de_Parametros_Cuenta_MultiAsignacion;
                            Logueo.Evento("[GeneraGastosAnualidad] Obteniendo prametros, cuenta " + cuenta.ID_Cuenta.ToString());
                            Set_de_Parametros_Cuenta_MultiAsignacion = servicioDatos_.ObtieneParametros_Cuenta(cuenta.ID_Cuenta, cuenta.ID_Corte, cuenta.Tarjeta);
                            if (Set_de_Parametros_Cuenta_MultiAsignacion.Count == 1)
                            {
                                Logueo.Error("[GeneraGastosAnualidad] Error al obtener los parametros:" + Set_de_Parametros_Cuenta_MultiAsignacion["@error"].Valor);
                                continue;
                            }

                            foreach (string parametro in Set_de_Parametros_Cuenta_MultiAsignacion.Keys)
                            {
                                Todos_losParametros.Add(parametro, Set_de_Parametros_Cuenta_MultiAsignacion[parametro]);
                            }

                            Dictionary<String, Parametro> ParametrosMultiasignacion = servicioDatos_.ConsultaParametrosTarjeta(cuenta.Tarjeta, null, null, conn2);
                            if (ParametrosMultiasignacion.Count == 1)
                            {
                                Logueo.Error("[GeneraGastosAnualidad] Error al obtener los parametros:" + ParametrosMultiasignacion["@error"].ToString());
                                continue;
                            }


                            if (ParametrosMultiasignacion["@TipoAnualidad"] == null)
                            {
                                Logueo.Evento("[GeneraGastosAnualidad] El parametro @TipoAnualidad no se encuentra configurado");
                                continue;
                            }

                            string ImporteAnualidad = ParametrosMultiasignacion["@ComAnualTitular"].Valor;
                            string TipoAnualidad = ParametrosMultiasignacion["@TipoAnualidad"].Valor;

                            if (cuenta.CumpleAnio == "HOY" && TipoAnualidad == "2")
                            {
                                Logueo.Evento("[GeneraGastosAnualidad] Se cobra anualidad hasta el próximo año");
                                continue;
                            }

                            decimal dImporteAnualidad = Convert.ToDecimal(ImporteAnualidad);

                            if (dImporteAnualidad > 0)
                            {
                                Todos_losParametros["@ComAnualTitular"].Valor = ImporteAnualidad;

                                Logueo.Evento("[GeneraGastosAnualidad] Obteniendo eventos agrupadores");
                                List<Evento> eventAgrupadores_ = servicioDatos_.Obtiene_EventosAgrupadores(cuenta.ID_Cuenta, cuenta.ClaveCorteTipo, cuenta.ID_Corte, Todos_losParametros["@numeroMes"].Valor);

                                if (eventAgrupadores_.Count == 0)
                                {
                                    Logueo.Error("[GeneraGastosAnualidad] No hay eventos agrupados:" + Set_de_Parametros_Cuenta_MultiAsignacion["@error"].Valor);
                                }
                                
                                using (SqlTransaction transaccionSQL = conn2.BeginTransaction())
                                {
                                    try
                                    {
                                        string[] sEventos = new string[] { eventoComAnual, eventoIvaComAnual };
                                        bool foreachCorrecto = EjecutaEventosAgrupadores(sEventos, eventAgrupadores_, Todos_losParametros, servicioDatos_, cuenta, transaccionSQL, conn2, cuenta.ID_Corte, ImporteAnualidad, fecha);

                                        if (foreachCorrecto)
                                        {
                                            var tabla = _daoCobranza.ActualizaBanderaAnualidad(cuenta.ID_Corte, conn2, transaccionSQL);
                                            transaccionSQL.Commit();
                                            Logueo.Evento("[GeneraGastosAnualidad] Se hace Commit de la transaccion");
                                        }
                                        else
                                        {
                                            transaccionSQL.Rollback();
                                            Logueo.Evento("[GeneraGastosAnualidad] Se hace Rollback de la transaccion");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            transaccionSQL.Rollback();
                                        }
                                        catch (Exception exTransaction)
                                        {
                                            Logueo.Error("[GeneraGastosAnualidad] error al realizar rollback:" + exTransaction.Message + " " + exTransaction.StackTrace);
                                            ApmNoticeWrapper.NoticeException(exTransaction);
                                        }
                                    }
                                }
                                
                                
                                ////POR CADA CUENTA OBTIENE LOS PARAMETROS DEL CORTE ANTERIOR
                                //Logueo.Evento("[GeneraEstadoCuentaCredito] Obtener parametros corte anterior");
                                //Set_de_Parametros_Cuenta_Corte_Anterior = servicioDatos_.ObtieneParametros_Cuenta_CorteAnterior(cuenta.ID_Cuenta, Todos_losParametros["@DiaFechaCorte"].Valor, cuenta.Fecha_Corte, cuenta.ID_Corte);

                                //foreach (string parametro in Set_de_Parametros_Cuenta_Corte_Anterior.Keys)
                                //{
                                //    Todos_losParametros.Add(parametro, Set_de_Parametros_Cuenta_Corte_Anterior[parametro]);
                                //}

                                //decimal sumaComisionesConPolizaPrevia = 0;
                                //decimal sumaIvaComisionesConPolizaPrevia = 0;
                                //bool commit = false;
                                //decimal sumaComisiones = 0;
                                //decimal iva = 0;
                                //decimal ivaComision = 0;
                                //decimal ivasIntereses = 0;
                                //string fechaCreacion = "";
                                //List<string> archivos = new List<string>();
                                //DateTime Hoy = cuenta.Fecha_Corte;
                                //bool sinDeuda = false;




                            }
                            else
                            {
                                Logueo.Error("[GeneraGastosAnualidad] No generó gastos de anualidad, el parametro es :" + dImporteAnualidad.ToString());
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraGastosAnualidad] " + ex.Message);
                        ApmNoticeWrapper.NoticeException(ex);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ApmNoticeWrapper.NoticeException(ex);
                throw ex;
            }
        }

        private bool EjecutaEventosAgrupadores(string[] sEventos, List<Evento> eventAgrupadores_, Dictionary<String, Parametro> Todos_losParametros, servicioDatosAnualidad servicioDatos_,
                Cuentas cuenta, SqlTransaction transaccionSQL, SqlConnection conn2, Int64 ID_Corte, string ImporteAnualidad, string fecha)
        {
            try
            {
                bool foreachCorrecto = true;    //validara las ejecuciones correctas de todos los eventos
                                                //decimal saldoVencido = 0;
                                                //bool interesMoratorio = false;
                bool sinDeuda = false;
                Logueo.Evento("[GeneraGastosAnualidad] Procesando eventos agrupadores");
                foreach (string sEvento in sEventos)
                {
                    foreach (Evento unEvento in eventAgrupadores_)
                    {
                        if (sEvento == unEvento.ClaveEventoAgrupador)
                        {
                            Logueo.Evento("[GeneraGastosAnualidad] Evento:" + unEvento.ClaveEventoAgrupador);
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

                            //RESPECTO AL STORED PROCEDURE DEL EVENTO AGRUPADOR ACTUAL, OBTIENE SUS PARAMETROS Y TIPOS 
                            if (unEvento.Stored_Procedure.Length > 0)
                            {
                                try
                                {
                                    DataTable laTabla = null;
                                    Logueo.Evento("[GeneraGastosAnualidad] ejecutando procedimiento evento");

                                    laTabla = servicioDatos_.ejectutar_SP_EventoAgrupador(unEvento.Stored_Procedure, Todos_losParametros, transaccionSQL);

                                    for (int k = 0; k < laTabla.Rows.Count; k++)
                                    {
                                        Parametro unParamentro = new Parametro();
                                        unParamentro.Nombre = (laTabla.Rows[k]["NombreParametro"]).ToString();
                                        unParamentro.Valor = (laTabla.Rows[k]["Valor"]).ToString();

                                        //AGREGA LOS PARAMETROS DEVUELTOS...
                                        Set_de_Parametros_Evento_Agrupador.Add((laTabla.Rows[k]["NombreParametro"]).ToString(), unParamentro);
                                        Todos_losParametros.Add((laTabla.Rows[k]["NombreParametro"]).ToString(), unParamentro);
                                    }
                                }
                                catch (Exception err)
                                {
                                    Logueo.Error("[GeneraGastosAnualidad] Error al ejecutar el SP del Evento Agrupador:" + err.Message);
                                    foreachCorrecto = false;
                                    break;
                                }
                            }

                            Todos_losParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = "0.00", Descripcion = "Importe" };

                            if (!string.IsNullOrEmpty(unEvento.parametroMontoCobranza))
                            {
                                Todos_losParametros["@Importe"].Valor = Todos_losParametros[unEvento.parametroMontoCobranza].Valor;
                            }
                            else
                            {//implica que no se va hacer ninguna poliza y solo se va guardar el valor para el xml(es decir los eventos ya se han aplicado)
                                Logueo.Evento("[GeneraGastosAnualidad] Obteniendo polizas ya creadas");
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

                            //Todos_losParametros["@Importe"].Valor = ImporteAnualidad;


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
                                Logueo.Evento("[GeneraGastosAnualidad] Generando poliza");

                                // Respuesta unaRespo =   GeneraPoliza.EjecutarEvento(unEvento.ID_Evento, 0, unEvento.ID_CadenaComercial.ToString(), unEvento.ClaveEvento, unEvento.Descripcion, "", (unEvento.Consecutivo), TodosLosParametros, unEvento, ID_Corte, ref conn2, ref transaccionSQL);
                                //LLAMA AL SCRIPT CONTABLE

                                DAOAnualidad _daoCobranza = new DAOAnualidad();
                                LNValidacionesCampos _validaciones = new LNValidacionesCampos();

                                DatosOperacionesExecute solicitudDatos = new DatosOperacionesExecute { claveEvento = unEvento.ClaveEventoAgrupador, importe = Todos_losParametros["@Importe"].Valor, tarjeta = cuenta.Tarjeta, tipoMedioAcceso = "TAR" };
                                DataTable tablaDatoCortes = _daoCobranza.obtenerDatosParaExecute(solicitudDatos, conn2, transaccionSQL);
                                Response errores = new Response();
                                if (!_validaciones.BusquedaSinErrores(errores, tablaDatoCortes))
                                {
                                    Logueo.Error("[GeneraGastosAnualidad] Error al obtener los datos para execute: [" + unEvento.ClaveEvento + "], Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                    foreachCorrecto = false;
                                    break;
                                }
                                tablaDatoCortes = FuncionesTablas.onbtenerValoresTablaEncriptadosAE(tablaDatoCortes);

                                Bonificacion bonificacion = LNBonificacion.obtenerDatosParaDiccionario(tablaDatoCortes);
                                bonificacion.Importe = Todos_losParametros["@Importe"].Valor; // solicitudDatos.importe.ToString();
                                                                                              //bonificacion.Observaciones = retirarTarjetaEmpresa.Observaciones;
                                                                                              //bonificacion.RefNumerica = Convert.ToInt32("0");
                                Logueo.Evento("[GeneraGastosAnualidad] obteniendo parametros");

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
                                Logueo.Evento("[GeneraGastosAnualidad] Ejecutando evento");

                                // Reemplazamos el ID_Corte por 0
                                Respuesta unaRespo = GeneraPoliza.EjecutarEvento(unEvento.ID_AgrupadorEvento, 0, unEvento.ID_CadenaComercial.ToString(), unEvento.ClaveEventoAgrupador, unEvento.Descripcion, "", unEvento.Consecutivo, Todos_losParametros, unEvento, 0, conn2, transaccionSQL, bonificacion, parametrosRetirar);
                                if (unaRespo.CodigoRespuesta != 00)
                                {
                                    Logueo.Error("[GeneraGastosAnualidad] La poliza no pudo crearse: [" + unEvento.ClaveEvento + "], Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                    foreachCorrecto = false;
                                    break;
                                }
                                else
                                {
                                    Logueo.Evento("[GeneraGastosAnualidad] La poliza Creada: [" + unEvento.ClaveEventoAgrupador + "], Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".");

                                    Logueo.Evento("[GeneraGastosAnualidad] Relacionando fecha corte");
                                    Dictionary<String, Parametro> ParametroMonto_delEventoAgrupador = servicioDatos_.relacionarCorteEventoPolizas(ID_Corte, unEvento.ClaveEventoAgrupador, cuenta.ID_Cuenta,
                                        unaRespo.ID_Poliza.ToString(), transaccionSQL, fecha);

                                    continue;
                                }
                            }
                            catch (Exception err)
                            {
                                Logueo.Error("[GeneraGastosAnualidad] GenerarLasPolizasDeCortes() Error se deshacen la polizas para la cadena comercial " + unEvento.ID_CadenaComercial + " :" + err.Message + "," + err.StackTrace);
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
