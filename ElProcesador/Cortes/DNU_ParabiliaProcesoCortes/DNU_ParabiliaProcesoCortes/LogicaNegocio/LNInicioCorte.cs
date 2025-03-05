#define PIEK
//#define SFTP114
#define PIEKMSI
#define ADICIONALES
using CommonProcesador;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
//using DALAutorizador.Entidades;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.LogicaNegocio;

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
using System.Threading;
using System.Globalization;
using DNU_SFTP.Services;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Xml.XPath;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;
using DNU_ParabiliaProcesoCortes.Common.Funciones;
using DNU_ParabiliaProcesoCortes.LogicaNegocio.LNUltranovel;
using DNU_ParabiliaProcesoCortes.LogicaNegocio.LNWirebit;

namespace DNU_ParabiliaProcesoCortes.CapaNegocio
{
    class LNInicioCorte
    {
        const string CLASE = "LNInicioCorte";
        // private static string connArchivosCacao = PNConfig.Get("ALTAEMPLEADOCACAO", "BDWriteProcesadorArchivosCacao");
        //  private static string cadenaConexion = "Data Source=45.32.4.114;initial catalog=Auto_producto;user id=OG;password=Og12369;";
        private string cadenaConexion;// = "Data Source=45.32.4.114;initial catalog=Auto_producto;user id=OG;password=Og12369;";
        static XslCompiledTransform _transformador = new XslCompiledTransform();
        static string rutas = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        const string FORMATO_CLIP = "FORMATOCLIP";
        const string FORMATO_GENERAL = "FORMATOGENERAL";
        //
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "PROCESAEDOCUENTA";
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
        //static LNInicioCorte()
        //{
        //    string ArchXSLT = Path.GetFullPath(Path.Combine(rutas, @"..\..\..\"));
        //    ArchXSLT = Path.GetFullPath(Path.Combine(ArchXSLT, @"DNU_ParabiliaProcesoCortes\Certificados\"));
        //    ArchXSLT = ArchXSLT + "CadenaOriginal_3_3.xslt";
        //    _transformador.Load(ArchXSLT);

        //}

        public LNInicioCorte()
        {
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
                Logueo.Error("[GeneraReporte] [Error al generar el corte] [Mensaje: " + es.Message + " TRACE: " + es.StackTrace + "]" + ArchXSLT);
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
            //
            try
            {
                string ArchXSLT = "";
                //forma 1
                //ConfiguracionContexto.InicializarContexto();
                //ArchXSLT = PNConfig.Get("PROCESAEDOCUENTA", "ArchivoXSLT");

                ////  ArchXSLT = Path.GetFullPath(Path.Combine(ArchXSLT, @"DNU_ParabiliaProcesoCortes\Certificados\"));
                ////  ArchXSLT = ArchXSLT + "CadenaOriginal_3_3.xslt";
                //_transformador = new XslCompiledTransform();
                //_transformador.Load(ArchXSLT);

                //nueva forma
                ConfiguracionContexto.InicializarContexto();
                ArchXSLT = PNConfig.Get("PROCESAEDOCUENTA", "ArchivoXSLT");
                //StreamReader reader = new StreamReader(ArchXSLT);
                //XPathDocument myXPathDoc = new XPathDocument(reader);

                //Cargando el XSLT
                _transformador = new XslCompiledTransform();
                _transformador.Load(ArchXSLT);

                //StringWriter str = new StringWriter();
                //XmlTextWriter myWriter = new XmlTextWriter(str);

                ////Aplicando transformacion
                //myXslTrans.Transform(myXPathDoc, null, myWriter);
            }
            catch (Exception ex)
            {
                string error = "";
            }
            rutaArchivosExterna = rutaArchivos;
        }
        RespuestaSolicitud respuestaSolicitud;
        string id;
        bool envioCorreo;
        string rutaArchivosExterna;
        bool esDebito;

