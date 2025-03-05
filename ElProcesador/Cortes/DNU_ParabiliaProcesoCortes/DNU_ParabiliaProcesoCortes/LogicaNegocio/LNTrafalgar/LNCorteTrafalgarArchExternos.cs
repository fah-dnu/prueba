using CommonProcesador;
using DNU_NewRelicNotifications.Services.Wrappers;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Contratos;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.LogicaNegocio.LNCortes;
using DNU_ParabiliaProcesoCortes.LogicaNegocio.LNTrafalgar;
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

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio.LNTrafalgar
{
    class LNCorteTrafalgarArchExternos : Corte
    {
        XslCompiledTransform _transformador;
        public LNCorteTrafalgarArchExternos(XslCompiledTransform _transformador)
        {
            string ArchXSLT = "";
            if (_transformador is null) {
                try
                {
                    ConfiguracionContexto.InicializarContexto();
                    ArchXSLT = PNConfig.Get("PROCESAEDOCUENTA", "ArchivoXSLT");
                    _transformador = new XslCompiledTransform();
                    _transformador.Load(ArchXSLT);

                }
                catch (Exception es)
                {
                    Logueo.Error("[GeneraReporte] [Error al generar el corte] [Mensaje: " + es.Message + " TRACE: " + es.StackTrace + "]" + ArchXSLT);
                }
            }
            this._transformador = _transformador;
        }

        public override bool inicio(string idOrig=null,string fecha = null)
        {
            LNFacturacion lnFacturacion = new LNFacturacion();
            //prueba excel
            // LNCreacionExcel.escribirEnExcel();

            Guid idLog = Guid.NewGuid();
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


            //LNOperaciones.crearDirectorio(rutaImagen);
            bool pdfGeneradoCorrectamente = false;
            string nombrePdfCredito = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFCredito");
            string nombrePdfPrepago = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFDebito");


            //Procesando cuentas
            //lo de arriba solo es para procesar el pdf y el envio del correo, lo que sigue ya es el proceso de generacion
            // LNBonificacion _lnBonificacion = new LNBonificacion();
            string urlAccesoServicioSAT = PNConfig.Get("PROCESAEDOCUENTA", "URLAccesoAlServicioSAT"); //ConfigurationManager.AppSettings["URLAccesoAlServicioSAT"].ToString();

            //LNFacturas.generaCodigoQR(ruta+"prueba.png", "   ");
            //prueba crystal reports
            string userCR = "";
            string passCR = "";
            string hostCR = "";
            string databaseCR = "";
            Cuentas cuentaPac = new Cuentas();
            RespuestaSolicitud respuestaSolicitud;

            try
            {
                respuestaSolicitud = new RespuestaSolicitud();

                //primero obtenermos los archivos de AWS
                List<String> listaDirectoriosDia = new List<string>();
                string directorioEntrada = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioEntradaArchivos");
                LNCargaArchivo.crearDirectorio(directorioEntrada, directorioEntrada);
               // LNOperacionesCorte.descargarArchivoAWSSFTP(directorioEntrada, listaDirectoriosDia);
                //en este punto ya se descargaron los archivos en ArchivosCortesTrafalgar\CargaArchivos\Xsolicitud
                //inicio proceso de carga en BD(ya de forma local con los archivos descargados)
                //prueba
               // listaDirectoriosDia.Add("290323");
                //
                List<Archivo> archivosLog = new List<Archivo>();
                string carpetaEstadosDecuentaAWS = PNConfig.Get("PROCESAEDOCUENTA", "CarpetaEstadosDecuenta");
                if (listaDirectoriosDia.Count > 0)
                {
                    Thread.Sleep(2000);//para que temrine de copiar losa archivos
                    Archivo archivoLog = new Archivo();
                    archivoLog.Ruta = carpetaEstadosDecuentaAWS;//ruta externa
                                                                //procesar archivos que se acaban de bajar del sftp
                    string[] listaCarpetasAws = carpetaEstadosDecuentaAWS.Split('/');
                    string ultimaCarpetasAws = listaCarpetasAws[listaCarpetasAws.Length - 2] + "\\";
                    new LNCargaArchivo().leerArchivosManual(false, idLog.ToString(), listaDirectoriosDia, directorioEntrada, archivoLog, directorioEntrada+ ultimaCarpetasAws);
                    archivoLog.FechaProcesamiento = DateTime.Now.ToString("ddMMyyyy");
                    //generacion del log salida

                }

                //validando debito trafalagar
                listaDirectoriosDia = new List<string>();
                DateTime fechaActual = DateTime.Now;
                string fechaFormateada = fechaActual.ToString("ddMMyy");
                ArchivoXLSX datosArchivoXLSX = new ArchivoXLSX();//esto es para el archivo xlsx
                datosArchivoXLSX.Detalle = new List<DetalleArchivoXLSX>();
                var random = new Random();
                DateTime fechaHoraActual = DateTime.Now;
                string idSolicitud = random.Next(1, 9).ToString();
                string claveIdentificacion = fechaHoraActual.ToString("yyyyMMddHHmmss") + idSolicitud;//fechaHoraActual.ToString("yyyyMMddHHmmssffff") + idSolicitud;
                datosArchivoXLSX.Nombre = "SalidaEstadosDeCuenta_"+ claveIdentificacion;//+ fechaFormateada;
                int registrosProcesados = 0;

                string directorioSalidaCTA = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalidaArchivos");//salida archivos
                directorioSalidaCTA = directorioSalidaCTA + "Procesados\\" + fechaFormateada + "\\";
                // fecha = "290622";
                try
                {
                    DataTable colectivaTimbrado = new DataTable();
                    string idFacturaExterna = "";
                    Factura laFacturaExt;
                    Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo  cliente externo ");
                    //obteniendo parametros
                    Bonificacion bonificacionClienteExterno = new Bonificacion { ClaveColectiva = PNConfig.Get("PROCESAEDOCUENTA", "ClaveColectivaFacturasExternasTRF") /*"99000091"*/, Tarjeta = "" };
                    Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo parametros cliente externo,  colectiva:"+ bonificacionClienteExterno.ClaveColectiva);
                    string conection = PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizador");
                    Dictionary<String, Parametro> parametrosCTAExt = new Dictionary<string, Parametro>();
                    using (SqlConnection conn2 = new SqlConnection(conection))//DBProcesadorArchivo.strBDLecturaAutorizador))
                    {
                        parametrosCTAExt = _daoCortes.ObtenerDatosParametrosCliente(bonificacionClienteExterno, new DataTable(), conn2, null);
                        // colectivaTimbrado = new DataTable();// _daoCortes.ObtenerDatosColectivaTimbre(null, null, conn2);
                        colectivaTimbrado = _daoCortes.ObtenerDatosColectivaTimbre(bonificacionClienteExterno.ClaveColectiva, null, conn2);

                    }
                    if (colectivaTimbrado.Rows.Count > 0)
                    {
                        cuentaPac.id_colectivaCliente = Convert.ToInt64(colectivaTimbrado.Rows[0]["ID_colectiva"].ToString());
                        cuentaPac.NombreORazonSocial = colectivaTimbrado.Rows[0]["NombreORazonSocial"].ToString();
                        cuentaPac.RFCCliente = colectivaTimbrado.Rows[0]["RFC"].ToString();
                        cuentaPac.CP = colectivaTimbrado.Rows[0]["CP"].ToString();

                    }
                    else
                    {
                        cuentaPac.NombreORazonSocial = "TRAFALGAR DIGITAL, INSTITUCION DE FONDOS DE PAGO ELECTRONICO";//colectivaTimbrado.Rows[0]["NombreORazonSocial"].ToString();
                        cuentaPac.RFCCliente = "TDI190405HF5";//colectivaTimbrado.Rows[0]["RFC"].ToString();
                        cuentaPac.CP = "06500";//colectivaTimbrado.Rows[0]["CP"].ToString();
                    }
                    
                    Logueo.Evento("[GeneraEstadoCuentaCredito] Obteniendo cortes de cliente externo " + cultura);
                    List<DetalleFactura> listaDetallesFacturaXML = new List<DetalleFactura>();
                    Logueo.Evento("[GeneraEstadoCuentaCredito] conexion con base de archivos(Batch) ");
                    string connArchivo=PNConfig.Get("PROCESAEDOCUENTA", "BDWriteProcesadorArchivosCacao");//
                    using (SqlConnection conn2 = new SqlConnection(connArchivo))//DBProcesadorArchivo.strBDEscrituraArchivo))
                    {
                        conn2.Open();
                        try
                        {

                            //DataSet ds = _daoCortes.ObtenerDatosPagos(ID_Corte.ToString(), null, conn);
                            foreach (DataRow row in _daoCortes.ObtenerCortesExternos(conn2).Rows)
                            {
                                DetalleArchivoXLSX detalleRegistro = new DetalleArchivoXLSX();
                                string id = "";
                                try
                                {

                                    laFacturaExt = new Factura();
                                    listaDetallesFacturaXML = new List<DetalleFactura>();
                                    Logueo.Evento("[GeneraEstadoCuentaCredito]  cortes de cliente externo " + cultura);
                                    id = row["Id_ArchivoDetalleEdoCtaExterno"].ToString();
                                    string fechaEx = row["AnioMes"].ToString();
                                    string clienteId = row["ClienteID"].ToString();
                                    string nombre = row["NombreCompleto"].ToString();
                                    string rfc = row["RFC"].ToString();
                                    string direccionCompleta= row["DireccionCompleta"].ToString();
                                    string regimenFiscal = row["RegimenFiscal"].ToString();
                                    string usoCFDI = row["UsoCFDI"].ToString();
                                    string domicilioFiscalReceptor = row["CodigoPostal"].ToString();
                                    DateTime fechaInsercion =Convert.ToDateTime(row["FechaInsercion"].ToString());
                                   // DateTime.Parse(elTimbre.FechaTimbrado);


                                    //obteniendo cuentas por cliente
                                    ClienteExterno clienteExterno = new ClienteExterno();
                                    clienteExterno.cuentas = new List<CuentaAhorroCLABE>();
                                    DataTable tablaCuentasPorCliente = _daoCortes.ObtenerCuentasPorCliente(fechaEx, id, conn2);
                                    if (tablaCuentasPorCliente != null && tablaCuentasPorCliente.Rows.Count > 0)
                                    {
                                        clienteExterno = LNCorteExterno.obtenerCuentasPorcliente(tablaCuentasPorCliente);
                                        clienteExterno.clientId = clienteId;
                                    }
                                    //cuentas obtenidas

                                    //string nombreArchivo = fechaEx + "-" + clienteId + "-" + nombre + ".pdf";
                                    string nombreArchivo = clienteId.PadLeft(10, '0') + "-" + fechaEx + ".pdf";
                                    nombreArchivo = nombreArchivo.Replace("?", "").Replace(":", "").Replace("\\", "").Replace("/", "").Replace("|", "").Replace("*", "").Replace("<", "").Replace(">", "");
                                 
                                    string nombreArchivoXML = nombreArchivo.Replace(".pdf", "");
                                    //validar si tiene tiene comisiones para hacer el timbrado
                                    Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo comisiones" + cultura+" c:"+ domicilioFiscalReceptor);
                                    DataTable tablaBusquedaComsiones = _daoCortes.ObtenerDatosComisiones(id, null, conn2);
                                    bool generarPDFExterno = true;
                                    bool generatimbre = true;
                                    if (generatimbre && (tablaBusquedaComsiones.Rows.Count > 0 && tablaBusquedaComsiones.Rows[0][0].ToString() == "0"))
                                    {

                                        decimal sumaComisiones = Convert.ToDecimal(tablaBusquedaComsiones.Rows[0]["SumaComiones"].ToString());
                                        decimal sumaIvaComisiones = Convert.ToDecimal(tablaBusquedaComsiones.Rows[0]["SumaIvaComiones"].ToString());
                                        // decimal sumaComisiones = Convert.ToDecimal(tablaBusquedaComsiones.Rows[0]["SumaComiones"].ToString("###,###,##0.000000"));
                                        //decimal sumaIvaComisiones = Convert.ToDecimal(tablaBusquedaComsiones.Rows[0]["SumaIvaComiones"].ToString("###,###,##0.000000"));

                                        string fecha_actualExt = DateTime.Now.ToString("yyyyMMddhhmmss");//5491XXXXXXXX6877
                                                                                                         //String folioFactura = inicioFolio + fecha_actual + (incrementoFolio.ToString().PadLeft(3, '0').Substring(0, 3));
                                        String folioFacturaExt = /*inicioFolio +*/ fecha_actualExt + (id.PadLeft(8, '0').Substring(0, 8));
                                        // laFacturaExt = LNFacturas.obtenerDatosFactura(parametrosCTAExt, sumaComisiones, sumaIvaComisiones, Convert.ToDecimal(0.16), 0, 0, 0, false, new Cuentas { NombreCuentahabiente = nombre, RFCCuentahabiente = rfc, UsoCFDI=usoCFDI, RegimenFiscal=regimenFiscal }, new List<DetalleFactura>(), listaDetallesFacturaXML, _validaciones, folioFacturaExt, cuentaPac);
                                        laFacturaExt = lnFacturacion.obtenerDatosFacturaCFDI4(parametrosCTAExt, sumaComisiones, sumaIvaComisiones, Convert.ToDecimal(0.16), 0, 0, 0, false, new Cuentas { NombreCuentahabiente = nombre, RFCCuentahabiente = rfc, UsoCFDI = usoCFDI, RegimenFiscal = regimenFiscal,CPFiscal= domicilioFiscalReceptor }, new List<DetalleFactura>(), listaDetallesFacturaXML, _validaciones, folioFacturaExt, cuentaPac, new List<DetalleFactura>());
                                        Logueo.Evento("[GeneraEstadoCuentaCredito] Generando XML insertar factura");
                                        DataTable tablaDatoFactura = _daoFacturas.InsertarFacturaExterna(laFacturaExt, Convert.ToInt64(id), null, null, false);
                                        Logueo.Evento("[GeneraEstadoCuentaCredito] Generando XML factura insertada");
                                        Response errores = new Response();
                                       // detalleRegistro.NombreEstadoCuenta = nombreArchivo;
                                       
                                        bool facturaGeneradacorrectamente = false;
                                        if (_validaciones.BusquedaSinErrores(errores, tablaDatoFactura))
                                        {
                                            facturaGeneradacorrectamente = true;
                                            idFacturaExterna = tablaDatoFactura.Rows[0][1].ToString();
                                            Logueo.Evento("[GeneraEstadoCuentaCredito] Generando XML timbrara XML");
                                            if (lnFacturacion.generarFacturaV4(laFacturaExt, directorioSalidaCTA, false, "", listaDirectoriosDia, cuentaPac, _transformador, nombreArchivo, false, nombreArchivoXML))
                                            // if (LNOperacionesCorte.generarFactura(laFacturaExt, directorioSalidaCTA, false, "", listaDirectoriosDia, cuentaPac, _transformador, nombreArchivo, false, nombreArchivoXML))
                                            {//una vez que se genera la factura ya no hay forma de hacer rollback
                                                try
                                                {
                                                    detalleRegistro.FechaTimbrado = laFacturaExt.FechaTimbrado;
                                                    detalleRegistro.AnioMes = fechaEx;
                                                    detalleRegistro.ClienteID = clienteId;
                                                    detalleRegistro.NombreCompleto = nombre;
                                                    detalleRegistro.RFCReceptor = rfc;
                                                    detalleRegistro.Folio = laFacturaExt.Folio;
                                                    detalleRegistro.DireccionCompleta = direccionCompleta;
                                                    detalleRegistro.UUID = laFacturaExt.UUID;
                                                    detalleRegistro.IVA = laFacturaExt.IVA;
                                                    detalleRegistro.SubTotal = laFacturaExt.SubTotal;
                                                    detalleRegistro.ImporteTotal = (Decimal.Round(laFacturaExt.SubTotal, 2) + Decimal.Round(laFacturaExt.IVA, 2));//laFacturaExt.ImporteTotal;
                                                    detalleRegistro.NombreEmisor = laFacturaExt.Emisora.NombreORazonSocial;
                                                    detalleRegistro.Timbrado = true;
                                                    detalleRegistro.RFCEmisor = laFacturaExt.Emisora.RFC;
                                                    detalleRegistro.FechaRegistro = fechaInsercion;

                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]  XML Timbrado");
                                                    string codigoQR = urlAccesoServicioSAT + "?id=" + laFacturaExt.UUID + "&re=" + (laFacturaExt.Emisora.RFC.Length > 13 ? laFacturaExt.Emisora.RFC.Substring(0, 13) : laFacturaExt.Emisora.RFC) + "&rr=" + (laFacturaExt.Receptora.RFC.Length > 13 ? laFacturaExt.Receptora.RFC.Substring(0, 13) : laFacturaExt.Receptora.RFC) + "&tt=" +
                                                    (Decimal.Round(laFacturaExt.SubTotal, 2) + Decimal.Round(laFacturaExt.IVA, 2)) + "&fe=" + laFacturaExt.Sello.Substring((laFacturaExt.Sello.Length - 8), 8);
                                                    string rutaImagenQR = directorioSalidaCTA + nombreArchivo.Replace(".pdf", "") + "imagenQR.png";
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]  codigo QR:" + rutaImagenQR + codigoQR);
                                                    LNFacturas.generaCodigoQR(rutaImagenQR, codigoQR);
                                                    laFacturaExt.UrlQrCode = false ? "" : rutaImagenQR;
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] Actualizando factura externa");
                                                    DataTable tablaDatoFacturaInserta = _daoFacturas.ActualizaFactura(Convert.ToInt64(idFacturaExterna), laFacturaExt, Convert.ToInt64(id), null, null, false);
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]  Factura externa actualizada");
                                                    errores = new Response();
                                                    if (_validaciones.BusquedaSinErrores(errores, tablaDatoFacturaInserta))
                                                    {

                                                    }
                                                    else
                                                    {
                                                        Logueo.Error("[GeneraEstadoCuentaCredito] error al actualizar factura externa, corte:" + id + " " + errores.CodRespuesta + " " + errores.DescRespuesta);
                                                        generarPDFExterno = false;

                                                    }

                                                }
                                                catch (Exception ex)
                                                {
                                                    Logueo.Error("[GeneraEstadoCuentaCredito] error al insertar factura externa" + ex.Message + " " + ex.StackTrace);
                                                    generarPDFExterno = false;
                                                }

                                            }
                                            else
                                            {
                                                generarPDFExterno = false;

                                            }
                                        }
                                        else
                                        {
                                            generarPDFExterno = false;
                                        }
                                    }
                                    if (generarPDFExterno)
                                    {
                                        Logueo.Evento("[GeneraEstadoCuentaCredito] generando pdf cliente externo" + cultura);
                                        bool estadoExternoGenerado = false;
                                        if (clienteExterno.cuentas.Count > 1)
                                        {
                                            estadoExternoGenerado = LNOperacionesEdoCuenta.edoCuentaPDFExternoConSubReporte(id, conn2, directorioSalidaCTA, _daoCortes, nombreArchivo, laFacturaExt, clienteExterno, detalleRegistro);
                                        }
                                        else
                                        {
                                            estadoExternoGenerado = LNOperacionesEdoCuenta.edoCuentaPDFExterno(id, conn2, directorioSalidaCTA, _daoCortes, nombreArchivo, laFacturaExt, detalleRegistro);

                                        }
                                        if (estadoExternoGenerado)
                                        {
                                            detalleRegistro.Generado = true;
                                            listaDirectoriosDia.Add(directorioSalidaCTA + nombreArchivo);
                                            Logueo.Evento("[GeneraEstadoCuentaCredito] actualizando estatus cliente externo" + cultura);
                                            DataTable datosRespuesta = _daoCortes.ActualizarCorteExterno(id, conn2);
                                            if (datosRespuesta.Rows[0][0].ToString() == "0")
                                            {
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] Estado de cuenta externo generado");
                                            }
                                            else
                                            {
                                                Logueo.Error("[GeneraEstadoCuentaCredito] [Error al actualizar registro externo]" + datosRespuesta.Rows[0][1]);

                                            }

                                        }
                                        else
                                        {
                                            DataTable datosRespuesta = _daoCortes.ActualizarCorteExternoError(id, conn2);
                                            Logueo.Error("[GeneraEstadoCuentaCredito] [Error al generar el PDF externo] [Mensaje:Errro al generar estado de cuentaarchivo:" + id + "]");

                                        }
                                    }
                                    else
                                    {
                                        DataTable datosRespuesta = _daoCortes.ActualizarCorteExternoError(id, conn2);
                                        Logueo.Error("[GeneraEstadoCuentaCredito] [Error al generar el PDF externo] [Mensaje:Errro  generando estado de cuenta archivo:"+ id + "]");

                                    }
                                }
                                catch (Exception ex)
                                {
                                    DataTable datosRespuesta = _daoCortes.ActualizarCorteExternoError(id, conn2);
                                    Logueo.Error("[GeneraEstadoCuentaCredito] [Error al generar el PDF externo] [Mensaje:Errro al generar estado de cuenta]" + ex.Message + " " + ex.StackTrace);

                                }
                                registrosProcesados++;
                                datosArchivoXLSX.Detalle.Add(detalleRegistro);
                            }

                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[GeneraEstadoCuentaCredito] [Error al generar el PDF externo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");

                        }
                        finally
                        {
                            conn2.Close();
                        }
                    }
                    Logueo.Evento("[GeneraEstadoCuentaCredito] Proceso generacion EDCTA externo finalizado" + cultura);
                }
                catch (Exception exCon)
                {
                    Logueo.Error("[GeneraEstadoCuentaCredito] [Error en cvonexion] [Mensaje: " + exCon.Message + " TRACE: " + exCon.StackTrace + "]");
                }

              
                if (datosArchivoXLSX.Detalle.Count > 0)
                {
                   
                    foreach (DetalleArchivoXLSX detalle in datosArchivoXLSX.Detalle)
                    {
                        if (detalle.Generado)
                        {
                            datosArchivoXLSX.EstadosDeCuentaCorrectos = datosArchivoXLSX.EstadosDeCuentaCorrectos + 1;
                        }
                        else
                        {
                            datosArchivoXLSX.EstadosDeCuentaErroneos = datosArchivoXLSX.EstadosDeCuentaErroneos + 1;
                        }

                        if (detalle.Timbrado)
                        {
                            datosArchivoXLSX.EstadosDeCuentaTimbrados = datosArchivoXLSX.EstadosDeCuentaTimbrados + 1;
                        }

                    }

                    datosArchivoXLSX.EstadosDeCuentaTotales = datosArchivoXLSX.Detalle.Count;
                    string fechaExcel = DateTime.Now.ToString("dd-MM-yyyy_HHmmss");
                    string nombreExcel = "ResumenArchivosGenerados_" + fechaExcel;
                    if (LNCreacionExcel.escribirEnExcelDatosEdocuenta(datosArchivoXLSX, directorioSalidaCTA))
                    {
                        Logueo.Evento("[GeneraEstadoCuentaCredito] escribiendo archivo exel en:" + directorioSalidaCTA);
                        //if (datosArchivoXLSX.EstadosDeCuentaTimbrados > 0)
                        // {
                        listaDirectoriosDia.Add(directorioSalidaCTA + datosArchivoXLSX.Nombre + ".xlsx");
                        //  }
                    }
                }

                Logueo.Evento("[GeneraEstadoCuentaCredito] suebiendo archivos a aws");
                if (listaDirectoriosDia.Count > 0)//solo comentera en determinadas pruebas
                {
                      LNOperacionesCorte.subirArhivosAWSSFTP(listaDirectoriosDia, fechaFormateada);
                }

            }
            catch (Exception err)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al Inicio:" + err.Message + " " + err.StackTrace);
                ApmNoticeWrapper.NoticeException(err);
            }
            Logueo.Evento("[GeneraEstadoCuentaCredito] Fin proceso corte");
            return true;
        }

        public override bool inicioSinConexionSFTP(string fecha = null)
        {
            Guid idLog = Guid.NewGuid();
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


            //LNOperaciones.crearDirectorio(rutaImagen);
            bool pdfGeneradoCorrectamente = false;
            string nombrePdfCredito = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFCredito");
            string nombrePdfPrepago = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFDebito");

            string urlAccesoServicioSAT = PNConfig.Get("PROCESAEDOCUENTA", "URLAccesoAlServicioSAT"); //ConfigurationManager.AppSettings["URLAccesoAlServicioSAT"].ToString();

            Cuentas cuentaPac = new Cuentas();
            RespuestaSolicitud respuestaSolicitud;
            try
            {
                respuestaSolicitud = new RespuestaSolicitud();

                DateTime fechaActual = DateTime.Now;
                string fechaFormateada = fechaActual.ToString("ddMMyy");
                ArchivoXLSX datosArchivoXLSX = new ArchivoXLSX();//esto es para el archivo xlsx
                datosArchivoXLSX.Detalle = new List<DetalleArchivoXLSX>();
                datosArchivoXLSX.Nombre = "SalidaEstadosDeCuenta_" + fechaFormateada;
                int registrosProcesados = 0;
                // fecha = "290622";
                try
                {
                    DataTable colectivaTimbrado = new DataTable();
                    string idFacturaExterna = "";
                    Factura laFacturaExt;
                    Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo  cliente externo ");
                    //obteniendo parametros
                    Bonificacion bonificacionClienteExterno = new Bonificacion { ClaveColectiva = PNConfig.Get("PROCESAEDOCUENTA", "ClaveColectivaFacturasExternasTRF") /*"99000091"*/, Tarjeta = "" };
                    Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo parametros cliente externo ");
                    Dictionary<String, Parametro> parametrosCTAExt = new Dictionary<string, Parametro>();
                    using (SqlConnection conn2 = new SqlConnection(DBProcesadorArchivo.strBDLecturaAutorizador))
                    {
                        parametrosCTAExt = _daoCortes.ObtenerDatosParametrosCliente(bonificacionClienteExterno, new DataTable(), conn2, null);
                        colectivaTimbrado = _daoCortes.ObtenerDatosColectivaTimbre(null, null, conn2);
                    }
                    if (colectivaTimbrado.Rows.Count > 0)
                    {
                        cuentaPac.id_colectivaCliente = Convert.ToInt64(colectivaTimbrado.Rows[0]["ID_colectiva"].ToString());
                        cuentaPac.NombreORazonSocial = colectivaTimbrado.Rows[0]["NombreORazonSocial"].ToString();
                        cuentaPac.RFCCliente = colectivaTimbrado.Rows[0]["RFC"].ToString();
                        cuentaPac.CP = colectivaTimbrado.Rows[0]["CP"].ToString();

                    }
                    Logueo.Evento("[GeneraEstadoCuentaCredito] Obteniendo cortes de cliente externo " + cultura);

                    List<DetalleFactura> listaDetallesFacturaXML = new List<DetalleFactura>();
                    Logueo.Evento("[GeneraEstadoCuentaCredito] conexion con base de archivos(Batch) ");
                    using (SqlConnection conn2 = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                    {
                        conn2.Open();
                        try
                        {
                            string directorioSalidaCTA = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalidaArchivos");
                            directorioSalidaCTA = directorioSalidaCTA + "Procesados\\" + fechaFormateada + "\\";
                            //DataSet ds = _daoCortes.ObtenerDatosPagos(ID_Corte.ToString(), null, conn);
                            foreach (DataRow row in _daoCortes.ObtenerCortesExternos(conn2).Rows)
                            {
                                DetalleArchivoXLSX detalleRegistro = new DetalleArchivoXLSX();
                                try
                                {

                                    laFacturaExt = new Factura();
                                    listaDetallesFacturaXML = new List<DetalleFactura>();
                                    Logueo.Evento("[GeneraEstadoCuentaCredito]  cortes de cliente externo " + cultura);
                                    string id = row["Id_ArchivoDetalleEdoCtaExterno"].ToString();
                                    string fechaEx = row["AnioMes"].ToString();
                                    string clienteId = row["ClienteID"].ToString();
                                    string nombre = row["NombreCompleto"].ToString();
                                    string rfc = row["RFC"].ToString();
                                    string regimenFiscal = row["RegimenFiscal"].ToString();
                                    string usoCFDI = row["UsoCFDI"].ToString();
                                    string fechaInsercion = row["RFC"].ToString();

                                    //obteniendo cuentas por cliente
                                    ClienteExterno clienteExterno = new ClienteExterno();
                                    clienteExterno.cuentas = new List<CuentaAhorroCLABE>();
                                    DataTable tablaCuentasPorCliente = _daoCortes.ObtenerCuentasPorCliente(fechaEx, id, conn2);
                                    if (tablaCuentasPorCliente != null && tablaCuentasPorCliente.Rows.Count > 0)
                                    {
                                        clienteExterno = LNCorteExterno.obtenerCuentasPorcliente(tablaCuentasPorCliente);
                                        clienteExterno.clientId = clienteId;
                                    }
                                    //cuentas obtenidas

                                    string nombreArchivo = clienteId.PadLeft(10, '0') + "-" + fechaEx + ".pdf";
                                    nombreArchivo = nombreArchivo.Replace("?", "").Replace(":", "").Replace("\\", "").Replace("/", "").Replace("|", "").Replace("*", "").Replace("<", "").Replace(">", "");
                                    //detalleRegistro.NombreEstadoCuenta = nombreArchivo;
                                    //detalleRegistro.FechaCorte = fechaEx;
                                    //detalleRegistro.NombreCliente = nombre;
                                    //detalleRegistro.NumeroCliente = clienteId;
                                    //validar si tiene tiene comisiones para hacer el timbrado
                                    Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo comisiones" + cultura);
                                    DataTable tablaBusquedaComsiones = _daoCortes.ObtenerDatosComisiones(id, null, conn2);
                                    bool generarPDFExterno = true;
                                    bool generatimbre = true;
                                    if (generatimbre && (tablaBusquedaComsiones.Rows.Count > 0 && tablaBusquedaComsiones.Rows[0][0].ToString() == "0"))
                                    {
                                        decimal sumaComisiones = Convert.ToDecimal(tablaBusquedaComsiones.Rows[0]["SumaComiones"].ToString());
                                        decimal sumaIvaComisiones = Convert.ToDecimal(tablaBusquedaComsiones.Rows[0]["SumaIvaComiones"].ToString());
                                        string fecha_actualExt = DateTime.Now.ToString("yyyyMMddhhmmss");//5491XXXXXXXX6877
                                                                                                         //String folioFactura = inicioFolio + fecha_actual + (incrementoFolio.ToString().PadLeft(3, '0').Substring(0, 3));
                                        String folioFacturaExt = /*inicioFolio +*/ fecha_actualExt + (id.PadLeft(8, '0').Substring(0, 8));
                                        laFacturaExt = LNFacturas.obtenerDatosFactura(parametrosCTAExt, sumaComisiones, sumaIvaComisiones, Convert.ToDecimal(0.16), 0, 0, 0, false, new Cuentas { NombreCuentahabiente = nombre, RFCCuentahabiente = rfc }, new List<DetalleFactura>(), listaDetallesFacturaXML, _validaciones, folioFacturaExt, cuentaPac);
                                        Logueo.Evento("[GeneraEstadoCuentaCredito] Generando XML insertar factura");
                                        DataTable tablaDatoFactura = _daoFacturas.InsertarFacturaExterna(laFacturaExt, Convert.ToInt64(id), null, null, false);
                                        Logueo.Evento("[GeneraEstadoCuentaCredito] Generando XML factura insertada");
                                        Response errores = new Response();
                                        detalleRegistro.Folio = folioFacturaExt;
                                        //detalleRegistro.MontoTimbrado = sumaComisiones;
                                        //detalleRegistro.MontoIVATimbrado = sumaIvaComisiones;
                                        bool facturaGeneradacorrectamente = false;
                                        if (_validaciones.BusquedaSinErrores(errores, tablaDatoFactura))
                                        {
                                            facturaGeneradacorrectamente = true;
                                            idFacturaExterna = tablaDatoFactura.Rows[0][1].ToString();
                                            Logueo.Evento("[GeneraEstadoCuentaCredito] Generando XML timbrara XML");
                                            if (LNOperacionesCorte.generarFacturaV4(laFacturaExt, directorioSalidaCTA, false, "", new List<string>(), cuentaPac, _transformador, nombreArchivo, false))
                                            {//una vez que se genera la factura ya no hay forma de hacer rollback
                                                try
                                                {
                                                    detalleRegistro.Timbrado = true;
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]  XML Timbrado");
                                                    string codigoQR = urlAccesoServicioSAT + "?id=" + laFacturaExt.UUID + "&re=" + (laFacturaExt.Emisora.RFC.Length > 13 ? laFacturaExt.Emisora.RFC.Substring(0, 13) : laFacturaExt.Emisora.RFC) + "&rr=" + (laFacturaExt.Receptora.RFC.Length > 13 ? laFacturaExt.Receptora.RFC.Substring(0, 13) : laFacturaExt.Receptora.RFC) + "&tt=" +
                                                    (Decimal.Round(laFacturaExt.SubTotal, 2) + Decimal.Round(laFacturaExt.IVA, 2)) + "&fe=" + laFacturaExt.Sello.Substring((laFacturaExt.Sello.Length - 8), 8);
                                                    string rutaImagenQR = directorioSalidaCTA + nombreArchivo.Replace(".pdf", "") + "imagenQR.png";
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]  codigo QR:" + rutaImagenQR + codigoQR);
                                                    LNFacturas.generaCodigoQR(rutaImagenQR, codigoQR);
                                                    laFacturaExt.UrlQrCode = false ? "" : rutaImagenQR;
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito] Actualizando factura externa");
                                                    DataTable tablaDatoFacturaInserta = _daoFacturas.ActualizaFactura(Convert.ToInt64(idFacturaExterna), laFacturaExt, Convert.ToInt64(id), null, null, false);
                                                    Logueo.Evento("[GeneraEstadoCuentaCredito]  Factura externa actualizada");
                                                    errores = new Response();
                                                    if (_validaciones.BusquedaSinErrores(errores, tablaDatoFacturaInserta))
                                                    {

                                                    }
                                                    else
                                                    {
                                                        Logueo.Error("[GeneraEstadoCuentaCredito] error al actualizar factura externa, corte:" + id + " " + errores.CodRespuesta + " " + errores.DescRespuesta);
                                                        generarPDFExterno = false;

                                                    }

                                                }
                                                catch (Exception ex)
                                                {
                                                    Logueo.Error("[GeneraEstadoCuentaCredito] error al insertar factura externa" + ex.Message + " " + ex.StackTrace);
                                                    generarPDFExterno = false;
                                                }

                                            }
                                            else
                                            {
                                                generarPDFExterno = false;
                                            }
                                        }
                                        else
                                        {
                                            generarPDFExterno = false;
                                        }
                                    }
                                    if (generarPDFExterno)
                                    {
                                        Logueo.Evento("[GeneraEstadoCuentaCredito] generando pdf cliente externo" + cultura);
                                        bool estadoExternoGenerado = false;
                                        if (clienteExterno.cuentas.Count > 1)
                                        {
                                            estadoExternoGenerado = LNOperacionesEdoCuenta.edoCuentaPDFExternoConSubReporte(id, conn2, directorioSalidaCTA, _daoCortes, nombreArchivo, laFacturaExt, clienteExterno, detalleRegistro);
                                        }
                                        else
                                        {
                                            estadoExternoGenerado = LNOperacionesEdoCuenta.edoCuentaPDFExterno(id, conn2, directorioSalidaCTA, _daoCortes, nombreArchivo, laFacturaExt, detalleRegistro);

                                        }
                                        if (estadoExternoGenerado)
                                        {
                                            detalleRegistro.Generado = true;

                                            Logueo.Evento("[GeneraEstadoCuentaCredito] actualizando estatus cliente externo" + cultura);
                                            DataTable datosRespuesta = _daoCortes.ActualizarCorteExterno(id, conn2);
                                            if (datosRespuesta.Rows[0][0].ToString() == "0")
                                            {
                                                Logueo.Evento("[GeneraEstadoCuentaCredito] Estado de cuenta externo generado");
                                            }
                                            else
                                            {
                                                Logueo.Error("[GeneraEstadoCuentaCredito] [Error al actualizar registro externo]" + datosRespuesta.Rows[0][1]);

                                            }

                                        }
                                        else
                                        {
                                            Logueo.Error("[GeneraEstadoCuentaCredito] [Error al generar el PDF externo] [Mensaje:Errro al generar estado de cuenta]");

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logueo.Error("[GeneraEstadoCuentaCredito] [Error al generar el PDF externo] [Mensaje:Errro al generar estado de cuenta]" + ex.Message + " " + ex.StackTrace);

                                }
                                registrosProcesados++;
                                datosArchivoXLSX.Detalle.Add(detalleRegistro);
                            }

                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[GeneraEstadoCuentaCredito] [Error al generar el PDF externo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");

                        }
                        finally
                        {
                            conn2.Close();
                        }
                    }
                    Logueo.Evento("[GeneraEstadoCuentaCredito] Proceso generacion EDCTA externo finalizado" + cultura);
                }
                catch (Exception exCon)
                {
                    Logueo.Error("[GeneraEstadoCuentaCredito] [Error en cvonexion] [Mensaje: " + exCon.Message + " TRACE: " + exCon.StackTrace + "]");
                }


                if (datosArchivoXLSX.Detalle.Count > 0)
                {
                    string directorioSalidaCTA = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalidaArchivos");
                    directorioSalidaCTA = directorioSalidaCTA + "Procesados\\" + fechaFormateada + "\\";
                    foreach (DetalleArchivoXLSX detalle in datosArchivoXLSX.Detalle)
                    {
                        if (detalle.Generado)
                        {
                            datosArchivoXLSX.EstadosDeCuentaCorrectos = datosArchivoXLSX.EstadosDeCuentaCorrectos + 1;
                        }
                        else
                        {
                            datosArchivoXLSX.EstadosDeCuentaErroneos = datosArchivoXLSX.EstadosDeCuentaErroneos + 1;
                        }

                        if (detalle.Timbrado)
                        {
                            datosArchivoXLSX.EstadosDeCuentaTimbrados = datosArchivoXLSX.EstadosDeCuentaTimbrados + 1;
                        }

                    }
                    datosArchivoXLSX.EstadosDeCuentaTotales = datosArchivoXLSX.Detalle.Count;
                  //  LNCreacionExcel.escribirEnExcelDatosEdocuenta(datosArchivoXLSX, directorioSalidaCTA);
                }

            }
            catch (Exception err)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al Inicio:" + err.Message + " " + err.StackTrace);
                ApmNoticeWrapper.NoticeException(err);
            }
            Logueo.Evento("[GeneraEstadoCuentaCredito] Fin proceso corte");
            return true;
        }
    }
}
