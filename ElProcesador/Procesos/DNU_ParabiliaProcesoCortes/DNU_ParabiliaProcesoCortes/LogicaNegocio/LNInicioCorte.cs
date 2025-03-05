using CommonProcesador;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
//using DALAutorizador.Entidades;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.LogicaNegocio;
using DNU_ParabiliaProcesoCortes.Reportes;
using DNU_ParabiliaProcesoCortes.wsFacturacionMySuit;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
//using Colectiva = DNU_ParabiliaProcesoCortes.Entidades.Colectiva;
//using CrystalDecisions.CrystalReports.Engine;
//using CrystalDecisions.Shared;
using DNU_ParabiliaProcesoCortes.ReporteTipado;
using DNU_ParabiliaProcesoCortes.ReporteDebito;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using log4net;
using DNU_ParabiliaProcesoCortes.Utilidades;
//using DALAutorizador.Entidades;

namespace DNU_ParabiliaProcesoCortes.CapaNegocio
{
    class LNInicioCorte
    {
        // private static string connArchivosCacao = PNConfig.Get("ALTAEMPLEADOCACAO", "BDWriteProcesadorArchivosCacao");
        //  private static string cadenaConexion = "Data Source=45.32.4.114;initial catalog=Auto_producto;user id=OG;password=Og12369;";
        private string cadenaConexion;// = "Data Source=45.32.4.114;initial catalog=Auto_producto;user id=OG;password=Og12369;";
        static XslCompiledTransform _transformador = new XslCompiledTransform();
        static string rutas = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        //static LNInicioCorte()
        //{
        //    string ArchXSLT = Path.GetFullPath(Path.Combine(rutas, @"..\..\..\"));
        //    ArchXSLT = Path.GetFullPath(Path.Combine(ArchXSLT, @"DNU_ParabiliaProcesoCortes\Certificados\"));
        //    ArchXSLT = ArchXSLT + "CadenaOriginal_3_3.xslt";
        //    _transformador.Load(ArchXSLT);

        //}