        //[Transaction]
        public bool inicio(string fecha = null)
        {
            //  ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("inicio");
            const string METODO = "inicio";
            // LNFacturas.generaCodigoQR("C:\\ArchivosCortesCacao\\Facturas\\imagenQR.png", "https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?id=36f2e48b-eb78-4422-8520-791d3c97cafe&re=cnf120614443&rr=sesj851228b13&tt=544.34&fe=zfkzyg==");
            string cultura = CultureInfo.CurrentCulture.Name;
            Logueo.Evento("[GeneraEstadoCuentaCredito] culture " + cultura);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            Logueo.Evento("[GeneraEstadoCuentaCredito] cultureMX " + cultura);
            Logueo.Evento("Iniciando proceso de corte");
            DAOCortes _daoCortes = new DAOCortes();
            LNEnvioCorreo _lnEnvioCorreo = new LNEnvioCorreo();
            LNValidacionesCampos _validaciones = new LNValidacionesCampos();
            LNOperaciones lnOperaciones = new LNOperaciones();
            DAOFacturas _daoFacturas = new DAOFacturas();
            bool isTemplateGeneral = false;
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
            {//qr en blanco
             //   LNFacturas.generaCodigoQR(@"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\EDCmanual\", "sinDatos");


                //pdfGeneradoCorrectamente = LNOperacionesCorte.GeneracionTXTSinCorte(id, cadenaConexion, fecha, ruta, rutaImagen, envioCorreo, respuestaSolicitud, _lnEnvioCorreo, _daoCortes, nombrePdfCredito, nombrePdfPrepago, esDebito, _daoFacturas);

                pdfGeneradoCorrectamente = LNOperacionesCorte.GeneracionPDFSinCorteSinTimbrar(id, cadenaConexion, fecha, ruta, rutaImagen, envioCorreo, respuestaSolicitud, _lnEnvioCorreo, _daoCortes, nombrePdfCredito, nombrePdfPrepago, esDebito, _daoFacturas);
                //timbrada
                //pdfGeneradoCorrectamente = LNOperacionesCorte.GeneracionPDFTimbreSinCorteV2(id, cadenaConexion, fecha, ruta, rutaImagen, envioCorreo, respuestaSolicitud, _lnEnvioCorreo, _daoCortes, nombrePdfCredito, nombrePdfPrepago, esDebito, _daoFacturas, _transformador);

                // pdfGeneradoCorrectamente = LNOperacionesCorte.generarFacturaPrueba(new Factura(), @"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\EDCmanual\", false, "1223444555", new List<String>(), new Cuentas(), _transformador, id);
                //trafa

            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraReporte] [Error al generar el PDF o envio de correo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                ApmNoticeWrapper.NoticeException(ex);
            }
            if (!string.IsNullOrEmpty(id))
            {
                Logueo.Evento("Fin proceso corte");
                return pdfGeneradoCorrectamente;
            }

            //Procesando cuentas
            //lo de arriba solo es para procesar el pdf y el envio del correo, lo que sigue ya es el proceso de generacion
            // LNBonificacion _lnBonificacion = new LNBonificacion();
            string urlAccesoServicioSAT = PNConfig.Get("PROCESAEDOCUENTA", "URLAccesoAlServicioSAT"); //ConfigurationManager.AppSettings["URLAccesoAlServicioSAT"].ToString();
            ruta = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalida");
            String rutaTxt = ruta;
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
            //string envioCorreoCredito = PNConfig.Get("PROCESAEDOCUENTA", "EnvioCorreoCredito");
            string conexionPN = PNConfig.Get("PROCESAEDOCUENTA", "BDReadProcesadorNocturno");
            string templateObtenido = string.Empty;

            bool generarFacturaTimbrada = Convert.ToBoolean(PNConfig.Get("PROCESAEDOCUENTA", "GeneraFacturaTimbrada"));


            //LNFacturas.generaCodigoQR(ruta+"prueba.png", "   ");
            //prueba crystal reports
            string userCR = "";
            string passCR = "";
            string hostCR = "";
            string databaseCR = "";
            Cuentas cuentaPac = new Cuentas();

            try
            {
                respuestaSolicitud = new RespuestaSolicitud();

                try
                {
                    if (Entidades.Constants.VERSION.Equals("1"))
                        return new LNCorteWB(_transformador).inicio();
                }
                catch (Exception ex)
                {
                    Logueo.Evento("[GeneraReporteEstadoCuenta] Continua proceso, sin definición de Configuración \"Versión\"");
                }
                //return new LNCorteUltranovel(_transformador).inicio();
#if !PIEK
                    // validando debito trafalagar
                    LNCorteTrafalgar trafalgar = new LNCorteTrafalgar(_transformador);
                    return trafalgar.inicio();


#endif
                //cadenaConexion = cadenaConexion.Replace("10.0.0.5", "20.225.175.141");
                //facturas internas
                using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                {
                    conn2.Open();
                    try
                    {
                        //OBTIENE EL SET DE CUENTAS A CORTAR HOY
                        servicioDatos servicioDatos_ = new servicioDatos(conn2, cadenaConexion);
                        Logueo.Evento("[GeneraReporteEstadoCuenta] Procesando corte");
                        List<DetalleFactura> listaDetallesFactura = new List<DetalleFactura>();
                        List<DetalleFactura> listaDetallesXML = new List<DetalleFactura>();
                        List<DetalleFactura> listaDetallesFacturaExtra = new List<DetalleFactura>();
                        int incrementoFolio = 0;
                        List<string> listaRutasEdoCuenta = new List<string>();
                        List<ParametrosSFTP> listaParametrosSFTP = new List<ParametrosSFTP>();
                        string rutaSFTPKey = "";
                        string rutaSFTPCer = "";
                        string rutaSFTPLogoCAT = "";
                        string rutaSFTPLogo = "";
                        string rutaSFTPLogoCondusef = "";
                        bool imagenLogo = false;
                        bool imagenLogoCAT = false;
                        bool imagenLogoCondusef = false;
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
                                Logueo.Evento("[GeneraReporteEstadoCuenta] Proceso prepago");
                                Logueo.Evento("[GeneraReporteEstadoCuenta] Procesesando corte: " + cuenta.ID_Corte);
                                string nombreEstado = "";
                                try
                                {
                                    nombreEstado = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFDebito");
                                }
                                catch (Exception ex)
                                {
                                    nombreEstado = "EstadoDeMovimientos.pdf";
                                    ApmNoticeWrapper.NoticeException(ex);
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
                                        Logueo.Evento("[GeneraReporteEstadoCuenta] Nuevo corte generado");
                                        idNuevoCorte = tablaNuevoCorte.Rows[0][1].ToString();

                                        rutaImagen = rutaImagen + cuenta.ClaveCliente;
                                        LNOperaciones.crearDirectorio(rutaImagen);
                                        LNOperaciones.crearDirectorio(ruta);
                                        Logueo.Evento("[GeneraReporteEstadoCuenta] Generando PDF ");
                                        //DataTable dt = new DataTable("TablaCliente");//
                                        DataSet ds = _daoCortes.ObtenerDatosPagosDebito(idNuevoCorte, cadenaConexion);
                                        ds = _daoCortes.ObtenerDatosCHDebito(idNuevoCorte, cadenaConexion, null, ds);
                                        Logueo.Evento("[GeneraReporteEstadoCuenta] Datos obtenidos ");
                                        //  DataSetOperacionesEdoCuenta dataSet = new DataSetOperacionesEdoCuenta();
                                        //string cadena2 = "Data Source=10.24.102.200;Initial Catalog=auto_cacao;User ID=cacao_dev_ws;Password=L7za]4X!;Application Name=DNU.AutorizadorParabiliaEvertec;max pool size=300;Pooling=True;Connection Timeout=60";
                                        templateObtenido = obtenerTemplateReporte(cadenaConexion, conexionPN, cuenta.ClaveCliente);

                                        switch (templateObtenido)
                                        {

                                            case FORMATO_CLIP:
                                                #region  CLIP

                                                DNU_ParabiliaProcesoCortes.ReporteDebito.clip.EdoCuentaTipDebito estadoDeCuenta = new DNU_ParabiliaProcesoCortes.ReporteDebito.clip.EdoCuentaTipDebito();
                                                estadoDeCuenta.SetDataSource(ds);

                                                Logueo.Evento("[GeneraReporteEstadoCuenta] generando pdf en carpeta ");
                                                estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + nombreEstado);
                                                estadoDeCuenta.Close();
                                                estadoDeCuenta.Dispose();
                                                pdfGeneradoCorrectamente = true;
                                                Logueo.Evento(string.Format("{0}.{1} Reporte generado : CLIP", CLASE, METODO));

                                                #endregion
                                                break;
                                            case FORMATO_GENERAL:
                                                #region FORMATO_GENERAL
                                                DNU_ParabiliaProcesoCortes.ReporteDebito.reporteCacao.EdoCuentaCacao estadoCuentaNuevo = new DNU_ParabiliaProcesoCortes.ReporteDebito.reporteCacao.EdoCuentaCacao();
                                                estadoCuentaNuevo.SetDataSource(ds);
                                                Logueo.Evento("[GeneraReporteEstadoCuenta] generando pdf en carpeta ");
                                                estadoCuentaNuevo.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + nombreEstado);
                                                estadoCuentaNuevo.Close();
                                                estadoCuentaNuevo.Dispose();
                                                pdfGeneradoCorrectamente = true;
                                                Logueo.Evento(string.Format("{0}.{1} Reporte generado : FORMATOGENERAL", CLASE, METODO));
                                                #endregion
                                                break;
                                            default:
                                                pdfGeneradoCorrectamente = false;
                                                Logueo.Evento(string.Format("{0}.{1} No existe una template para el reporte  Cliente : {2}", CLASE, METODO, cuenta.ClaveCliente));
                                                break;
                                        }
                                    }
                                    if (pdfGeneradoCorrectamente)
                                    {
                                        envioCorreo = false;
                                        Logueo.Evento("[GeneraReporteEstadoCuenta] PDF generado ");
                                        ruta = ruta + nombreEstado;//"ResumenDeMovimientos.pdf";
                                        if (File.Exists(ruta))
                                        {
                                            List<String> ArchivosCorreo = new List<String>();
                                            ArchivosCorreo.Add(ruta);
                                            if (!string.IsNullOrEmpty(cuenta.CorreoCuentahabiente))
                                            {
                                                Logueo.Evento("[GeneraReporteEstadoCuenta] Enviando correo");
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
                                                envioCorreo = _lnEnvioCorreo.envioCorreo(_correo, cuenta, respuestaSolicitud);
                                                if (envioCorreo)
                                                {
                                                    Logueo.Evento("[GeneraReporteEstadoCuenta] Correo Enviado");
                                                    respuestaSolicitud.codigoRespuesta = "0000";
                                                    respuestaSolicitud.descripcionRespuesta = "Correcto";
                                                }
                                                else
                                                {
                                                    respuestaSolicitud.codigoRespuesta = "0003";
                                                    respuestaSolicitud.descripcionRespuesta = "Error al generar Estado de cuenta";
                                                    Logueo.Error("[GeneraReporteEstadoCuenta] [Error al generar el PDF o envio de correo] [Mensaje:No se envío el correo]");

                                                }
                                            }
                                            else
                                            {
                                                Logueo.Error("[GeneraReporteEstadoCuenta] [Error al generar el PDF o envio de correo] [Mensaje:El cuentahabiente no cuenta con correo]");

                                                respuestaSolicitud = new RespuestaSolicitud { codigoRespuesta = "0002", descripcionRespuesta = "El cuentahabiente no cuenta con correo" };
                                            }
                                            //insertar registro de correo y generacion en la base de datos
                                            //acrtualizando estatus


                                        }
                                        else
                                        {
                                            respuestaSolicitud.codigoRespuesta = "0003";
                                            respuestaSolicitud.descripcionRespuesta = "No existe el Estado de cuenta";
                                            Logueo.Error("[GeneraReporteEstadoCuenta] [Error al generar el PDF o envio de correo] [Mensaje:No existe el Estado de cuenta]");

                                        }
                                    }
                                    else
                                    {
                                        respuestaSolicitud.codigoRespuesta = "0003";
                                        respuestaSolicitud.descripcionRespuesta = "Error al generar Estado de cuenta";
                                        Logueo.Error("[GeneraReporteEstadoCuenta] [Error al generar el PDF o envio de correo] [Mensaje:No se generó el pdf]");

                                    }
                                    // if (envioCorreo || pdfGeneradoCorrectamente)
                                    // {
                                    Logueo.Evento("[GeneraReporteEstadoCuenta] Actualizando registro envio pdf y correo:" + idNuevoCorte + "," + pdfGeneradoCorrectamente + "," + envioCorreo);
                                    _daoCortes.ActualizarDatoCorreoYPDF(pdfGeneradoCorrectamente, envioCorreo, Convert.ToInt64(idNuevoCorte), conn2);
                                    Logueo.Evento("[GeneraReporteEstadoCuenta] registro envio pdf y correo actualizado");
                                    // }


                                }
                                catch (Exception exDebito)
                                {
                                    Logueo.Error("[GeneraReporteEstadoCuenta] Error al generar el estado de cuenta:" + exDebito.Message + " " + exDebito.StackTrace);
                                    ApmNoticeWrapper.NoticeException(exDebito);
                                }
                                Logueo.Evento("Fin proceso corte debito");
                                continue;
                            }

                            //credito
                            Logueo.Evento("[GeneraEstadoCuentaCredito] Iniciando Proceso Credito");
                            Logueo.Evento("[GeneraEstadoCuentaCredito] Cuenta: " + cuenta.ID_Cuenta);
                            //pruebas culture
                            //try
                            //{
                            //    string cultura = CultureInfo.CurrentCulture.Name;
                            //    DateTime pruebaHoy = DateTime.Now;
                            //    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");


                            //    DateTime date= DateTime.ParseExact(pruebaHoy.ToString(), "d/M/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);
                            //    DateTime date1;// = DateTime.ParseExact(pruebaHoy.ToString(), "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);////no valido con culture eu
                            //    cultura = CultureInfo.CurrentCulture.Name;
                            //    Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
                            //    cultura = CultureInfo.CurrentCulture.Name;
                            //    string fecha1a= pruebaHoy.ToString("dd/MM/yyyy HH:mm:ss");
                            //    string fecha2a = pruebaHoy.ToString("dd/MM/yyyy hh:mm:ss tt");

                            //    // date = DateTime.ParseExact(pruebaHoy.ToString(), "d/M/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);//no valido con culture mx
                            //    //     DateTime conversion = Convert.ToDateTime(fecha.Replace("p.m.", "PM").Replace("a.m.", "AM"));

                            //    date1 = DateTime.ParseExact(fecha1a, "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture);
                            //}
                            //catch (Exception ex) {
                            //    string g = "";
                            //}


                            if (string.IsNullOrEmpty(cuenta.RFCCliente))
                            {

                                Logueo.Error("[GeneraEstadoCuentaCredito] el cliente no cuenta con RFC, cuenta:" + cuenta.ID_Cuenta);
                                continue;
                            }
                            incrementoFolio = incrementoFolio + 1;
                            string inicioFolio = cuenta.ClaveCliente.Replace("_", "").PadRight(8, '0').Substring(0, 8);
                            DateTime fechaFolio = DateTime.Now;//DateTime.Today;
                            string fecha_actual = fechaFolio.ToString("yyyyMMddhhmmss");//5491XXXXXXXX6877
                                                                                        //   String folioFactura = inicioFolio + fecha_actual + (incrementoFolio.ToString().PadLeft(3, '0').Substring(0, 3));
                            String folioFactura = /*inicioFolio +*/ fecha_actual + (cuenta.ID_Corte.ToString().PadLeft(8, '0').Substring(0, 8));

                            //string folio = 
                            Logueo.Evento("[GeneraEstadoCuentaCredito] Generacion Folio");
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
                            Logueo.Evento("[GeneraEstadoCuentaCredito] Obteniendo Parametros, cuenta " + cuenta.ID_Cuenta.ToString());
                            Set_de_Parametros_Cuenta_MultiAsignacion = servicioDatos_.ObtieneParametros_Cuenta(cuenta.ID_Cuenta, cuenta.ID_Corte, cuenta.Tarjeta);
                            if (Set_de_Parametros_Cuenta_MultiAsignacion.Count == 1)
                            {
                                Logueo.Error("[GeneraEstadoCuentaCredito] Error al obtener los parametros:" + Set_de_Parametros_Cuenta_MultiAsignacion["@error"].Valor);
                                continue;
                            }
                            foreach (string parametro in Set_de_Parametros_Cuenta_MultiAsignacion.Keys)
                            {
                                Todos_losParametros.Add(parametro, Set_de_Parametros_Cuenta_MultiAsignacion[parametro]);
                            }
                            bool generaEstadoDeCuenta = _validaciones.validaParametroDiccionario(Todos_losParametros, "@GeneraFacturaTimbrada") ? Convert.ToBoolean(Todos_losParametros["@GeneraFacturaTimbrada"].Valor) : false;
                            rutaTxt = _validaciones.validaParametroDiccionario(Todos_losParametros, "@RutaGenEdC") ? Todos_losParametros["@RutaGenEdC"].Valor : ruta;
                            Logueo.Evento("[GeneraEstadoCuentaCredito] ruta txt" + rutaTxt);
                            //obteniendo datos sftp ruta
                            string rutaSFTP = _validaciones.validaParametroDiccionario(Todos_losParametros, "@RutaGenEdC") ? Todos_losParametros["@RutaGenEdC"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "CarpetaInicialSFTP"); ;
                            Logueo.Evento("[GeneraEstadoCuentaCredito] ruta sftp" + rutaSFTP);
                            //valida si es sftp para guardar sus datos en una tabla temporal y no tarde el proceso
                            //en esta parte se hara la validacion en caso de ser varios clientes, en este caso es uno y tiene sftp, cuando haya mas se tendra que implementar
                            //if sftp
                            //laFactura.RutaCerSAT = _validaciones.validaParametroDiccionario(Todos_losParametros, "@RutaCerSAT") ? Todos_losParametros["@RutaCerSAT"].Valor : ""; //PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
                            //laFactura.RutaKeySAT = _validaciones.validaParametroDiccionario(Todos_losParametros, "@RutaKeySAT") ? Todos_losParametros["@RutaKeySAT"].Valor : "";//PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
#if PIEK
                            bool generaCerTemporal = false;//este valor es para que no guarde el certificado cada que procese la cuenta, ya no se bajara el scertificado de un cliente si ya se bajo anteriormnte
#else
                                bool generaCerTemporal = false;//este valor es para que no guarde el certificado cada que procese la cuenta, ya no se bajara el scertificado de un cliente si ya se bajo anteriormnte
#endif
                            foreach (ParametrosSFTP cliente in listaParametrosSFTP)
                            {
                                if (cuenta.ClaveCliente == cliente.cliente)
                                {
                                    generaCerTemporal = false;
                                    rutaSFTPCer = cliente.rutaCert;
                                    rutaSFTPKey = cliente.rutaKey;
                                }
                            }
                            if (generaCerTemporal)
                            {
                                string nuevaRutaCer = ruta + Todos_losParametros["@RutaCerSAT"].Valor;
                                nuevaRutaCer = nuevaRutaCer.Replace(@"\", "/");
                                string nuevaRutaKey = ruta + Todos_losParametros["@RutaKeySAT"].Valor;
                                nuevaRutaKey = nuevaRutaKey.Replace(@"\", "/");
                                if (_validaciones.validaParametroDiccionario(Todos_losParametros, "@RutaCerSAT"))
                                {
                                    rutaSFTPCer = LNOperacionesCorte.generarArchivoTemporalSFTP(Todos_losParametros["@RutaCerSAT"].Valor, nuevaRutaCer) ? nuevaRutaCer : "";
                                }
                                if (_validaciones.validaParametroDiccionario(Todos_losParametros, "@RutaKeySAT"))
                                {
                                    rutaSFTPKey = LNOperacionesCorte.generarArchivoTemporalSFTP(Todos_losParametros["@RutaKeySAT"].Valor, nuevaRutaKey) ? nuevaRutaKey : "";

                                }
                            }

                            //recorriendo el dictionary                                             
                            bool saldoInsolutoPagado = Todos_losParametros["@pagoMinimoSinIntereses"].Valor == "1" ? true : false;

                            //obteniendo las imagenes del estado de cuenta
                            string rutaImagenCAT = _validaciones.validaParametroDiccionario(Todos_losParametros, "@ImagenCat") ? Todos_losParametros["@ImagenCat"].Valor : "";
                            string rutaImagenUNE = _validaciones.validaParametroDiccionario(Todos_losParametros, "@ImagenDatosCondusef") ? Todos_losParametros["@ImagenDatosCondusef"].Valor : "";
                            string rutaImagenLogo = _validaciones.validaParametroDiccionario(Todos_losParametros, "@ImagenLogoCredito") ? Todos_losParametros["@ImagenLogoCredito"].Valor : "";

                            string rutaRaiz = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalida");
                            rutaRaiz = rutaRaiz.Replace("Facturas\\", "");

                            rutaImagenCAT = rutaRaiz + rutaImagenCAT;
                            rutaImagenUNE = rutaRaiz + rutaImagenUNE;
                            rutaImagenLogo = rutaRaiz + rutaImagenLogo;
                            //crear directorios
                            FileInfo archivo = new FileInfo(rutaImagenCAT);

                            if (!Directory.Exists(archivo.DirectoryName))
                            {
                                Directory.CreateDirectory(archivo.DirectoryName);
                            }

                            archivo = new FileInfo(rutaImagenUNE);
                            if (!Directory.Exists(archivo.DirectoryName))
                            {
                                Directory.CreateDirectory(archivo.DirectoryName);
                            }


                            archivo = new FileInfo(rutaImagenLogo);
                            if (!Directory.Exists(archivo.DirectoryName))
                            {
                                Directory.CreateDirectory(archivo.DirectoryName);
                            }

                            // Path.GetDirectoryName(rutaImagenLogo);


                            //guardando las imagenes tempora imagnes temporales
                            bool segSFTPhandler = false;
#if !SFTP114
                            segSFTPhandler = true;
#else
                            segSFTPhandler = false;
#endif
                            if (!imagenLogo)
                            {
                                string nuevaRutaimagenLogo = ruta + Todos_losParametros["@ImagenLogoCredito"].Valor;
                            }
                            if (!imagenLogoCAT)
                            {
                                string nuevaRutaimagenLogoCAT = ruta + Todos_losParametros["@ImagenCat"].Valor;
                            }
                            if (!imagenLogoCondusef)
                            {
                                string nuevaRutaimagenLogoCondusef = ruta + Todos_losParametros["@ImagenDatosCondusef"].Valor;
                            }

                            //pruebas
                            //rutaImagenCAT = @"C:\ArchivosCortesPIEK\LogosClientes\CAT_JYU.jpg";
                            // rutaImagenUNE = @"C:\ArchivosCortesPIEK\LogosClientes\UNE_JYU.jpg";
                            // rutaImagenLogo = @"C:\ArchivosCortesPIEK\LogosClientes\logo_JYU.jpg";

                            //fin pruebas





                            // POR CADA CUENTA OBTIENE SUS PARAMETROS DEL CORTE ANTERIOR
                            Logueo.Evento("[GeneraEstadoCuentaCredito] Obtener parametros corte anterior");
                            Set_de_Parametros_Cuenta_Corte_Anterior = servicioDatos_.ObtieneParametros_Cuenta_CorteAnterior(cuenta.ID_Cuenta, Todos_losParametros["@DiaFechaCorte"].Valor, cuenta.Fecha_Corte, cuenta.ID_Corte);

                            //envio correo
                            string envioCorreoCredito = _validaciones.validaParametroDiccionario(Todos_losParametros, "@EnvioCorreoEdoCta") ? Todos_losParametros["@EnvioCorreoEdoCta"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "EnvioCorreoCredito");


                            foreach (string parametro in Set_de_Parametros_Cuenta_Corte_Anterior.Keys)
                            {
                                Todos_losParametros.Add(parametro, Set_de_Parametros_Cuenta_Corte_Anterior[parametro]);


                            }


                            Int64 ID_Corte = 0;
                            Logueo.Evento("[GeneraEstadoCuentaCredito] Utileria");
                            DAOUtilerias laUtil = new DAOUtilerias(conn2);
                            //inicia transacciones para comit
                            decimal sumaComisionesConPolizaPrevia = 0;
                            decimal sumaIvaComisionesConPolizaPrevia = 0;
                            bool commit = false;
                            decimal sumaComisiones = 0;
                            decimal iva = 0;
                            decimal ivaComision = 0;
                            decimal ivasIntereses = 0;
                            decimal SumaIntOrd = 0;
                            decimal SumaIvaIntOrd = 0;//tiene que ir aqui para llegar hasta donde se genera el pdf
                            decimal sumaDiferimiento = 0;
                            string fechaCreacion = "";
                            List<string> archivos = new List<string>();
                            DateTime Hoy = cuenta.Fecha_Corte;
                            bool sinDeuda = false;
                            // DataTable tablaDatoAdicionales = new DataTable();
                            List<CuentaAdicional> cuentasAdicionales = new List<CuentaAdicional>();
                            using (SqlTransaction transaccionSQL = conn2.BeginTransaction())//System.Data.IsolationLevel.ReadCommitted
                            {
                                try
                                {
                                    Logueo.Evento("[GeneraEstadoCuentaCredito] Insertando Corte");
                                    //INSERTA EL CORTE.
                                    ID_Corte = laUtil.InsertaCorteCuenta(cuenta.ClaveCorteTipo, cuenta.Fecha_Corte, cuenta.ID_Cuenta, Todos_losParametros["@Fecha_Final"].Valor,
                                                         Todos_losParametros["@DiaFechaCorte"].Valor, Todos_losParametros["@Fecha_Inicial"].Valor, Todos_losParametros["@fechaLimiteDePago"].Valor, cuenta.ID_Corte, transaccionSQL, Todos_losParametros["@numeroMes"].Valor);
                                    //POR CADA CUENTA OBTIENE LOS EVENTOS AGRUPADORES INVOLUCRADOS EN EL CORTE A PROCESAR
                                    if (ID_Corte > 1)
                                    {
                                        Logueo.Evento("[GeneraEstadoCuentaCredito] Obteniendo eventos agrupadores");
                                        List<Evento> eventAgrupadores_ = servicioDatos_.Obtiene_EventosAgrupadores(cuenta.ID_Cuenta, cuenta.ClaveCorteTipo, cuenta.ID_Corte, Todos_losParametros["@numeroMes"].Valor);
                                        bool foreachCorrecto = true;//validara las ejecuciones correctas de todos los eventos
                                        decimal saldoVencido = 0;
                                        bool interesMoratorio = false;
                                        if (eventAgrupadores_.Count == 0)
                                        {
                                            Logueo.Error("[GeneraEstadoCuentaCredito] No hay eventos agrupados:" + Set_de_Parametros_Cuenta_MultiAsignacion["@error"].Valor);
                                            foreachCorrecto = false;
                                            //    continue;
                                        }
                                        //aqui van todos los datos del contrato pues siempre seran los mismos para una tarjeta


                                        //fin
                                        decimal ivaOrd = 0;// = Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                        decimal ivaMor = 0;// = Convert.ToDecimal(Todos_losParametros["@IVAINTMOR"].Valor);
                                        decimal intOrd = 0;// = Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                        decimal intMor = 0;


                                        Logueo.Evento("[GeneraEstadoCuentaCredito] Procesando eventos agrupadores");
                                        foreach (Evento unEvento in eventAgrupadores_)
                                        {
                                            Logueo.Evento("[GeneraEstadoCuentaCredito] Evento:" + unEvento.ClaveEventoAgrupador);
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
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] ejecutando procedimiento evento");

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
                                                    Logueo.Error("[GeneraEstadoCuentaCredito] Error al ejecutar el SP del Evento Agrupador:" + err.Message);
                                                    ApmNoticeWrapper.NoticeException(err);
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
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] Obteniendo polizas ya creadas");
                                                //if ((!string.IsNullOrEmpty(unEvento.eventoPrincipal)))
                                                //{
                                                //    foreach (Evento evento in eventAgrupadores_)
                                                //    {
                                                //        if (evento.id_corteEventoAgrupador == unEvento.eventoPrincipal)
                                                //        {
                                                //            // if (string.IsNullOrEmpty(Todos_losParametros["@" + evento.ClaveEvento].Valor))
                                                //            //{

                                                //            DataTable tablaDatoFactura = _daoFacturas.ObtenerDatosMovimientosEvento(evento.ClaveEventoAgrupador, unEvento.ClaveEventoAgrupador, cuenta.ID_Cuenta.ToString(), ID_Corte.ToString(), conn2, transaccionSQL);
                                                //            Response errores = new Response();
                                                //            if (_validaciones.BusquedaSinErrores(errores, tablaDatoFactura))
                                                //            {
                                                //                if (Convert.ToDecimal(tablaDatoFactura.Rows[0][2]) > 0)
                                                //                {
                                                //                    string eventoIva = tablaDatoFactura.Rows[0][2].ToString();
                                                //                    string eventoPrimario = tablaDatoFactura.Rows[0][1].ToString();
                                                //                    if (evento.parametroMonto != "@INTORD" && evento.parametroMonto != "@INTMOR")
                                                //                    {
                                                //                        //es comision //aqui yan no deberia de entrar, depsuies de priuebas se borrara
                                                //                        //sumaComisionesConPolizaPrevia = sumaComisionesConPolizaPrevia + Convert.ToDecimal(eventoPrimario);
                                                //                        //sumaIvaComisionesConPolizaPrevia = sumaIvaComisionesConPolizaPrevia + Convert.ToDecimal(eventoIva);


                                                //                        //sumaComisiones = sumaComisiones + Convert.ToDecimal(eventoPrimario);
                                                //                        //ivaComision = ivaComision + Convert.ToDecimal(eventoIva);
                                                //                    }
                                                //                    DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = unEvento.unidadSAT, ClaveProdServ = unEvento.ClaveProdServSAT, ClaveUnidad = unEvento.ClaveUnidadSAT, NombreProducto = unEvento.descripcionEventoEdoCuenta, impImporte = tablaDatoFactura.Rows[0][2].ToString(), Total = Convert.ToDecimal(tablaDatoFactura.Rows[0][1].ToString()), PrecioUnitario = Convert.ToDecimal(tablaDatoFactura.Rows[0][1].ToString()), impBase = tablaDatoFactura.Rows[0][1].ToString(), impImpuesto = unEvento.impImpuestoSAT, impTipoFactor = unEvento.impTipoFactorSAT, impTasaOCuota = Todos_losParametros["@IVA"].Valor };
                                                //                    listaDetallesFactura.Add(detalle);
                                                //                }
                                                //            }

                                                //            //}
                                                //        }
                                                //    }
                                                //}
                                                //else
                                                if (unEvento.ClaveEventoAgrupador == "COMISIONES")
                                                {
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]Obteniendo comisiones:" + sumaComisionesConPolizaPrevia + " iva:" + sumaIvaComisionesConPolizaPrevia);

                                                    DataTable tablaDatoFactura = _daoFacturas.ObtenerDatosMovimientosEvento(unEvento.ClaveEventoAgrupador, unEvento.ClaveEventoAgrupador, cuenta.ID_Cuenta.ToString(), ID_Corte.ToString(), conn2, transaccionSQL);
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]Comisiones obtenidas:" + sumaComisionesConPolizaPrevia + " iva:" + sumaIvaComisionesConPolizaPrevia);

                                                    Response errores = new Response();
                                                    if (_validaciones.BusquedaSinErrores(errores, tablaDatoFactura))
                                                    {
                                                        if (Convert.ToDecimal(tablaDatoFactura.Rows[0][2]) > 0)
                                                        {
                                                            string eventoIva = tablaDatoFactura.Rows[0][2].ToString();
                                                            string eventoPrimario = tablaDatoFactura.Rows[0][1].ToString();
                                                            if (unEvento.parametroMonto != "@INTORD" && unEvento.parametroMonto != "@INTMOR")
                                                            {
                                                                //es comision
                                                                sumaComisionesConPolizaPrevia = sumaComisionesConPolizaPrevia + Convert.ToDecimal(eventoPrimario);
                                                                sumaIvaComisionesConPolizaPrevia = sumaIvaComisionesConPolizaPrevia + Convert.ToDecimal(eventoIva);
                                                                //sumaComisiones = sumaComisiones + Convert.ToDecimal(eventoPrimario);
                                                                //ivaComision = ivaComision + Convert.ToDecimal(eventoIva);
                                                                Logueo.Evento("[GeneraEstadoCuentaCredito]Obteniendo comision:" + sumaComisionesConPolizaPrevia + " iva:" + sumaIvaComisionesConPolizaPrevia);
                                                            }

                                                            DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = unEvento.unidadSAT, ClaveProdServ = unEvento.ClaveProdServSAT, ClaveUnidad = unEvento.ClaveUnidadSAT, NombreProducto = unEvento.descripcionEventoEdoCuenta, impImporte = tablaDatoFactura.Rows[0][2].ToString(), Total = Convert.ToDecimal(tablaDatoFactura.Rows[0][1].ToString()), PrecioUnitario = Convert.ToDecimal(tablaDatoFactura.Rows[0][1].ToString()), impBase = tablaDatoFactura.Rows[0][1].ToString(), impImpuesto = unEvento.impImpuestoSAT, impTipoFactor = unEvento.impTipoFactorSAT, impTasaOCuota = Todos_losParametros["@IVA"].Valor };
                                                            listaDetallesFactura.Add(detalle);
                                                        }
                                                    }
                                                }
                                                continue;
                                            }