        public LNInicioCorte()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            string ArchXSLT = "";
            try
            {
                ConfiguracionContexto.InicializarContexto();
                ArchXSLT = PNConfig.Get("PROCESAEDOCUENTA", "ArchivoXSLT");

                //  ArchXSLT = Path.GetFullPath(Path.Combine(ArchXSLT, @"DNU_ParabiliaProcesoCortes\Certificados\"));
                //  ArchXSLT = ArchXSLT + "CadenaOriginal_3_3.xslt";
                _transformador = new XslCompiledTransform();
                cadenaConexion = PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizador");
                _transformador.Load(ArchXSLT);

            }
            catch (Exception es)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporte, Error al generar el corte, Mensaje: " + es.Message + " TRACE: " + es.StackTrace + ", " + ArchXSLT + "]");
            }
        }
        public LNInicioCorte(string idCorte, string cadenaConexionExterna = null, string rutaArchivos = null, Boolean envioCorreo = false, RespuestaSolicitud respuestaSolicitud = null, bool esDebito = false)
        {            
            ConfiguracionContexto.InicializarContexto();
            if (string.IsNullOrEmpty(cadenaConexionExterna))
            {
                cadenaConexion = PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizador");
            }
            else
            {
                cadenaConexion = cadenaConexionExterna;
            }
            this.id = idCorte;
            this.envioCorreo = envioCorreo;
            this.respuestaSolicitud = respuestaSolicitud;
            this.esDebito = esDebito;
            rutaArchivosExterna = rutaArchivos;
        }
        RespuestaSolicitud respuestaSolicitud;
        string id;
        bool envioCorreo;
        string rutaArchivosExterna;
        bool esDebito;
        public bool inicio(string fecha = null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
        
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Iniciando proceso de corte]");
            //Logueo.Evento("Iniciando proceso de corte");
            DAOCortes _daoCortes = new DAOCortes();
            LNValidacionesCampos _validaciones = new LNValidacionesCampos();
            LNEnvioCorreo _lnEnvioCorreo = new LNEnvioCorreo();
            LNOperaciones lnOperaciones = new LNOperaciones();
            DAOFacturas _daoFacturas = new DAOFacturas();

            //
            string ruta = "";
            if (string.IsNullOrEmpty(rutaArchivosExterna))
            {
                ruta = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalida");
            }
            else
            {
                ruta = rutaArchivosExterna;
            }
            string rutaImagen = ruta.Replace("Facturas\\", "") + "LogosClientes\\";
            //LNOperaciones.crearDirectorio(rutaImagen);
            bool pdfGeneradoCorrectamente = false;
            string nombrePdfCredito = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFCredito");
            string nombrePdfPrepago = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFDebito");
            try
            {
                pdfGeneradoCorrectamente = LNOperacionesCorte.GeneracionPDFSinCorte(id, cadenaConexion, fecha, ruta, rutaImagen, envioCorreo, respuestaSolicitud, _lnEnvioCorreo, _daoCortes, nombrePdfCredito, nombrePdfPrepago);
            }
            catch (Exception ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporte, Error al generar el PDF o envio de correo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
           }
            if (!string.IsNullOrEmpty(id))
            {
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Fin proceso corte]");
                return pdfGeneradoCorrectamente;
            }

            //Procesando cuentas
            //lo de arriba solo es para procesar el pdf y el envio del correo, lo que sigue ya es el proceso de generacion
            // LNBonificacion _lnBonificacion = new LNBonificacion();
            string urlAccesoServicioSAT = PNConfig.Get("PROCESAEDOCUENTA", "URLAccesoAlServicioSAT"); //ConfigurationManager.AppSettings["URLAccesoAlServicioSAT"].ToString();
            ruta = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalida");
            string eventoIntOrd = PNConfig.Get("PROCESAEDOCUENTA", "EventoIntOrd");
            string eventoIvaIntOrd = PNConfig.Get("PROCESAEDOCUENTA", "EventoIvaIntOrd");
            string eventoIntMor = PNConfig.Get("PROCESAEDOCUENTA", "EventoIntMor");
            string eventoIvaIntMor = PNConfig.Get("PROCESAEDOCUENTA", "EventoIvaIntMor");
            string eventoComPagoTardio = PNConfig.Get("PROCESAEDOCUENTA", "EventoComPagoTardio");
            string eventoIvaComPagoTardio = PNConfig.Get("PROCESAEDOCUENTA", "EventoIvaComPagoTardio");
            string eventoComPorNoPago = PNConfig.Get("PROCESAEDOCUENTA", "EventoComPorNoPago");
            string eventoIvaComPorNoPago = PNConfig.Get("PROCESAEDOCUENTA", "EventoIvaComPorNoPago");
            string eventoComAnual = PNConfig.Get("PROCESAEDOCUENTA", "EventoComAnual");
            string eventoIvaComAnual = PNConfig.Get("PROCESAEDOCUENTA", "EventoIvaComAnual");
            string eventoComAnualAdicional = PNConfig.Get("PROCESAEDOCUENTA", "EventoComAnualAdicional");
            string eventoIvaComAnualAdicional = PNConfig.Get("PROCESAEDOCUENTA", "EventoIvaComAnualAdicional");
            string eventoComReposicion = PNConfig.Get("PROCESAEDOCUENTA", "EventoComReposicion");
            string eventoIvaComReposicion = PNConfig.Get("PROCESAEDOCUENTA", "EventoIvaComReposicion");
            string envioCorreoCredito = PNConfig.Get("PROCESAEDOCUENTA", "EnvioCorreoCredito");

            //LNFacturas.generaCodigoQR(ruta+"prueba.png", "   ");
            //prueba crystal reports
            string userCR = "";
            string passCR = "";
            string hostCR = "";
            string databaseCR = "";

            try
            {//aqui no deberia de entrar pero estaba cuando se tomo el codigo
                respuestaSolicitud = new RespuestaSolicitud();

                using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                {
                    conn2.Open();
                    try
                    {
                        //OBTIENE EL SET DE CUENTAS A CORTAR HOY
                        servicioDatos servicioDatos_ = new servicioDatos(conn2, cadenaConexion);
                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Procesando corte]");
                        List<DetalleFactura> listaDetallesFactura = new List<DetalleFactura>();
                        List<DetalleFactura> listaDetallesXML = new List<DetalleFactura>();
                        int incrementoFolio = 0;
                        foreach (Cuentas cuenta in servicioDatos_.Obtiene_Set_deCuentas(fecha))
                        {
                            listaDetallesFactura.Clear();
                            listaDetallesXML.Clear();
                            ruta = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalida");
                            rutaImagen = ruta.Replace("Facturas\\", "") + "LogosClientes\\";
                            _daoCortes = new DAOCortes();

                            //prepago
                            if (cuenta.ClaveCorteTipo == "MTD001")
                            {
                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Proceso prepago]");
                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Procesando corte]");
                                string nombreEstado = "";
                                try
                                {
                                    nombreEstado = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFDebito");
                                }
                                catch (Exception ex)
                                {
                                    nombreEstado = "EstadoDeMovimientos.pdf";
                                }
                                try
                                {
                                    DateTime HoyDebito = cuenta.Fecha_Corte;
                                    string mes = HoyDebito.ToString("MM");
                                    if (mes.Length == 1)
                                    {
                                        mes = "0" + mes;
                                    }
                                    string anio = HoyDebito.ToString("yyyy");
                                    if (ruta[ruta.Length - 1] == '\\')
                                    {
                                        ruta = ruta + "\\";
                                    }
                                    ruta = ruta + cuenta.ClaveCliente + "\\" + cuenta.ClaveCuentahabiente + "\\" + cuenta.ID_Cuenta + "\\" + anio + "\\" + mes + "\\";
                                    DataTable tablaNuevoCorte = _daoCortes.GenerarNuevoCorteDebito(Convert.ToInt64(cuenta.ID_Corte), ruta, conn2);
                                    String idNuevoCorte = "";
                                    if (tablaNuevoCorte.Columns.Count == 2 && tablaNuevoCorte.Rows[0][0].ToString() == "Correcto")
                                    {
                                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Nuevo corte generado]");
                                       idNuevoCorte = tablaNuevoCorte.Rows[0][1].ToString();

                                        rutaImagen = rutaImagen + cuenta.ClaveCliente;
                                        LNOperaciones.crearDirectorio(rutaImagen);
                                        LNOperaciones.crearDirectorio(ruta);
                                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Generando PDF]");
                                       //DataTable dt = new DataTable("TablaCliente");
                                        DataSet ds = _daoCortes.ObtenerDatosPagosDebito(idNuevoCorte, cadenaConexion);
                                        ds = _daoCortes.ObtenerDatosCHDebito(idNuevoCorte, cadenaConexion, null, ds);
                                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Datos obtenidos]");
                                       //  DataSetOperacionesEdoCuenta dataSet = new DataSetOperacionesEdoCuenta();
                                        DNU_ParabiliaProcesoCortes.ReporteDebito.clip.EdoCuentaTipDebito estadoDeCuenta = new DNU_ParabiliaProcesoCortes.ReporteDebito.clip.EdoCuentaTipDebito();
                                        estadoDeCuenta.SetDataSource(ds);
                                        //    estadoDeCuenta.SetParameterValue("imagenLogo", rutaImagen + "\\logo.jpg");//\\ArchivosCortesCacao\\Facturas\\dnuLogo.png");
                                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, generando pdf en carpeta]");
                                       estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + nombreEstado);//"ResumenDeMovimientos.pdf");
                                        estadoDeCuenta.Close();
                                        estadoDeCuenta.Dispose();
                                        pdfGeneradoCorrectamente = true;

                                    }
                                    if (pdfGeneradoCorrectamente)
                                    {
                                        envioCorreo = false;
                                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, PDF generado]");
                                        ruta = ruta + nombreEstado;//"ResumenDeMovimientos.pdf";
                                        if (File.Exists(ruta))
                                        {
                                            List<String> ArchivosCorreo = new List<String>();
                                            ArchivosCorreo.Add(ruta);
                                            if (!string.IsNullOrEmpty(cuenta.CorreoCuentahabiente))
                                            {
                                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Enviando correo]");
                                                string fechaCreacionCorreo = HoyDebito.ToString("dd-MM-yyyy");
                                                Correo _correo = new Correo
                                                {
                                                    asunto = "Estado de cuenta del" + fechaCreacionCorreo,
                                                    correoReceptor = cuenta.CorreoCuentahabiente,
                                                    mensaje = PNConfig.Get("PROCESAEDOCUENTA", "AsuntoCorreo"),
                                                    archivos = ArchivosCorreo,
                                                    titulo = PNConfig.Get("PROCESAEDOCUENTA", "AsuntoCorreo"),
                                                    correoEmisor = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioCorreo"),
                                                    cuerpoMensaje = PNConfig.Get("PROCESAEDOCUENTA", "BodyCorreoElectronico"),
                                                    host = PNConfig.Get("PROCESAEDOCUENTA", "HostCorreo"),
                                                    password = PNConfig.Get("PROCESAEDOCUENTA", "PasswordCorreo"),
                                                    puerto = PNConfig.Get("PROCESAEDOCUENTA", "PuertoCorreo"),
                                                    usuario = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioCorreo")
                                                }; //"Estado De Cuenta Cacao" };
                                                envioCorreo = _lnEnvioCorreo.envioCorreo(_correo, respuestaSolicitud);
                                                if (envioCorreo)
                                                {
                                                    LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Correo Enviado]");
                                                    respuestaSolicitud.codigoRespuesta = "0000";
                                                    respuestaSolicitud.descripcionRespuesta = "Correcto";
                                                }
                                                else
                                                {
                                                    respuestaSolicitud.codigoRespuesta = "0003";
                                                    respuestaSolicitud.descripcionRespuesta = "Error al generar Estado de cuenta";
                                                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Error al generar el PDF o envio de correo, Mensaje:No se envío el correo]");
                                                  
                                                }
                                            }
                                            else
                                            {
                                                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Error al generar el PDF o envio de correo, Mensaje:El cuentahabiente no cuenta con correo]");
                                                respuestaSolicitud = new RespuestaSolicitud { codigoRespuesta = "0002", descripcionRespuesta = "El cuentahabiente no cuenta con correo" };
                                            }
                                            //insertar registro de correo y generacion en la base de datos
                                            //acrtualizando estatus


                                        }
                                        else
                                        {
                                            respuestaSolicitud.codigoRespuesta = "0003";
                                            respuestaSolicitud.descripcionRespuesta = "No existe el Estado de cuenta";
                                            LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Error al generar el PDF o envio de correo, Mensaje:No existe el Estado de cuenta]");
                                          
                                        }
                                    }
                                    else
                                    {
                                        respuestaSolicitud.codigoRespuesta = "0003";
                                        respuestaSolicitud.descripcionRespuesta = "Error al generar Estado de cuenta";
                                        LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Error al generar el PDF o envio de correo, Mensaje:No se generó el pdf]");
                                      
                                    }
                                    // if (envioCorreo || pdfGeneradoCorrectamente)
                                    // {
                                    LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Actualizando registro envio pdf y correo:" + idNuevoCorte + "," + pdfGeneradoCorrectamente + "," + envioCorreo + "]");
                                    _daoCortes.ActualizarDatoCorreoYPDF(pdfGeneradoCorrectamente, envioCorreo, Convert.ToInt64(idNuevoCorte), conn2);
                                    LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, registro envio pdf y correo actualizado]");
                                    // }


                                }
                                catch (Exception exDebito)
                                {
                                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraReporteEstadoCuenta, Error al generar el estado de cuenta:" + exDebito.Message + " " + exDebito.StackTrace + "]");
                                  
                                }
                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Fin proceso corte debito]");
                                continue;
                            }

                            //credito
                            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Iniciando Proceso Credito]");
                            if (string.IsNullOrEmpty(cuenta.RFCCliente))
                            {
                                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito] el cliente no cuenta com RFC, cuenta:" + cuenta.ID_Cuenta + "]");
                             
                            }
                            incrementoFolio = incrementoFolio + 1;
                            string inicioFolio = cuenta.ClaveCliente.Replace("_", "").PadRight(8, '0').Substring(0, 8);
                            DateTime fechaFolio = DateTime.Now;//DateTime.Today;
                            string fecha_actual = fechaFolio.ToString("yyyyMMddhhmmss");//5491XXXXXXXX6877
                            String folioFactura = inicioFolio + fecha_actual + (incrementoFolio.ToString().PadLeft(3, '0').Substring(0, 3));
                            //string folio = 
                            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito,  Generacion Folio]");
                            Dictionary<String, Parametro> Todos_losParametros = new Dictionary<string, Parametro>();
                            Todos_losParametros["@ID_Cuenta"] = new Parametro { Nombre = "@ID_Cuenta", Valor = cuenta.ID_Cuenta.ToString(), Descripcion = "id de la cuenta" };
                            // Todos_losParametros["@ID_CadenaComercial"] = new Parametro() { Nombre = "@ID_CadenaComercial", Valor = cuenta.id_CadenaComercial.ToString(), Descripcion = "id cadena comercial" };
                            Todos_losParametros["@Fecha_Corte"] = new Parametro() { Nombre = "@Fecha_Corte", Valor = cuenta.Fecha_Corte.ToString(), Descripcion = "Fecha corte" };
                            Todos_losParametros["@ID_CuentaHabiente"] = new Parametro() { Nombre = "@ID_CuentaHabiente", Valor = cuenta.ID_CuentaHabiente.ToString(), ID_TipoColectiva = cuenta.ID_TipoColectiva, Descripcion = " " };
                            Todos_losParametros["@ID_CadenaComercial"] = new Parametro() { Nombre = "@ID_CadenaComercial", Valor = cuenta.id_CadenaComercial.ToString(), Descripcion = " " };
                            Todos_losParametros["@ClaveCorteTipo"] = new Parametro() { Nombre = "@ClaveCorteTipo", Valor = cuenta.ClaveCorteTipo.ToString(), Descripcion = " " };
                            Todos_losParametros["@Tarjeta"] = new Parametro() { Nombre = "@Tarjeta", Valor = cuenta.Tarjeta.ToString(), Descripcion = " " };
                            Dictionary<String, Parametro> Set_de_Parametros_Cuenta_MultiAsignacion;
                            Dictionary<String, Parametro> Set_de_Parametros_Cuenta_Corte_Anterior;


                            //POR CADA CUENTA OBTIENE SUS PARAMETROS MULTIASIGNACION Y SALDO DEL CORTE ANTERIOR
                            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Obteniendo Parametros, cuenta " + cuenta.ID_Cuenta.ToString() + "]");
                           Set_de_Parametros_Cuenta_MultiAsignacion = servicioDatos_.ObtieneParametros_Cuenta(cuenta.ID_Cuenta, cuenta.ID_Corte, cuenta.Tarjeta);
                            if (Set_de_Parametros_Cuenta_MultiAsignacion.Count == 1)
                            {
                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Error al obtener los parametros:" + Set_de_Parametros_Cuenta_MultiAsignacion["@error"].Valor + "]");
                                 continue;
                            }
                            foreach (string parametro in Set_de_Parametros_Cuenta_MultiAsignacion.Keys)
                            {
                                Todos_losParametros.Add(parametro, Set_de_Parametros_Cuenta_MultiAsignacion[parametro]);
                            }
                            //recorriendo el dictionary
                            bool saldoInsolutoPagado = Todos_losParametros["@pagoMinimoSinIntereses"].Valor == "1" ? true : false;

                            //obteniendo las imagenes del estado de cuenta
                            string rutaImagenCAT = _validaciones.validaParametroDiccionario(Todos_losParametros, "@ImagenCat") ? Todos_losParametros["@ImagenCat"].Valor : "";
                            string rutaImagenUNE = _validaciones.validaParametroDiccionario(Todos_losParametros, "@ImagenDatosCondusef") ? Todos_losParametros["@ImagenDatosCondusef"].Valor : "";
                            string rutaImagenLogo = _validaciones.validaParametroDiccionario(Todos_losParametros, "@ImagenLogoCredito") ? Todos_losParametros["@ImagenLogoCredito"].Valor : "";

                            // POR CADA CUENTA OBTIENE SUS PARAMETROS DEL CORTE ANTERIOR
                            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Obtener parametros corte anterior]");
                            Set_de_Parametros_Cuenta_Corte_Anterior = servicioDatos_.ObtieneParametros_Cuenta_CorteAnterior(cuenta.ID_Cuenta, Todos_losParametros["@DiaFechaCorte"].Valor, cuenta.Fecha_Corte, cuenta.ID_Corte);



                            foreach (string parametro in Set_de_Parametros_Cuenta_Corte_Anterior.Keys)
                            {
                                Todos_losParametros.Add(parametro, Set_de_Parametros_Cuenta_Corte_Anterior[parametro]);


                            }


                            Int64 ID_Corte = 0;
                            DAOUtilerias laUtil = new DAOUtilerias(conn2);
                            //inicia transacciones para comit
                            decimal sumaComisionesConPolizaPrevia = 0;
                            decimal sumaIvaComisionesConPolizaPrevia = 0;
                            bool commit = false;
                            decimal sumaComisiones = 0;
                            decimal iva = 0;
                            decimal ivaComision = 0;
                            decimal ivasIntereses = 0;
                            string fechaCreacion = "";
                            List<string> archivos = new List<string>();
                            DateTime Hoy = cuenta.Fecha_Corte;
                            bool sinDeuda = false;
                            using (SqlTransaction transaccionSQL = conn2.BeginTransaction())//System.Data.IsolationLevel.ReadCommitted
                            {
                                try
                                {
                                    LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Insertando Corte]");
                                    //INSERTA EL CORTE.
                                    ID_Corte = laUtil.InsertaCorteCuenta(cuenta.ClaveCorteTipo, cuenta.Fecha_Corte, cuenta.ID_Cuenta, Todos_losParametros["@Fecha_Final"].Valor,
                                                         Todos_losParametros["@DiaFechaCorte"].Valor, Todos_losParametros["@Fecha_Inicial"].Valor, Todos_losParametros["@fechaLimiteDePago"].Valor, cuenta.ID_Corte, transaccionSQL, Todos_losParametros["@numeroMes"].Valor);
                                    //POR CADA CUENTA OBTIENE LOS EVENTOS AGRUPADORES INVOLUCRADOS EN EL CORTE A PROCESAR
                                    if (ID_Corte > 1)
                                    {
                                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Obteniendo eventos agrupadores]");
                                        List<Evento> eventAgrupadores_ = servicioDatos_.Obtiene_EventosAgrupadores(cuenta.ID_Cuenta, cuenta.ClaveCorteTipo, cuenta.ID_Corte, Todos_losParametros["@numeroMes"].Valor);
                                        bool foreachCorrecto = true;//validara las ejecuciones correctas de todos los eventos
                                        decimal saldoVencido = 0;
                                        bool interesMoratorio = false;
                                        if (eventAgrupadores_.Count == 0)
                                        {
                                            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, No hay eventos agrupados:" + Set_de_Parametros_Cuenta_MultiAsignacion["@error"].Valor + "]");
                                            //Logueo.Error("[GeneraEstadoCuentaCredito] No hay eventos agrupados:" + Set_de_Parametros_Cuenta_MultiAsignacion["@error"].Valor);
                                            foreachCorrecto = false;
                                            //    continue;
                                        }
                                        //aqui van todos los datos del contrato pues siempre seran los mismos para una tarjeta


                                        //fin
                                        decimal ivaOrd = 0;// = Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                        decimal ivaMor = 0;// = Convert.ToDecimal(Todos_losParametros["@IVAINTMOR"].Valor);
                                        decimal intOrd = 0;// = Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                        decimal intMor = 0;

                                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Procesando eventos agrupadores]");
                                       foreach (Evento unEvento in eventAgrupadores_)
                                        {
                                            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Evento:" + unEvento.ClaveEventoAgrupador + "]");
                                           //diccionario que va contener  todos los parametros a enviar al ejecutor  
                                            Dictionary<String, Parametro> Set_de_Parametros_Evento_Agrupador = new Dictionary<string, Parametro>();
                                            Todos_losParametros["@" + unEvento.ClaveEventoAgrupador] = new Parametro() { Nombre = "@" + unEvento.ClaveEventoAgrupador, Valor = "0.00" };
                                            //  OJO!!!  INICIAL EL LLENADO  DEL SET DE PARAMETROS PARA EL EVENTO!!!!
                                            // OBTIEN SUS PARAMETROS CONTRATO

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
                                                    // List<Evento> larespuesta = new List<Evento>();
                                                    DataTable laTabla = null;
                                                    LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito,  ejecutando procedimiento evento]");
                                                  
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
                                                    //   transaccionSQL.Rollback();
                                                    // GeneracionExitosaDePolizas = false;
                                                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito Error al ejecutar el SP del Evento Agrupador:" + err.Message + "]");
                                                    foreachCorrecto = false;
                                                    break;
                                                }
                                            }
                                            Todos_losParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = "0.00", Descripcion = "Importe" };

                                            if (!string.IsNullOrEmpty(unEvento.parametroMonto))
                                            {
                                                Todos_losParametros["@Importe"].Valor = Todos_losParametros[unEvento.parametroMonto].Valor;
                                            }
                                            else
                                            {//implica que no se va hacer ninguna poliza y solo se va guardar el valor para el xml(es decir los eventos ya se han aplicado)
                                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Obteniendo polizas ya creadas]");
                                                if ((!string.IsNullOrEmpty(unEvento.eventoPrincipal)))
                                                {
                                                    foreach (Evento evento in eventAgrupadores_)
                                                    {
                                                        if (evento.id_corteEventoAgrupador == unEvento.eventoPrincipal)
                                                        {
                                                            // if (string.IsNullOrEmpty(Todos_losParametros["@" + evento.ClaveEvento].Valor))
                                                            //{

                                                            DataTable tablaDatoFactura = _daoFacturas.ObtenerDatosMovimientosEvento(evento.ClaveEventoAgrupador, unEvento.ClaveEventoAgrupador, cuenta.ID_Cuenta.ToString(), ID_Corte.ToString(), conn2, transaccionSQL);
                                                            Response errores = new Response();
                                                            if (_validaciones.BusquedaSinErrores(errores, tablaDatoFactura))
                                                            {
                                                                if (Convert.ToDecimal(tablaDatoFactura.Rows[0][2]) > 0)
                                                                {
                                                                    string eventoIva = tablaDatoFactura.Rows[0][2].ToString();
                                                                    string eventoPrimario = tablaDatoFactura.Rows[0][1].ToString();
                                                                    if (evento.parametroMonto != "@INTORD" && evento.parametroMonto != "@INTMOR")
                                                                    {
                                                                        //es comision
                                                                        sumaComisionesConPolizaPrevia = sumaComisionesConPolizaPrevia + Convert.ToDecimal(eventoPrimario);
                                                                        sumaIvaComisionesConPolizaPrevia = sumaIvaComisionesConPolizaPrevia + Convert.ToDecimal(eventoIva);
                                                                        //sumaComisiones = sumaComisiones + Convert.ToDecimal(eventoPrimario);
                                                                        //ivaComision = ivaComision + Convert.ToDecimal(eventoIva);
                                                                    }
                                                                    DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = unEvento.unidadSAT, ClaveProdServ = unEvento.ClaveProdServSAT, ClaveUnidad = unEvento.ClaveUnidadSAT, NombreProducto = unEvento.descripcionEventoEdoCuenta, impImporte = tablaDatoFactura.Rows[0][2].ToString(), Total = Convert.ToDecimal(tablaDatoFactura.Rows[0][1].ToString()), PrecioUnitario = Convert.ToDecimal(tablaDatoFactura.Rows[0][1].ToString()), impBase = tablaDatoFactura.Rows[0][1].ToString(), impImpuesto = unEvento.impImpuestoSAT, impTipoFactor = unEvento.impTipoFactorSAT, impTasaOCuota = Todos_losParametros["@IVA"].Valor };
                                                                    listaDetallesFactura.Add(detalle);
                                                                }
                                                            }

                                                            //}
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
                                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Generando poliza]");
                                                
                                                // Respuesta unaRespo =   GeneraPoliza.EjecutarEvento(unEvento.ID_Evento, 0, unEvento.ID_CadenaComercial.ToString(), unEvento.ClaveEvento, unEvento.Descripcion, "", (unEvento.Consecutivo), TodosLosParametros, unEvento, ID_Corte, ref conn2, ref transaccionSQL);
                                                //LLAMA AL SCRIPT CONTABLE
                                                DatosOperacionesExecute solicitudDatos = new DatosOperacionesExecute { claveEvento = unEvento.ClaveEventoAgrupador, importe = Todos_losParametros["@Importe"].Valor, tarjeta = cuenta.Tarjeta, tipoMedioAcceso = "TAR" };
                                                DataTable tablaDatoCortes = _daoCortes.obtenerDatosParaExecute(solicitudDatos, conn2, transaccionSQL);
                                                Response errores = new Response();
                                                if (!_validaciones.BusquedaSinErrores(errores, tablaDatoCortes))
                                                {
                                                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Error al obtener los datos para execute: " + unEvento.ClaveEvento + ", Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial]");
                                                    foreachCorrecto = false;
                                                    break;
                                                }
                                                Bonificacion bonificacion = LNBonificacion.obtenerDatosParaDiccionario(tablaDatoCortes);
                                                bonificacion.Importe = Todos_losParametros["@Importe"].Valor; // solicitudDatos.importe.ToString();
                                                                                                              //bonificacion.Observaciones = retirarTarjetaEmpresa.Observaciones;
                                                                                                              //bonificacion.RefNumerica = Convert.ToInt32("0");
                                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, obteniendo paraetros]");
                                               
                                                Dictionary<String, Parametro> parametrosRetirar = _daoCortes.ObtenerDatosParametros(bonificacion, tablaDatoCortes, conn2, transaccionSQL);


                                                foreach (KeyValuePair<string, Parametro> Par in Todos_losParametros)
                                                {
                                                    if (!parametrosRetirar.ContainsKey(Par.Key))
                                                    {
                                                        //if (Par.Value == "0") {
                                                        if (Par.Key.Contains("@"))
                                                        {
                                                            parametrosRetirar[Par.Key] = new Parametro()
                                                            {
                                                                Nombre = Todos_losParametros[Par.Key].Nombre,
                                                                Valor = Todos_losParametros[Par.Key].Valor,
                                                                Descripcion = Todos_losParametros[Par.Key].Descripcion

                                                            };
                                                        }
                                                        //}
                                                    }
                                                }
                                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Ejecutando evento]");
                                               
                                                Respuesta unaRespo = GeneraPoliza.EjecutarEvento(unEvento.ID_AgrupadorEvento, 0, unEvento.ID_CadenaComercial.ToString(), unEvento.ClaveEventoAgrupador, unEvento.Descripcion, "", unEvento.Consecutivo, Todos_losParametros, unEvento, ID_Corte, conn2, transaccionSQL, bonificacion, parametrosRetirar);
                                                if (unaRespo.CodigoRespuesta != 00)
                                                {
                                                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, La poliza no pudo crearse: " + unEvento.ClaveEvento + ", Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial]");
                                                   // GeneracionExitosaDePolizas = false;
                                                    //transaccionSQL.Rollback();
                                                    foreachCorrecto = false;
                                                    break;
                                                    //continue;
                                                }
                                                else
                                                {
                                                    LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, La poliza Creada: " + unEvento.ClaveEventoAgrupador + ", Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + "." + "]");
                                                    //  GeneracionExitosaDePolizas = true;
                                                    //Ligar las polizas a los eventos de corte
                                                    // laUtil.LigarPolizasAEventoDeCorte(unEvento.ID_AgrupadorEvento, ID_Corte, transaccionSQL);
                                                    // transaccionSQL.Commit();
                                                    LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Relacionando fecha corte]");
                                                    Dictionary<String, Parametro> ParametroMonto_delEventoAgrupador = servicioDatos_.relacionarCorteEventoPolizas(ID_Corte, unEvento.ClaveEventoAgrupador, cuenta.ID_Cuenta,
                                                     cuenta.Fecha_Corte, Todos_losParametros["@DiaFechaCorte"].Valor, Todos_losParametros["@Fecha_Inicial"].Valor,
                                                     Todos_losParametros["@Fecha_Final"].Valor, unaRespo.ID_Poliza.ToString(), transaccionSQL, Todos_losParametros["@Fecha_LimitePago"].Valor);
                                                    //AGREGA A  Todos_losParametros  la CLAVE DEL EVENTO AGRUPADOR COMO PARAMETRO CON EL VALOR (TOTAL DE CARGOS DE SUS POLIZAS)
                                                    // las cuales fueron generadas por el Script
                                                    foreach (string parametro in ParametroMonto_delEventoAgrupador.Keys)
                                                    {
                                                        if (!Todos_losParametros.ContainsKey(parametro))
                                                        {
                                                            Todos_losParametros.Add(parametro, ParametroMonto_delEventoAgrupador[parametro]);
                                                        }
                                                        else
                                                        {
                                                            Todos_losParametros[parametro] = new Parametro() { Nombre = parametro, Valor = ParametroMonto_delEventoAgrupador[parametro].Valor };

                                                        }
                                                    }
                                                    LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Obteniendo ivas y comisiones]");
                                                    //llenando lista de factura
                                                    if ((!string.IsNullOrEmpty(unEvento.eventoPrincipal)))
                                                    {
                                                        foreach (Evento evento in eventAgrupadores_)
                                                        {
                                                            if (evento.id_corteEventoAgrupador == unEvento.eventoPrincipal)
                                                            {
                                                                if (evento.ClaveEventoAgrupador == "INTMOR")
                                                                {
                                                                    intMor = Convert.ToDecimal(Todos_losParametros["@INTMOR"].Valor);// Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                                                    ivaMor = Convert.ToDecimal(Todos_losParametros["@IVAINTMOR"].Valor);
                                                                }
                                                                string eventoIva = Todos_losParametros["@" + unEvento.ClaveEventoAgrupador].Valor;
                                                                string eventoPrimario = Todos_losParametros["@" + evento.ClaveEventoAgrupador].Valor;
                                                                if (evento.parametroMonto != "@INTORD" && evento.parametroMonto != "@INTMOR")
                                                                {
                                                                    //es comision
                                                                    sumaComisiones = sumaComisiones + Convert.ToDecimal(eventoPrimario);
                                                                    ivaComision = ivaComision + Convert.ToDecimal(eventoIva);
                                                                }
                                                                DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = unEvento.unidadSAT, ClaveProdServ = unEvento.ClaveProdServSAT, ClaveUnidad = unEvento.ClaveUnidadSAT, NombreProducto = unEvento.descripcionEventoEdoCuenta, impImporte = eventoIva, Total = Convert.ToDecimal(eventoPrimario), PrecioUnitario = Convert.ToDecimal(eventoPrimario), impBase = eventoPrimario, impImpuesto = unEvento.impImpuestoSAT, impTipoFactor = unEvento.impTipoFactorSAT, impTasaOCuota = Todos_losParametros["@IVA"].Valor };
                                                                listaDetallesFactura.Add(detalle);
                                                            }
                                                        }
                                                    }
                                                    continue;
                                                }
                                            }
                                            catch (Exception err)
                                            {
                                                //   transaccionSQL.Rollback();
                                                // GeneracionExitosaDePolizas = false;
                                                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, GenerarLasPolizasDeCortes() Error se deshacen la polizas para la cadena comercial " + unEvento.ID_CadenaComercial + " :" + err.Message + "," + err.StackTrace + "]");
                                                foreachCorrecto = false;
                                                break;
                                            }

                                        }

                                        if (foreachCorrecto)
                                        {
                                            ivaOrd = Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                            intOrd = Convert.ToDecimal(Todos_losParametros["@INTORD"].Valor);
                                        }

                                        //iva = Convert.ToDecimal(Todos_losParametros["@IVA"].Valor);
                                        //ivaComision = sumaComisiones * iva;
                                        //ivasIntereses = ivaOrd + ivaMor;

                                        iva = Convert.ToDecimal(Todos_losParametros["@IVA"].Valor);
                                        ivasIntereses = ivaOrd + ivaMor;
                                        sumaComisiones = sumaComisiones + sumaComisionesConPolizaPrevia;
                                        ivaComision = ivaComision = sumaIvaComisionesConPolizaPrevia;
                                        // aqui procede a INSERTAR SALDO EN CORTE  
                                        if (foreachCorrecto)
                                        {
                                            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, actulizando saldo:" + Todos_losParametros["@PAGOS"].Valor + "]");
                                          
                                            bool saldo = servicioDatos_.Calculo_NuevoSaldoCorteCuenta(transaccionSQL, ID_Corte, cuenta.Fecha_Corte,
                                                    Todos_losParametros["@DiaFechaCorte"].Valor,
                                                    Todos_losParametros["@Saldo_PromedioDiario"].Valor, Todos_losParametros["@Pago_MinimoANT"].Valor,
                                                    Todos_losParametros["@FactorSaldoInsoluto"].Valor, Todos_losParametros["@FactorLimiteCredito"].Valor,
                                                    Todos_losParametros["@SaldoInsoluto"].Valor,
                                                    Todos_losParametros["@INTORD"].Valor,
                                                    Todos_losParametros["@IVAINTORD"].Valor,
                                                    intMor.ToString(),
                                                    ivaMor.ToString(),
                                                    Todos_losParametros["@limiteCreditoReal"].Valor,
                                                    Todos_losParametros["@Saldo_Vencido"].Valor,
                                                    cuenta.ID_Cuenta,
                                                    Todos_losParametros["@ClaveCorteTipo"].Valor, cuenta.ID_Corte, sumaComisiones, ivaComision,
                                                    Convert.ToDecimal(Todos_losParametros["@InteresOrd"].Valor),
                                                    Convert.ToDecimal(Todos_losParametros["@InteresMor"].Valor),
                                                    Convert.ToInt32(Todos_losParametros["@Dias_delPeriodo"].Valor),
                                                    Convert.ToDecimal(Todos_losParametros["@Compras/Disposiciones"].Valor),
                                                    Convert.ToDecimal(Todos_losParametros["@PAGOS"].Valor),
                                                    Convert.ToDecimal(Todos_losParametros["@LimRetEfectivo"].Valor),
                                                    Todos_losParametros["@FactorSaldoCuenta"].Valor);

                                            if (saldo)
                                            {
                                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, obteniendo datos factura]");
                                                //generando factura
                                                Factura laFactura = LNFacturas.obtenerDatosFactura(Todos_losParametros, sumaComisiones, ivaComision, iva, ivasIntereses, ivaOrd, ivaMor, sinDeuda, cuenta, listaDetallesFactura, listaDetallesXML, _validaciones, folioFactura);
                                                archivos = new List<string>();
                                                LNOperaciones.crearDirectorio(ruta);

                                                fechaCreacion = Hoy.ToString("ddMMyyyyHHmmss");
                                                string mes = Hoy.ToString("MM");
                                                if (mes.Length == 1)
                                                {
                                                    mes = "0" + mes;
                                                }
                                                string anio = Hoy.ToString("yyyy");
                                                if (ruta[ruta.Length - 1] == '\\')
                                                {
                                                    ruta = ruta + "\\";
                                                }
                                                ruta = ruta + cuenta.ClaveCliente + "\\" + cuenta.ClaveCuentahabiente + "\\" + cuenta.ID_Cuenta + "\\" + anio + "\\" + mes + "\\";
                                                if (!sinDeuda)
                                                {
                                                    LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, generando factura]");
                                                    if (LNOperacionesCorte.generarFactura(laFactura, ruta, sinDeuda, cuenta.Tarjeta, archivos, cuenta, _transformador))
                                                    {//una vez que se genera la factura ya no hay forma de hacer rollback
                                                        try
                                                        {

                                                            string codigoQR = urlAccesoServicioSAT + "&id=" + laFactura.UUID + "&re=" + (laFactura.Emisora.RFC.Length > 13 ? laFactura.Emisora.RFC.Substring(0, 13) : laFactura.Emisora.RFC) + "&rr=" + (laFactura.Receptora.RFC.Length > 13 ? laFactura.Receptora.RFC.Substring(0, 13) : laFactura.Receptora.RFC) + "&tt=" +
                                                            (Decimal.Round(laFactura.SubTotal, 2) + Decimal.Round(laFactura.IVA, 2)) + "&fe=" + laFactura.Sello.Substring((laFactura.Sello.Length - 8), 8);
                                                            string rutaImagenQR = ruta + "imagenQR.png";
                                                            LNFacturas.generaCodigoQR(rutaImagenQR, codigoQR);
                                                            laFactura.UrlQrCode = sinDeuda ? "" : rutaImagenQR;
                                                            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Insertando factura]");
                                                           DataTable tablaDatoFactura = _daoFacturas.InsertarFactura(laFactura, ID_Corte, conn2, transaccionSQL, sinDeuda);
                                                            Response errores = new Response();
                                                            if (_validaciones.BusquedaSinErrores(errores, tablaDatoFactura))
                                                            {

                                                            }
                                                            else {
                                                                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, error al insertar factura, corte:" + ID_Corte + " " + errores.CodRespuesta + " " + errores.DescRespuesta+ "]");
                                                             }

                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, error al insertar factura " + ex.Message + " " + ex.StackTrace + "]");
                                                        }
                                                        // throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                                                        commit = true;
                                                    }
                                                }
                                                else
                                                {
                                                    commit = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, error al generar corte de cuenta " + cuenta.ID_Cuenta + "]");
                                         }
                                    }
                                    else
                                    {
                                        LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, error al insertar corte]");
                                     }

                                    if (commit)
                                    {
                                        transaccionSQL.Commit();
                                    }
                                    else
                                    {
                                        transaccionSQL.Rollback();
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
                                        LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, error al realizar rollback:" + exTransaction.Message + " " + exTransaction.StackTrace + "]");
                                    }
                                }
                            } //fin DE LOS EVENTOS AGRUPADORES CUENTAS
                              //como el estado de cuenta abre su conexion, no es necesario meter esos datos dentro de la transaccion
                            if (commit)
                            {
                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, inciando generacion pdf]");
                              
                                string nombreEstado = "";
                                try
                                {
                                    nombreEstado = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFCredito");
                                }
                                catch (Exception ex)
                                {
                                    nombreEstado = "EstadoDeCuenta.pdf";
                                }
                                //generando el estado de cuenta
                                bool enviaCorreo = false;
                                bool estadoDeCuenta = LNOperacionesEdoCuenta.edoCuentaPDF(archivos, userCR, passCR, hostCR, databaseCR, ID_Corte, ruta, _daoCortes, conn2, rutaImagen, cuenta.ClaveCliente, rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, nombreEstado, sinDeuda);
                                fechaCreacion = Hoy.ToString("dd-MM-yyyy");
                                bool envioCorreoCred = Convert.ToBoolean(envioCorreoCredito);
                                if (envioCorreoCred && estadoDeCuenta)//no se enviaran facturas
                                    if (!string.IsNullOrEmpty(cuenta.CorreoCuentahabiente))
                                    {
                                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, enviando correo]");
                                        Correo _correo = new Correo
                                        {
                                            asunto = "Estado de cuenta del" + fechaCreacion,
                                            correoReceptor = cuenta.CorreoCuentahabiente,
                                            mensaje = PNConfig.Get("PROCESAEDOCUENTA", "AsuntoCorreo"),
                                            archivos = archivos,
                                            titulo = PNConfig.Get("PROCESAEDOCUENTA", "AsuntoCorreo"),
                                            correoEmisor = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioCorreo"),
                                            cuerpoMensaje = PNConfig.Get("PROCESAEDOCUENTA", "BodyCorreoElectronico"),
                                            host = PNConfig.Get("PROCESAEDOCUENTA", "HostCorreo"),
                                            password = PNConfig.Get("PROCESAEDOCUENTA", "PasswordCorreo"),
                                            puerto = PNConfig.Get("PROCESAEDOCUENTA", "PuertoCorreo"),
                                            usuario = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioCorreo")
                                        }; ;//"Estado De Cuenta Cacao" };
                                        enviaCorreo = _lnEnvioCorreo.envioCorreo(_correo);
                                    }
                                //insertar registro de correo y generacion en la base de datos
                                // cuenta.ID_Corte
                                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Actualizando estatus envio correo]");
                                _daoCortes.ActualizarDatoCorreoYPDF(estadoDeCuenta, enviaCorreo, ID_Corte, conn2, null, (ruta + nombreEstado));
                            }

                        } //DE LAS CUENTAS A CORTAR HOY

                    }
                    catch (Exception exConection)
                    {
                        LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, error al iniciar coneccion:" + exConection.Message + " " + exConection.StackTrace + "]");
                    }
                    finally
                    {
                        conn2.Close();
                    }
                } //using conection

            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, error al Inicio:" + err.Message + " " + err.StackTrace + "]");
             }
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito, Fin proceso corte]");
            return true;
        }




    }
}