                                            //intereses Parcialidad

#if PIEKMSI
                                            if (unEvento.ClaveEventoAgrupador == "INTORD" || unEvento.ClaveEventoAgrupador == "INTMOR")
                                            {
                                                Logueo.Evento("[GeneraEstadoCuentaCredito]Obteniendo Interes Parcialidad");

                                                DataTable tablaDatoFactura = _daoFacturas.CalculaInteresDiferimiento(unEvento.ClaveEventoAgrupador, unEvento.ClaveEventoAgrupador, cuenta.ID_Cuenta.ToString(), ID_Corte.ToString(), conn2, transaccionSQL);
                                                Logueo.Evento("[GeneraEstadoCuentaCredito]Obteniendo Interes Parcialidad obtenido");

                                                Response errores = new Response();
                                                if (_validaciones.BusquedaSinErrores(errores, tablaDatoFactura))
                                                {
                                                    if (Convert.ToDecimal(tablaDatoFactura.Rows[0][2]) > 0)
                                                    {
                                                        string eventoIva = tablaDatoFactura.Rows[0][2].ToString();
                                                        string eventoPrimario = tablaDatoFactura.Rows[0][1].ToString();
                                                        SumaIntOrd = Convert.ToDecimal(eventoPrimario);
                                                        SumaIvaIntOrd = Convert.ToDecimal(eventoIva);

                                                        //ivaOrd = Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                                        //intOrd = Convert.ToDecimal(Todos_losParametros["@INTORD"].Valor);


                                                        //  DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = unEvento.unidadSAT, ClaveProdServ = unEvento.ClaveProdServSAT, ClaveUnidad = unEvento.ClaveUnidadSAT, NombreProducto = unEvento.descripcionEventoEdoCuenta, impImporte = tablaDatoFactura.Rows[0][2].ToString(), Total = Convert.ToDecimal(tablaDatoFactura.Rows[0][1].ToString()), PrecioUnitario = Convert.ToDecimal(tablaDatoFactura.Rows[0][1].ToString()), impBase = tablaDatoFactura.Rows[0][1].ToString(), impImpuesto = unEvento.impImpuestoSAT, impTipoFactor = unEvento.impTipoFactorSAT, impTasaOCuota = Todos_losParametros["@IVA"].Valor };
                                                        //  listaDetallesFactura.Add(detalle);
                                                        //DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = "Servicios", ClaveProdServ = "84121500", ClaveUnidad = "E48", NombreProducto = "INTERES ORDINARIO", impImporte = SumaIvaIntOrd.ToString(), Total = valorInteres, PrecioUnitario = valorInteres, impBase = valorInteres.ToString(), impImpuesto = "002", impTipoFactor = "Tasa", impTasaOCuota = iva.ToString("###,###,##0.000000") };
                                                        DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = unEvento.unidadSAT, ClaveProdServ = unEvento.ClaveProdServSAT, ClaveUnidad = unEvento.ClaveUnidadSAT, NombreProducto = unEvento.descripcionEventoEdoCuenta, impImporte = SumaIvaIntOrd.ToString(), Total = Convert.ToDecimal(SumaIntOrd), PrecioUnitario = Convert.ToDecimal(0), impBase = "0.0", impImpuesto = unEvento.impImpuestoSAT, impTipoFactor = unEvento.impTipoFactorSAT, impTasaOCuota = Todos_losParametros["@IVA"].Valor };
                                                        listaDetallesFacturaExtra.Add(detalle);
                                                    }
                                                    if (Convert.ToDecimal(tablaDatoFactura.Rows[0]["cargosDiferimiento"]) > 0)
                                                    {
                                                        string montoDiferimiento = tablaDatoFactura.Rows[0]["cargosDiferimiento"].ToString();
                                                        sumaDiferimiento = Convert.ToDecimal(montoDiferimiento);

                                                    }

                                                }
                                            }

#endif


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
                                                        if (unEvento.ClaveEventoAgrupador == "INTORD")
                                                        {
                                                            Decimal saldoPromedio = Convert.ToDecimal(Todos_losParametros["@Saldo_PromedioDiario"].Valor);
                                                            if (saldoPromedio < 0)
                                                            {
                                                                sinDeuda = true;
                                                                continue;
                                                            }

                                                        }

                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] Generando poliza");

                                                // Respuesta unaRespo =   GeneraPoliza.EjecutarEvento(unEvento.ID_Evento, 0, unEvento.ID_CadenaComercial.ToString(), unEvento.ClaveEvento, unEvento.Descripcion, "", (unEvento.Consecutivo), TodosLosParametros, unEvento, ID_Corte, ref conn2, ref transaccionSQL);
                                                //LLAMA AL SCRIPT CONTABLE
                                                DatosOperacionesExecute solicitudDatos = new DatosOperacionesExecute { claveEvento = unEvento.ClaveEventoAgrupador, importe = Todos_losParametros["@Importe"].Valor, tarjeta = cuenta.Tarjeta, tipoMedioAcceso = "TAR" };
                                                DataTable tablaDatoCortes = _daoCortes.obtenerDatosParaExecute(solicitudDatos, conn2, transaccionSQL);
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] Parametros para excecute obtenidos");

                                                Response errores = new Response();
                                                if (!_validaciones.BusquedaSinErrores(errores, tablaDatoCortes))
                                                {
                                                    Logueo.Error("[GeneraEstadoCuentaCredito] Error al obtener los datos para execute: [" + unEvento.ClaveEvento + "], Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                                    foreachCorrecto = false;
                                                    break;
                                                }
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo paraetros encriptados");
                                                tablaDatoCortes = FuncionesTablas.onbtenerValoresTablaEncriptadosAE(tablaDatoCortes);

                                                Bonificacion bonificacion = LNBonificacion.obtenerDatosParaDiccionario(tablaDatoCortes);
                                                bonificacion.Importe = Todos_losParametros["@Importe"].Valor; // solicitudDatos.importe.ToString();
                                                                                                              //bonificacion.Observaciones = retirarTarjetaEmpresa.Observaciones;
                                                                                                              //bonificacion.RefNumerica = Convert.ToInt32("0");
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo paraetros");

                                                Dictionary<String, Parametro> parametrosRetirar = _daoCortes.ObtenerDatosParametros(bonificacion, tablaDatoCortes, conn2, transaccionSQL, cadenaConexion);


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
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] Ejecutando evento");

                                                Respuesta unaRespo = GeneraPoliza.EjecutarEvento(unEvento.ID_AgrupadorEvento, 0, unEvento.ID_CadenaComercial.ToString(), unEvento.ClaveEventoAgrupador, unEvento.Descripcion, "", unEvento.Consecutivo, Todos_losParametros, unEvento, ID_Corte, conn2, transaccionSQL, bonificacion, parametrosRetirar);
                                                if (unaRespo.CodigoRespuesta != 00)
                                                {
                                                    Logueo.Error("[GeneraEstadoCuentaCredito] La poliza no pudo crearse: [" + unEvento.ClaveEvento + "], Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                                    // GeneracionExitosaDePolizas = false;
                                                    //transaccionSQL.Rollback();
                                                    foreachCorrecto = false;
                                                    break;
                                                    //continue;
                                                }
                                                else
                                                {
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] La poliza Creada: [" + unEvento.ClaveEventoAgrupador + "], Descripcion: " + unEvento.Descripcion + ", Cadena comercial:" + unEvento.ID_CadenaComercial + ".");
                                                    //  GeneracionExitosaDePolizas = true;
                                                    //Ligar las polizas a los eventos de corte
                                                    // laUtil.LigarPolizasAEventoDeCorte(unEvento.ID_AgrupadorEvento, ID_Corte, transaccionSQL);
                                                    // transaccionSQL.Commit();
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]Relacionando fecha corte");
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
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]Obteniendo ivas y comisiones");
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
                                                                else if (evento.ClaveEventoAgrupador == "INTORD")
                                                                {
                                                                    ivaOrd = Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                                                    intOrd = Convert.ToDecimal(Todos_losParametros["@INTORD"].Valor);

                                                                }
                                                                string eventoIva = Todos_losParametros["@" + unEvento.ClaveEventoAgrupador].Valor;
                                                                string eventoPrimario = Todos_losParametros["@" + evento.ClaveEventoAgrupador].Valor;
                                                                if (evento.ClaveEventoAgrupador != "INTORD" && evento.ClaveEventoAgrupador != "INTMOR")//(evento.parametroMonto != "@INTORD" && evento.parametroMonto != "@INTMOR")
                                                                {
                                                                    string vacio = "";
                                                                    //es comision ya nos e deberian sumar aqui ya nod ebe entra a este if
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
                                                Logueo.Error("[GeneraEstadoCuentaCredito] GenerarLasPolizasDeCortes() Error se deshacen la polizas para la cadena comercial " + unEvento.ID_CadenaComercial + " :" + err.Message + "," + err.StackTrace);
                                                ApmNoticeWrapper.NoticeException(err);
                                                foreachCorrecto = false;
                                                break;
                                            }

                                        }

#if ADICIONALES
                                        //adicionales
                                        DataTable tablaDatoAdicionales = new DataTable();
                                        if (foreachCorrecto)
                                        {
                                            try
                                            {
                                                tablaDatoAdicionales = _daoFacturas.ObtieneTarjetasAdicionales(cuenta.ID_Cuenta, conn2, transaccionSQL);
                                                foreach (DataRow row in tablaDatoAdicionales.Rows)
                                                {
                                                    string rutaAdicional = ruta;
                                                    string mesAdicional = Hoy.ToString("MM");
                                                    if (mesAdicional.Length == 1)
                                                    {
                                                        mesAdicional = "0" + mesAdicional;
                                                    }
                                                    string anioAdicional = Hoy.ToString("yyyy");
                                                    if (rutaAdicional[rutaAdicional.Length - 1] != '\\')
                                                    {
                                                        rutaAdicional = rutaAdicional + "\\";
                                                    }
                                                    rutaAdicional = rutaAdicional + cuenta.ClaveCliente + "\\" + row["TarjetaAdicional"].ToString().Substring(6) + "\\" + anioAdicional + "\\" + mesAdicional + "\\";

                                                    cuentasAdicionales.Add(new CuentaAdicional
                                                    {
                                                        Tarjeta = row["TarjetaAdicional"].ToString(),
                                                        IDMATarjetaAdicional = row["IDMATarjetaAdicional"].ToString(),
                                                        CorreoCuentahabiente = row["Email"].ToString(),
                                                        NombreORazonSocial = row["NombreORazonSocial"].ToString(),
                                                        RutaEdoCuenta = rutaAdicional,
                                                        ID_ColectivaAdicional = row["ID_Colectiva"].ToString(),
                                                        ID_CuentaCompensacion = row["ID_CuentaCompensacion"].ToString(),
                                                        Archivos = new List<string>()
                                                    });
                                                    DatosOperacionesExecute solicitudDatos = new DatosOperacionesExecute { claveEvento = row["claveEvento"].ToString(), importe = Todos_losParametros["@Importe"].Valor, tarjeta = row["TarjetaAdicional"].ToString(), tipoMedioAcceso = "TAR" };
                                                    DataTable tablaDatoCortes = _daoCortes.obtenerDatosParaExecute(solicitudDatos, conn2, transaccionSQL);
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] Parametros para excecute obtenidos adicional");
                                                    Response errores = new Response();
                                                    if (!_validaciones.BusquedaSinErrores(errores, tablaDatoCortes))
                                                    {
                                                        Logueo.Error("[GeneraEstadoCuentaCredito] Error al obtener los datos adicional para execute : [" + row["claveEvento"].ToString() + "], , Cadena comercial:" + cuenta.id_colectivaCliente + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                                        foreachCorrecto = false;
                                                        break;
                                                    }
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo paraetros adicional encriptados");
                                                    tablaDatoCortes = FuncionesTablas.onbtenerValoresTablaEncriptadosAE(tablaDatoCortes);

                                                    Bonificacion bonificacion = LNBonificacion.obtenerDatosParaDiccionario(tablaDatoCortes);
                                                    // bonificacion.Importe = Todos_losParametros["@Importe"].Valor; // solicitudDatos.importe.ToString();
                                                    //bonificacion.Observaciones = retirarTarjetaEmpresa.Observaciones;
                                                    //bonificacion.RefNumerica = Convert.ToInt32("0");
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo paraetros");

                                                    //obtendremos los parametros de la tarjeta titular
                                                    //bonificacion.Tarjeta = cuenta.Tarjeta;
                                                    Dictionary<String, Parametro> parametrosRetirar = _daoCortes.ObtenerDatosParametros(bonificacion, tablaDatoCortes, conn2, transaccionSQL, cadenaConexion);
                                                    // bonificacion.Importe = Todos_losParametros["@Saldo_Adicional_CCLC"].Valor;
                                                    parametrosRetirar["@Saldo_Adicional_CCLC"] = new Parametro() { Nombre = "@Saldo_Adicional_CCLC", Valor = row["SaldoActualAutorizacion"].ToString(), Descripcion = "Saldo_Adicional_CCLC" };
                                                    parametrosRetirar["@Saldo_Adicional_CDC"] = new Parametro() { Nombre = "@Saldo_Adicional_CDC", Valor = row["SaldoActualCompensacion"].ToString(), Descripcion = "Saldo_Adicional_CDC" };

                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] Ejecutando evento adicional ");

                                                    Respuesta unaRespo = GeneraPoliza.EjecutarEvento(Convert.ToInt32(row["IdEvento"]), 0, cuenta.id_colectivaCliente.ToString(), bonificacion.Tarjeta, row["EventoDescripcion"].ToString(), bonificacion.Observaciones, Convert.ToInt64(row["Consecutivo"]), parametrosRetirar, new Evento(), ID_Corte, conn2, transaccionSQL, bonificacion, parametrosRetirar);
                                                    if (unaRespo.CodigoRespuesta != 00)
                                                    {
                                                        Logueo.Error("[GeneraEstadoCuentaCredito] La poliza no pudo crearse: [" + row["claveEvento"] + "], Descripcion: " + row["eventoDescripcion"] + ", Cadena comercial:" + cuenta.id_CadenaComercial + ".  Se Realiza Rollback de las polizas creadas para dicha Cadena Comercial");
                                                        // GeneracionExitosaDePolizas = false;
                                                        //transaccionSQL.Rollback();
                                                        foreachCorrecto = false;
                                                        break;
                                                        //continue;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Logueo.Error("[GeneraEstadoCuentaCredito] Error al obtener los datos adicional :" + ex.Message);

                                                foreachCorrecto = false;
                                            }
                                        }
#endif
                                        //}

                                        //if (foreachCorrecto)
                                        //{
                                        //    ivaOrd = Convert.ToDecimal(Todos_losParametros["@IVAINTORD"].Valor);
                                        //    intOrd = Convert.ToDecimal(Todos_losParametros["@INTORD"].Valor);
                                        //}

                                        //iva = Convert.ToDecimal(Todos_losParametros["@IVA"].Valor);
                                        //ivaComision = sumaComisiones * iva;
                                        //ivasIntereses = ivaOrd + ivaMor;
                                        intOrd = SumaIntOrd + intOrd;
                                        ivaOrd = SumaIvaIntOrd + ivaOrd;
                                        iva = Convert.ToDecimal(Todos_losParametros["@IVA"].Valor);
                                        ivasIntereses = ivaOrd + ivaMor;
                                        sumaComisiones = sumaComisiones + sumaComisionesConPolizaPrevia;
                                        ivaComision = ivaComision + sumaIvaComisionesConPolizaPrevia;
                                        // aqui procede a INSERTAR SALDO EN CORTE  
                                        if (foreachCorrecto)
                                        {
                                            Logueo.Evento("[GeneraEstadoCuentaCredito] actulizando saldo:" + Todos_losParametros["@PAGOS"].Valor);
#if PIEKMSI
                                            bool saldo = servicioDatos_.Calculo_NuevoSaldoCorteCuenta(transaccionSQL, ID_Corte, cuenta.Fecha_Corte,
                                                    Todos_losParametros["@DiaFechaCorte"].Valor,
                                                    Todos_losParametros["@Saldo_PromedioDiario"].Valor, Todos_losParametros["@Pago_MinimoANT"].Valor,
                                                    Todos_losParametros["@FactorSaldoInsoluto"].Valor, Todos_losParametros["@FactorLimiteCredito"].Valor,
                                                    Todos_losParametros["@SaldoInsoluto"].Valor,
                                                    intOrd.ToString(),// Todos_losParametros["@INTORD"].Valor,
                                                    ivaOrd.ToString(),// Todos_losParametros["@IVAINTORD"].Valor,
                                                    intMor.ToString(),
                                                    ivaMor.ToString(),
                                                    Todos_losParametros["@limiteCreditoReal"].Valor,
                                                    Todos_losParametros["@Saldo_Vencido"].Valor,
                                                    cuenta.ID_Cuenta,
                                                    Todos_losParametros["@ClaveCorteTipo"].Valor, cuenta.ID_Corte, sumaComisiones, ivaComision,
                                                    Convert.ToDecimal(Todos_losParametros["@TasaInteresOrd"].Valor),
                                                    Convert.ToDecimal(Todos_losParametros["@MultIntMor"].Valor),//Convert.ToDecimal(Todos_losParametros["@InteresMor"].Valor),
                                                    Convert.ToInt32(Todos_losParametros["@Dias_delPeriodo"].Valor),
                                                    Convert.ToDecimal(Todos_losParametros["@Compras/Disposiciones"].Valor),
                                                    Convert.ToDecimal(Todos_losParametros["@PAGOS"].Valor),
                                                    Convert.ToDecimal(Todos_losParametros["@LimRetEfectivo"].Valor),
                                                    Todos_losParametros["@FactorSaldoCuenta"].Valor,
                                                    SumaIntOrd.ToString(), SumaIvaIntOrd.ToString(), sumaDiferimiento.ToString());
#else
                                            bool saldo = servicioDatos_.Calculo_NuevoSaldoCorteCuenta(transaccionSQL, ID_Corte, cuenta.Fecha_Corte,
                                                                                               Todos_losParametros["@DiaFechaCorte"].Valor,
                                                                                               Todos_losParametros["@Saldo_PromedioDiario"].Valor, Todos_losParametros["@Pago_MinimoANT"].Valor,
                                                                                               Todos_losParametros["@FactorSaldoInsoluto"].Valor, Todos_losParametros["@FactorLimiteCredito"].Valor,
                                                                                               Todos_losParametros["@SaldoInsoluto"].Valor,
                                                                                               intOrd.ToString(),// Todos_losParametros["@INTORD"].Valor,
                                                                                               ivaOrd.ToString(),// Todos_losParametros["@IVAINTORD"].Valor,
                                                                                               intMor.ToString(),
                                                                                               ivaMor.ToString(),
                                                                                               Todos_losParametros["@limiteCreditoReal"].Valor,
                                                                                               Todos_losParametros["@Saldo_Vencido"].Valor,
                                                                                               cuenta.ID_Cuenta,
                                                                                               Todos_losParametros["@ClaveCorteTipo"].Valor, cuenta.ID_Corte, sumaComisiones, ivaComision,
                                                                                               Convert.ToDecimal(Todos_losParametros["@TasaInteresOrd"].Valor),
                                                                                               Convert.ToDecimal(Todos_losParametros["@MultIntMor"].Valor),//Convert.ToDecimal(Todos_losParametros["@InteresMor"].Valor),
                                                                                               Convert.ToInt32(Todos_losParametros["@Dias_delPeriodo"].Valor),
                                                                                               Convert.ToDecimal(Todos_losParametros["@Compras/Disposiciones"].Valor),
                                                                                               Convert.ToDecimal(Todos_losParametros["@PAGOS"].Valor),
                                                                                               Convert.ToDecimal(Todos_losParametros["@LimRetEfectivo"].Valor),
                                                                                               Todos_losParametros["@FactorSaldoCuenta"].Valor);
#endif
                                            if (saldo)
                                            {
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] obten iendo datos factura");
                                                //generando factura
                                                Factura laFactura = LNFacturas.obtenerDatosFacturaCFDI4(Todos_losParametros, sumaComisiones, ivaComision, iva, ivasIntereses, ivaOrd, ivaMor, sinDeuda, cuenta, listaDetallesFactura, listaDetallesXML, _validaciones, folioFactura, cuenta, listaDetallesFacturaExtra);
                                                //sftp
                                                //if stp
#if PIEK
                                                if (string.IsNullOrEmpty(rutaSFTPCer))
                                                {
                                                    laFactura.RutaCerSAT = PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
                                                    laFactura.RutaKeySAT = PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
                                                    laFactura.PassCerSAT = PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");
                                                }
                                                else
                                                {
                                                    laFactura.RutaCerSAT = rutaSFTPCer;
                                                    laFactura.RutaKeySAT = rutaSFTPKey;
                                                }
#endif
                                                //fin stp
                                                archivos = new List<string>();
                                                LNOperaciones.crearDirectorio(ruta);

                                                fechaCreacion = Hoy.ToString("ddMMyyyyHHmmss");
                                                string mes = Hoy.ToString("MM");
                                                if (mes.Length == 1)
                                                {
                                                    mes = "0" + mes;
                                                }
                                                string anio = Hoy.ToString("yyyy");
                                                if (ruta[ruta.Length - 1] != '\\')
                                                {
                                                    ruta = ruta + "\\";
                                                }
                                                ruta = ruta + cuenta.ClaveCliente + "\\" + cuenta.Tarjeta.Substring(6) + "\\" + anio + "\\" + mes + "\\";
                                                //ruta = ruta + cuenta.ClaveCliente + "\\" + cuenta.ClaveCuentahabiente + "\\" + cuenta.ID_Cuenta + "\\" + anio + "\\" + mes + "\\";
                                                Int64 idFactura = 0;
                                                if (!sinDeuda || sumaComisiones > 0 || SumaIntOrd > 0)
                                                {
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] generando factura");
                                                    //Ahora primero guardare todos los datos y luego los actualizare, este paos es importante para posterior mente generar el pdf
                                                    DataTable tablaDatoFactura = _daoFacturas.InsertarFactura(laFactura, ID_Corte, conn2, transaccionSQL, sinDeuda);
                                                    Response errores = new Response();
                                                    bool facturaGeneradacorrectamente = false;
                                                    if (_validaciones.BusquedaSinErrores(errores, tablaDatoFactura))
                                                    {
                                                        facturaGeneradacorrectamente = true;
                                                        idFactura = Convert.ToInt64(tablaDatoFactura.Rows[0][1].ToString());
                                                    }
                                                    else
                                                    {
                                                        Logueo.Error("[GeneraEstadoCuentaCredito] error al insertar factura, corte:" + ID_Corte + " " + errores.CodRespuesta + " " + errores.DescRespuesta);
                                                    }
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] factura insertada");
                                                    if (facturaGeneradacorrectamente && generaEstadoDeCuenta)//generarFacturaTimbrada)
                                                    {
                                                        Logueo.Evento("[GeneraEstadoCuentaCredito] factura insertada");
                                                        if (LNOperacionesCorte.generarFacturaV4(laFactura, ruta, false, cuenta.Tarjeta, archivos, cuenta, _transformador, "", false))
                                                        {//una vez que se genera la factura ya no hay forma de hacer rollback
                                                            try
                                                            {
                                                                Logueo.Evento("[GeneraEstadoCuentaCredito] datos factura obtenidos");
                                                                string codigoQR = urlAccesoServicioSAT + "?id=" + laFactura.UUID + "&re=" + (laFactura.Emisora.RFC.Length > 13 ? laFactura.Emisora.RFC.Substring(0, 13) : laFactura.Emisora.RFC) + "&rr=" + (laFactura.Receptora.RFC.Length > 13 ? laFactura.Receptora.RFC.Substring(0, 13) : laFactura.Receptora.RFC) + "&tt=" +
                                                                (Decimal.Round(laFactura.SubTotal, 2) + Decimal.Round(laFactura.IVA, 2)) + "&fe=" + laFactura.Sello.Substring((laFactura.Sello.Length - 8), 8);
                                                                string rutaImagenQR = ruta + "imagenQR.png";
                                                                LNFacturas.generaCodigoQR(rutaImagenQR, codigoQR);
                                                                laFactura.UrlQrCode = sinDeuda ? "" : rutaImagenQR;
                                                                Logueo.Evento("[GeneraEstadoCuentaCredito] Insertando factura");
                                                                DataTable tablaDatoFacturaInserta = _daoFacturas.ActualizaFactura(idFactura, laFactura, ID_Corte, conn2, transaccionSQL, sinDeuda);
                                                                errores = new Response();
                                                                if (_validaciones.BusquedaSinErrores(errores, tablaDatoFacturaInserta))
                                                                {

                                                                }
                                                                else
                                                                {
                                                                    Logueo.Error("[GeneraEstadoCuentaCredito] error al insertar factura, corte:" + ID_Corte + " " + errores.CodRespuesta + " " + errores.DescRespuesta);
                                                                }

                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                Logueo.Error("[GeneraEstadoCuentaCredito] error al insertar factura " + ex.Message + " " + ex.StackTrace);
                                                                ApmNoticeWrapper.NoticeException(ex);
                                                            }
                                                            // throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                                                            commit = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (facturaGeneradacorrectamente)
                                                        {//si la factura s egenero correctamente y no esta activa la bandera de timbrar
                                                         //entonces termina el proceso y hace el commit
                                                            commit = true;
                                                            Logueo.Evento("[GeneraEstadoCuentaCredito] factura generada sin timbrar");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //    DataTable tablaDatoFactura = _daoFacturas.InsertarFactura(laFactura, ID_Corte, conn2, transaccionSQL, sinDeuda);
                                                    commit = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Logueo.Error("[GeneraEstadoCuentaCredito] error al generar corte de cuenta " + cuenta.ID_Cuenta);
                                        }
                                    }
                                    else
                                    {
                                        Logueo.Error("[GeneraEstadoCuentaCredito] error al insertar corte");
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
                                        Logueo.Error("[GeneraEstadoCuentaCredito]  Error se deshacen la polizas para la cadena comercial " + ex.Message + "," + ex.StackTrace);
                                        ApmNoticeWrapper.NoticeException(ex);
                                    }
                                    catch (Exception exTransaction)
                                    {
                                        Logueo.Error("[GeneraEstadoCuentaCredito] error al realizar rollback:" + exTransaction.Message + " " + exTransaction.StackTrace);
                                        ApmNoticeWrapper.NoticeException(exTransaction);
                                    }
                                }
                            } //fin DE LOS EVENTOS AGRUPADORES CUENTAS
                              //como el estado de cuenta abre su conexion, no es necesario meter esos datos dentro de la transaccion
                            if (commit)
                            {
                                Logueo.Evento("[GeneraEstadoCuentaCredito] inciando generacion pdf");

                                string nombreEstado = "";
                                try
                                {
                                    nombreEstado = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFCredito");
                                }
                                catch (Exception ex)
                                {
                                    nombreEstado = "EstadoDeCuenta.pdf";
                                    ApmNoticeWrapper.NoticeException(ex);
                                }
                                //generando el estado de cuenta
                                bool enviaCorreo = false;
                                bool estadoDeCuenta = false;

                                if (sinDeuda && sumaComisiones == 0 && SumaIntOrd == 0)
                                {
                                    Logueo.Evento("[GeneraEstadoCuentaCredito] generando pdf sin deuda");
                                    //   estadoDeCuenta = LNOperacionesEdoCuenta.edoCuentaPDF(archivos, userCR, passCR, hostCR, databaseCR, ID_Corte, ruta, _daoCortes, conn2, rutaImagen, cuenta.ClaveCliente, rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, nombreEstado, sinDeuda);
                                    estadoDeCuenta = LNOperacionesEdoCuenta.edoCuentaPDFV2(archivos, userCR, passCR, hostCR, databaseCR, ID_Corte, ruta, _daoCortes, conn2, rutaImagen, cuenta.ClaveCliente, rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, nombreEstado, sinDeuda, cuentasAdicionales);
                                }
                                else if (generaEstadoDeCuenta)//generarFacturaTimbrada)
                                {
                                    Logueo.Evento("[GeneraEstadoCuentaCredito] generando pdf timbrado");
                                    //  estadoDeCuenta = LNOperacionesEdoCuenta.edoCuentaPDF(archivos, userCR, passCR, hostCR, databaseCR, ID_Corte, ruta, _daoCortes, conn2, rutaImagen, cuenta.ClaveCliente, rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, nombreEstado, false);
                                    estadoDeCuenta = LNOperacionesEdoCuenta.edoCuentaPDFV2(archivos, userCR, passCR, hostCR, databaseCR, ID_Corte, ruta, _daoCortes, conn2, rutaImagen, cuenta.ClaveCliente, rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, nombreEstado, false, cuentasAdicionales);
                                }
                                else if (!generaEstadoDeCuenta)//con el otro provedor este if va separado pues se genera el estado y aparte un txt peor en este caso no
                                {
                                    Logueo.Evento("[GeneraEstadoCuentaCredito] generando pdf con deuda pero sin timbrar");
                                    //    estadoDeCuenta = LNOperacionesEdoCuenta.edoCuentaPDF(archivos, userCR, passCR, hostCR, databaseCR, ID_Corte, ruta, _daoCortes, conn2, rutaImagen, cuenta.ClaveCliente, rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, nombreEstado, true);
                                    estadoDeCuenta = LNOperacionesEdoCuenta.edoCuentaPDFV2(archivos, userCR, passCR, hostCR, databaseCR, ID_Corte, ruta, _daoCortes, conn2, rutaImagen, cuenta.ClaveCliente, rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, nombreEstado, true, cuentasAdicionales);
                                    //estadoDeCuenta = LNOperacionesEdoCuenta.edoCuentaText(archivos, userCR, passCR, hostCR, databaseCR, ID_Corte, rutaTxt, _daoCortes, conn2, rutaImagen, cuenta.ClaveCliente, rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, nombreEstado, sinDeuda, cuenta, incrementoFolio, servicioDatos_.numeroCuentas);
                                }
                                if (estadoDeCuenta)
                                {
                                    listaRutasEdoCuenta.Add(ruta + nombreEstado);
#if PIEK
                                    listaParametrosSFTP.Add(new ParametrosSFTP { rutaNuevoArchivo = rutaSFTP, rutaOriginal = ruta + nombreEstado, cliente = cuenta.ClaveCliente, rutaCert = rutaSFTPCer, rutaKey = rutaSFTPKey });
                                    Logueo.Evento("[GeneraEstadoCuentaCredito] ruta:" + rutaSFTP);
#endif

                                }

                                fechaCreacion = Hoy.ToString("dd-MM-yyyy");

                                bool envioCorreoCred = Convert.ToBoolean(envioCorreoCredito);
                                if (envioCorreoCred && estadoDeCuenta)//no se enviaran facturas
                                {


#if ADICIONALES
                                    //adicionales

                                    //List<CuentaAdicional> cuentasAdicionales
                                    if (cuentasAdicionales != null && cuentasAdicionales.Count > 0)
                                    {
                                        foreach (CuentaAdicional cuentaAdicional in cuentasAdicionales)
                                        {

                                            if (!string.IsNullOrEmpty(cuentaAdicional.CorreoCuentahabiente))
                                            {
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] enviando correo adicional");

                                                Correo _correo = new Correo
                                                {
                                                    asunto = "Estado de cuenta del" + fechaCreacion,
                                                    correoReceptor = cuentaAdicional.CorreoCuentahabiente,
                                                    mensaje = PNConfig.Get("PROCESAEDOCUENTA", "AsuntoCorreo"),
                                                    archivos = cuentaAdicional.Archivos,
                                                    titulo = PNConfig.Get("PROCESAEDOCUENTA", "AsuntoCorreo"),
                                                    correoEmisor = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioCorreo"),
                                                    cuerpoMensaje = PNConfig.Get("PROCESAEDOCUENTA", "BodyCorreoElectronico"),
                                                    host = PNConfig.Get("PROCESAEDOCUENTA", "HostCorreo"),
                                                    password = PNConfig.Get("PROCESAEDOCUENTA", "PasswordCorreo"),
                                                    puerto = PNConfig.Get("PROCESAEDOCUENTA", "PuertoCorreo"),
                                                    usuario = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioCorreo"),
                                                    rutaimagen = PNConfig.Get("PROCESAEDOCUENTA", "RutaLogoCreditoCorreo"),
                                                    sobrenombre = PNConfig.Get("PROCESAEDOCUENTA", "NombreAMostrarCorreo"),
                                                }; //"Estado De Cuenta Cacao" };
                                                envioCorreo = _lnEnvioCorreo.envioCorreoConImagen(_correo, cuentaAdicional, respuestaSolicitud);
                                                if (envioCorreo)
                                                {
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] Correo Edo Cuenta Adicional Enviado");

                                                }
                                                else
                                                {
                                                    Logueo.Error("[GeneraEstadoCuentaCredito] [Error al generar el PDF o envio de correo adicional] [Mensaje:No se envío el correo]");

                                                }

                                            }
                                        }
                                    }

#endif

                                    if (!string.IsNullOrEmpty(cuenta.CorreoCuentahabiente))
                                    {
                                        Logueo.Evento("[GeneraEstadoCuentaCredito] enviando correo");
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
                                            usuario = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioCorreo"),
                                            rutaimagen = PNConfig.Get("PROCESAEDOCUENTA", "RutaLogoCreditoCorreo"),
                                            sobrenombre = PNConfig.Get("PROCESAEDOCUENTA", "NombreAMostrarCorreo"),
                                        }; //"Estado De Cuenta Cacao" };
                                        envioCorreo = _lnEnvioCorreo.envioCorreoConImagen(_correo, cuenta, respuestaSolicitud);
                                        if (envioCorreo)
                                        {
                                            Logueo.Evento("[GeneraEstadoCuentaCredito] Correo Enviado");
                                            respuestaSolicitud.codigoRespuesta = "0000";
                                            respuestaSolicitud.descripcionRespuesta = "Correcto";
                                        }
                                        else
                                        {
                                            respuestaSolicitud.codigoRespuesta = "0003";
                                            respuestaSolicitud.descripcionRespuesta = "Error al generar Estado de cuenta";
                                            Logueo.Error("[GeneraEstadoCuentaCredito] [Error al generar el PDF o envio de correo] [Mensaje:No se envío el correo]");

                                        }
                                    }
                                }
                                //insertar registro de correo y generacion en la base de datos
                                // cuenta.ID_Corte
                                Logueo.Evento("[GeneraEstadoCuentaCredito] Actualizando estatus envio correo");
                                _daoCortes.ActualizarDatoCorreoYPDF(estadoDeCuenta, envioCorreo, ID_Corte, conn2, null, (estadoDeCuenta ? (ruta + nombreEstado) : (ruta)));
                            }

                        } //DE LAS CUENTAS A CORTAR HOY fin foreach
                          //pasando archivos a sftp

                        string servidorSFTP = PNConfig.Get("PROCESAEDOCUENTA", "ServidorSFTP");
                        string puertoSFTP = PNConfig.Get("PROCESAEDOCUENTA", "PuertoSFTP");
                        string usuarioSFTP = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioSFTP");
                        string passUsuarioSFTP = PNConfig.Get("PROCESAEDOCUENTA", "PassUsuarioSFTP");
                        string carpetaEstadosDecuenta = PNConfig.Get("PROCESAEDOCUENTA", "CarpetaEstadosDecuenta");
                        string carpetaInicial = PNConfig.Get("PROCESAEDOCUENTA", "CarpetaInicialSFTP");
                        //string rutaTxt = _validaciones.validaParametroDiccionario(Todos_losParametros, "@RutaGenEdC") ? Todos_losParametros["@RutaGenEdC"].Valor : ruta;
                        Logueo.Evento("[GeneraEstadoCuentaCredito] enviando por sftp");
                        try
                        {
                            KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(usuarioSFTP);
                            keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(LNOperacionesCorte.HandleKeyEvent);
#if !SFTP114
                            Renci.SshNet.ConnectionInfo conInfo = new Renci.SshNet.ConnectionInfo(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, keybAuth);

                            //
                            //using (SftpClient sftp = new SftpClient(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, passUsuarioSFTP))
                            using (SftpClient sftp = new SftpClient(conInfo))
                            {
#else

                            using (SftpClient sftp = new SftpClient(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, passUsuarioSFTP))
                            {
#endif
                                sftp.Connect();
                                Logueo.Evento("[GeneraEstadoCuentaCredito]  directorio trabajo:" + sftp.WorkingDirectory);
                                string directoryMain = sftp.WorkingDirectory;// + "/" + carpetaInicial;
                                foreach (ParametrosSFTP parametros in listaParametrosSFTP)
                                { //}
                                    Logueo.Evento("[GeneraEstadoCuentaCredito]  directorio:" + directoryMain);
                                    sftp.ChangeDirectory(directoryMain + "/" + parametros.rutaNuevoArchivo);
                                    //foreach (string rutaEdo in listaRutasEdoCuenta)
                                    //{
                                    bool estadoGenerado = false;
                                    try
                                    {
                                        Logueo.Evento("[GeneraEstadoCuentaCredito] enviando por sftp" + carpetaEstadosDecuenta);
                                        estadoGenerado = SFTPService.CreateFileAndDirectoryWithoutConnection(sftp, parametros.rutaOriginal, carpetaEstadosDecuenta);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logueo.Error("[GeneraEstadoCuentaCredito] error al copiar a sftp:" + ex.Message + ", " + parametros.rutaOriginal);
                                        ApmNoticeWrapper.NoticeException(ex);
                                    }
                                    if (!estadoGenerado)
                                    {
                                        Logueo.Error("[GeneraEstadoCuentaCredito] error al copiar aarchivo sftp");

                                    }
                                    Logueo.Evento("[GeneraEstadoCuentaCredito] cambiar directorio");
                                    sftp.ChangeDirectory(directoryMain);
                                }
                                sftp.Disconnect();
                            }
                        }
                        catch (Exception exEdo)
                        {
                            Logueo.Error("[GeneraEstadoCuentaCredito] error al conectar a sftp:" + exEdo.Message);
                            ApmNoticeWrapper.NoticeException(exEdo);
                        }
                        Logueo.Evento("[GeneraEstadoCuentaCredito] enviado");


                    }
                    catch (Exception exConection)
                    {
                        Logueo.Error("[GeneraEstadoCuentaCredito] error al iniciar coneccion:" + exConection.Message + " " + exConection.StackTrace);
                        ApmNoticeWrapper.NoticeException(exConection);
                    }
                    finally
                    {
                        conn2.Close();
                    }
                } //using conection


            }
            catch (Exception err)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al Inicio:" + err.Message + " " + err.StackTrace);
                ApmNoticeWrapper.NoticeException(err);
            }
            Logueo.Evento("[GeneraEstadoCuentaCredito] Fin proceso corte");
            return true;
        }

        //private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
        //{
        //    string passUsuarioSFTP = PNConfig.Get("PROCESAEDOCUENTA", "PassUsuarioSFTP");
        //    foreach (AuthenticationPrompt Prompt in e.Prompts)
        //    {
        //        if (Prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
        //        {
        //            Prompt.Response = passUsuarioSFTP;
        //        }
        //    }
        //}

        /// <summary>
        /// metodo que obtiene el valor del template para realizar el formato
        /// </summary>
        /// <param name="tabla"></param>
        /// <returns></returns>
        private string obtenerValorTemplateDatos(DataTable tabla)
        {
            string templateSalida = string.Empty;

            foreach (DataRow rows in tabla.Rows)
            {
                if (rows["Nombre"].ToString() == "@Template_Salida")
                {
                    templateSalida = string.IsNullOrEmpty(rows["Valor"].ToString()) ?
                        string.Empty
                        : rows["Valor"].ToString();
                    break;
                }
            }

            return templateSalida;
        }

        /// <summary>
        /// Metodo que obtiene el valor del formato para el template
        /// </summary>
        /// <param name="conexion"></param>
        /// <param name="claveColectiva"></param>
        /// <returns></returns>
        private string obtenerTemplateReporte(string cadenaConexion, string cadenaConexionPN, string claveColectiva)
        {
            const string PROCESO = "PROCESAEDOCUENTA";
            const string CODIGO_EXITOSO = "00";

            string templateSeleccionado = string.Empty;
            string template = string.Empty;

            DataTable tablaConfiguracionPN = new DataTable();


            DataTable tablaContrato = new DAOCortes().ObtenerDatosContratosPorColectiva(claveColectiva, cadenaConexion, null);
            if (tablaContrato.Rows.Count > 0)
            {
                templateSeleccionado = obtenerValorTemplateDatos(tablaContrato);
                if (!string.IsNullOrEmpty(templateSeleccionado))
                    tablaConfiguracionPN = new DAOCortes().ObtenerDatosTemplateParaMovimiento(templateSeleccionado, PROCESO, cadenaConexionPN);
                else
                    return template;

                if (tablaConfiguracionPN.Rows[0]["Codigo"].ToString() == CODIGO_EXITOSO)
                {
                    template = tablaConfiguracionPN.Rows[0]["ClaveTemplate"].ToString();
                }
            }
            return template;
        }


    }
}


